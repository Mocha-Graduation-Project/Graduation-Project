#region 
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
#endregion

public class StageSupportTool : EditorWindow
{
    private const string WindowTitle = "Quick Prefab Replacer & Match Tools";
    private GameObject targetPrefab;
    private Vector2 scrollPosition;
    private List<GameObject> sceneObjects = new List<GameObject>();
    private string searchString = "";
    private HashSet<GameObject> selectedObjects = new HashSet<GameObject>();
    private bool filterOnlyRootInstances = false;
    private bool multiSelectMode = false;
    
    // Unityタグを使用
    private string currentUnityTagFilter = "All"; 
    private string tagToAssign = "Untagged"; 

    // UnityTagColorManagerのインスタンスを保持 (このコードに依存する外部クラス)
    private UnityTagColorManager colorManager; 
    // タグ色をキャッシュするためのディクショナリ
    private Dictionary<string, Color> unityTagColors = new Dictionary<string, Color>(); 
    private Dictionary<Color, Texture2D> colorTextureCache = new Dictionary<Color, Texture2D>();

    [MenuItem("Tools/Quick Prefab Replacer & Match Tools")]
    public static void ShowWindow()
    {
        GetWindow<StageSupportTool>(WindowTitle);
    }

    // MatchSizeEditor から移動したメニュー項目 
    // 既存のメニュー項目は維持しつつ、処理をこのクラスに移管します
    [MenuItem("GameObject/Match/サイズを合わせる")]//Size
    private static void MatchScaleMenu() => MatchScale(Selection.gameObjects);
    
    [MenuItem("GameObject/Match/Y座標を合わせる")]//YTransform
    private static void FlattenYPositionMenu() => FlattenPosition(Selection.gameObjects, Axis.Y);

    [MenuItem("GameObject/Match/X座標を合わせる")]//XTransform
    private static void FlattenXPositionMenu() => FlattenPosition(Selection.gameObjects, Axis.X);

    [MenuItem("GameObject/Match/Z座標を合わせる")]//ZTransform
    private static void FlattenZPositionMenu() => FlattenPosition(Selection.gameObjects, Axis.Z);
    // ---

    private void OnEnable()
    {
        // 外部クラスに依存するため、存在チェック
        colorManager = UnityTagColorManager.Instance; 

        EditorApplication.hierarchyChanged += RefreshSceneObjects;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        Selection.selectionChanged += Repaint;
        
        // OnEnable時にも色をロード
        LoadUnityTagColors(); 
        
        if (InternalEditorUtility.tags.Length > 0)
        {
            tagToAssign = InternalEditorUtility.tags[0];
        }
        
        ClearTextureCache();
        RefreshSceneObjects(); // 最後に呼び出す
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= RefreshSceneObjects;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        Selection.selectionChanged -= Repaint;
        
        ClearTextureCache();
    }
    
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
        {
            LoadUnityTagColors();
        }
    }

    // タグと色の情報をScriptableObjectからロード/同期する
    private void LoadUnityTagColors()
    {
        if (colorManager != null)
        {
            unityTagColors = colorManager.SyncAndGetTagColors();
            ClearTextureCache();
        }
    }
    
    private Texture2D MakeTex(Color col)
    {
        if (colorTextureCache.ContainsKey(col))
        {
            return colorTextureCache[col];
        }
        
        Texture2D result = new Texture2D(1, 1);
        result.SetPixel(0, 0, col);
        result.Apply();
        
        colorTextureCache.Add(col, result);
        return result;
    }

    private void ClearTextureCache()
    {
        foreach (var tex in colorTextureCache.Values)
        {
            if (tex != null)
            {
                DestroyImmediate(tex);
            }
        }
        colorTextureCache.Clear();
    }

    private void RefreshSceneObjects()
    {
        LoadUnityTagColors(); 

        var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        sceneObjects.Clear();
        foreach (var root in rootObjects)
        {
            sceneObjects.Add(root);
            sceneObjects.AddRange(root.GetComponentsInChildren<Transform>(true)
                .Where(t => t.gameObject != root)
                .Select(t => t.gameObject));
        }

        sceneObjects = sceneObjects
            .Where(go => go != null) 
            .OrderBy(go => go.name)  
            .ToList();

        Repaint();
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Quick Replacement Tool & Match Tools", EditorStyles.boldLabel);
        
        DrawTargetPrefabArea();
        DrawObjectListArea();
        DrawActionArea();

        HandleSelectionSync();
    }

    private void DrawTargetPrefabArea()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("1. Target Prefab", EditorStyles.miniLabel);

        targetPrefab = (GameObject)EditorGUILayout.ObjectField(
            "NewReplacePrefab",
            targetPrefab,
            typeof(GameObject),
            false
        );

        if (targetPrefab != null && PrefabUtility.GetPrefabAssetType(targetPrefab) == PrefabAssetType.NotAPrefab)
        {
            EditorGUILayout.HelpBox("選択されたアセットは有効なPrefabではありません。", MessageType.Warning);//The selected asset is not a valid Prefab.
            targetPrefab = null;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void DrawObjectListArea()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label($"2. Scene Objects ({sceneObjects.Count} Total)", EditorStyles.miniLabel);

        // 検索ボックス
        EditorGUILayout.BeginHorizontal();
        searchString = EditorGUILayout.TextField("", searchString, "SearchTextField");
        if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50)))
        {
            searchString = "";
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();

        // フィルタリングと複数選択モードトグル
        EditorGUILayout.BeginHorizontal();
        filterOnlyRootInstances = GUILayout.Toggle(filterOnlyRootInstances, "フィルター：Prefabのみ表示");//Filter: Show only prefabs

        // Unityタグフィルタリングのドロップダウン
        List<string> filterOptions = new List<string> { "All" };
        filterOptions.AddRange(InternalEditorUtility.tags); 
        
        int currentFilterIndex = filterOptions.IndexOf(currentUnityTagFilter);
        int newFilterIndex = EditorGUILayout.Popup(currentFilterIndex, filterOptions.ToArray(), GUILayout.Width(100));
        
        if (newFilterIndex != currentFilterIndex)
        {
            currentUnityTagFilter = filterOptions[newFilterIndex];
            Repaint();
        }
        
        // タグの色設定をPingするボタン
        if (GUILayout.Button("Color Settings", EditorStyles.miniButton, GUILayout.Width(100)))//色の設定
        {
            if (colorManager != null)
            {
                EditorGUIUtility.PingObject(colorManager);
                Selection.activeObject = colorManager;
            }
            else
            {
                Debug.LogWarning("UnityTagColorManager not found.");//UnityTagColorManagerが見つかりません。
            }
        }
        
        EditorGUILayout.EndHorizontal();

        // 複数選択モード
        multiSelectMode = GUILayout.Toggle(multiSelectMode, "複数選択モード", EditorStyles.toolbarButton);//Multiple Selection Mode
        EditorGUILayout.Space(5);

        // オブジェクト一覧
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - 340)); // Adjust height

        var displayObjects = sceneObjects
            .Where(go =>
            {
                if (filterOnlyRootInstances && !PrefabUtility.IsAnyPrefabInstanceRoot(go))
                {
                    return false;
                }
                
                if (currentUnityTagFilter != "All")
                {
                    if (go.tag != currentUnityTagFilter)
                    {
                        return false;
                    }
                }
                
                return string.IsNullOrEmpty(searchString) || go.name.ToLower().Contains(searchString.ToLower());
            })
            .ToList();

        // リスト表示
        foreach (var go in displayObjects)
        {
            DrawObjectItem(go);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void DrawObjectItem(GameObject go)
    {
        bool isSelected = selectedObjects.Contains(go);
        string currentUnityTag = go.tag; 
        Color tagBgColor;
        
        if (!unityTagColors.TryGetValue(currentUnityTag, out tagBgColor))
        {
            tagBgColor = Color.gray * 0.5f;
        }

        // Prefab Status Logic 
        PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(go);
        string statusText;
        Color statusColor;
        bool isRootInstance = PrefabUtility.IsAnyPrefabInstanceRoot(go);

        if (isRootInstance)
        {
            statusText = "Prefab Root";
            statusColor = new Color(0.3f, 0.7f, 1f);
        }
        else
        {
            switch (status)
            {
                case PrefabInstanceStatus.Connected:
                    statusText = "Prefab Instance";
                    statusColor = new Color(0.6f, 0.6f, 0.6f);
                    break;
                case PrefabInstanceStatus.Disconnected:
                    statusText = "Disconnected";
                    statusColor = new Color(1f, 0.5f, 0.5f);
                    break;
                default: 
                    statusText = "Normal Object";
                    statusColor = Color.white;
                    break;
            }
        }
        
        // 背景色を描画するためのスタイルを取得
        GUIStyle itemStyle = new GUIStyle("CN Box"); 
        
        if (isSelected)
        {
            itemStyle = new GUIStyle("SelectionRect"); 
        }
        else
        {
            itemStyle.normal.background = MakeTex(tagBgColor);
        }
        
        GUIStyle objectNameStyle = new GUIStyle(EditorStyles.label);
        objectNameStyle.normal.textColor = statusColor; 
        
        EditorGUILayout.BeginHorizontal(itemStyle);
        
        bool newSelectionState = EditorGUILayout.Toggle(isSelected, GUILayout.Width(15));
        if (newSelectionState != isSelected)
        {
            if (multiSelectMode || Event.current.control || Event.current.command)
            {
                if (newSelectionState) selectedObjects.Add(go);
                else selectedObjects.Remove(go);
            }
            else
            {
                selectedObjects.Clear();
                if (newSelectionState) selectedObjects.Add(go);
            }
            Selection.objects = selectedObjects.ToArray();
        }

        EditorGUILayout.LabelField(go.name, statusText, objectNameStyle, GUILayout.Width(150)); 
        
        string[] availableTags = InternalEditorUtility.tags;
        int currentIndex = System.Array.IndexOf(availableTags, currentUnityTag);
        
        int newIndex = EditorGUILayout.Popup(currentIndex >= 0 ? currentIndex : 0, availableTags, GUILayout.ExpandWidth(true));
        
        if (newIndex != currentIndex)
        {
            string newTag = availableTags[newIndex];
            
            Undo.RecordObject(go, $"Set Tag on {go.name}"); 
            go.tag = newTag; 
            
            Repaint();
        }

        if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(40)))
        {
            EditorGUIUtility.PingObject(go);
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawActionArea()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("3. Actions", EditorStyles.boldLabel);

        int count = selectedObjects.Count;
        
        // --- Prefab Replacement ---
        GUILayout.Label("Prefab Replacement", EditorStyles.miniLabel);
        
        GUI.enabled = targetPrefab != null && count > 0;

        string replaceText = $"Replace {count} Selected Object(s) with Prefab";
  
        if (GUILayout.Button(replaceText, GUILayout.Height(30)))
        {
            ReplaceSelectedObjects();
        }
        
        GUI.enabled = true;

        EditorGUILayout.Space();

        // --- Match Tools ---
        GUILayout.Label("Match Tools (Requires 2+ Objects)", EditorStyles.miniLabel);
        GUI.enabled = count >= 2;
        
        // サイズ合わせ
        if (GUILayout.Button("最初に選んだオブジェクトのサイズに合わせる", GUILayout.Height(20)))//Match Scale (1st object's scale)
        {
            MatchScale(selectedObjects.ToArray());
        }

        EditorGUILayout.BeginHorizontal();
        // Y座標合わせ
        if (GUILayout.Button("Y座標を合わせる", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))//Match Y Position
        {
            FlattenPosition(selectedObjects.ToArray(), Axis.Y);
        }
        // X座標合わせ
        if (GUILayout.Button("X座標を合わせる", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))//Match X Position
        {
            FlattenPosition(selectedObjects.ToArray(), Axis.X);
        }
        // Z座標合わせ
        if (GUILayout.Button("Z座標を合わせる", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))//Match Z Position
        {
            FlattenPosition(selectedObjects.ToArray(), Axis.Z);
        }
        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;
        
        EditorGUILayout.EndVertical();
    }
    
    // MatchSizeEditor から移動したメソッド
    private static void MatchScale(GameObject[] selectedObjects)
    {
        if (selectedObjects == null || selectedObjects.Length < 2)
        {
            Debug.LogWarning("サイズを合わせるには、少なくとも2つのオブジェクトを選択してください。");//To adjust the size, select at least two objects.
            return;
        }

        // 最初のオブジェクトを基準（コピー元）とする
        Transform sourceTransform = selectedObjects[0].transform;
        Vector3 targetScale = sourceTransform.localScale;

        // 2つ目以降のオブジェクトのスケールを変更
        for (int i = 1; i < selectedObjects.Length; i++)
        {
            Transform targetTransform = selectedObjects[i].transform;
            
            Undo.RecordObject(targetTransform, "Match Scale");
            
            targetTransform.localScale = targetScale;
        }
        Debug.Log($"[Quick Prefab Replacer] Scale matched on {selectedObjects.Length - 1} object(s) based on '{selectedObjects[0].name}'.");
    }

    private enum Axis { X, Y, Z }

    private static void FlattenPosition(GameObject[] selectedObjects, Axis axis)
    {
        if (selectedObjects == null || selectedObjects.Length < 2)
        {
            Debug.LogWarning("座標を合わせるには、少なくとも2つのオブジェクトを選択してください。");//To align coordinates, select at least two objects.
            return;
        }

        // 最初のオブジェクトを基準（コピー元）とする
        Transform sourceTransform = selectedObjects[0].transform;
        float targetValue = 0f;
        string axisName = "";

        switch (axis)
        {
            case Axis.X:
                targetValue = sourceTransform.position.x;
                axisName = "X";
                break;
            case Axis.Y:
                targetValue = sourceTransform.position.y;
                axisName = "Y";
                break;
            case Axis.Z:
                targetValue = sourceTransform.position.z;
                axisName = "Z";
                break;
        }

        // 2つ目以降のオブジェクトの座標を変更
        for (int i = 1; i < selectedObjects.Length; i++)
        {
            Transform targetTransform = selectedObjects[i].transform;
            
            Undo.RecordObject(targetTransform, $"Flatten {axisName} Position");
            
            Vector3 newPosition = targetTransform.position;
            switch (axis)
            {
                case Axis.X:
                    newPosition.x = targetValue;
                    break;
                case Axis.Y:
                    newPosition.y = targetValue;
                    break;
                case Axis.Z:
                    newPosition.z = targetValue;
                    break;
            }
            targetTransform.position = newPosition;
        }
        Debug.Log($"[Quick Prefab Replacer] {axisName} Position matched on {selectedObjects.Length - 1} object(s) based on '{selectedObjects[0].name}'.");
    }
    // ---

    private void ReplaceSelectedObjects()
    {
        if (targetPrefab == null || selectedObjects.Count == 0) return;

        Undo.IncrementCurrentGroup();
        
        List<GameObject> objectsToDestroy = new List<GameObject>();
        List<Object> newSelection = new List<Object>(); 

        var targets = selectedObjects.ToList();
        
        foreach (var original in targets)
        {
            if (original == null) continue;

            Transform parent = original.transform.parent;
            Vector3 position = original.transform.position;
            Quaternion rotation = original.transform.rotation;
            Vector3 scale = original.transform.localScale;
            
            // 元のオブジェクトのUnityタグを取得
            string originalTag = original.tag;

            GameObject newInstance = (GameObject)PrefabUtility.InstantiatePrefab(targetPrefab);

            if (newInstance == null)
            {
                Debug.LogError($"Prefabのインスタンス化に失敗しました: {targetPrefab.name}");//Failed to instantiate the Prefab.
                continue;
            }
            
            Undo.RegisterCreatedObjectUndo(newInstance, "Replace Object");
            
            Transform newT = newInstance.transform;
            newT.SetParent(parent);
            newT.position = position;
            newT.rotation = rotation;
            newT.localScale = scale;
            
            // 新しいオブジェクトに元のUnityタグを設定
            newInstance.tag = originalTag;

            objectsToDestroy.Add(original);
            newSelection.Add(newInstance);
        }
        
        foreach (var original in objectsToDestroy)
        {
            Undo.DestroyObjectImmediate(original);
        }

        Selection.objects = newSelection.ToArray();
        selectedObjects = new HashSet<GameObject>(newSelection.Cast<GameObject>());
        
        RefreshSceneObjects();

        Debug.Log($"[Quick Prefab Replacer] {objectsToDestroy.Count} objects replaced successfully with '{targetPrefab.name}'.");
        
        Undo.SetCurrentGroupName($"Replace {objectsToDestroy.Count} Objects");
    }

    private void HandleSelectionSync()
    {
        var currentSelection = Selection.gameObjects;
        
        if (!selectedObjects.SetEquals(currentSelection))
        {
            selectedObjects.Clear();
            foreach (var go in currentSelection)
            {
                selectedObjects.Add(go);
            }
            Repaint();
        }
    }
}