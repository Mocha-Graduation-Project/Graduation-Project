#region 
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
#endregion

public class QuickPrefabReplacer : EditorWindow
{
    private const string WindowTitle = "Quick Prefab Replacer";
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

    // UnityTagColorManagerのインスタンスを保持
    private UnityTagColorManager colorManager; 
    // タグ色をキャッシュするためのディクショナリ
    private Dictionary<string, Color> unityTagColors = new Dictionary<string, Color>(); 
    private Dictionary<Color, Texture2D> colorTextureCache = new Dictionary<Color, Texture2D>();

    [MenuItem("Tools/" + WindowTitle)]
    public static void ShowWindow()
    {
        GetWindow<QuickPrefabReplacer>(WindowTitle);
    }

    private void OnEnable()
    {
        // 色管理マネージャーをロード
        colorManager = UnityTagColorManager.Instance; 

        EditorApplication.hierarchyChanged += RefreshSceneObjects;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        RefreshSceneObjects();
        Selection.selectionChanged += Repaint;
        
        // OnEnable時にも色をロード
        LoadUnityTagColors(); 
        
        if (InternalEditorUtility.tags.Length > 0)
        {
            tagToAssign = InternalEditorUtility.tags[0];
        }
        
        ClearTextureCache();
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
        // プレイモードの出入りでヒエラルキーが変更されるため、色情報を再ロード
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
            // UnityTagColorManagerから色情報を取得し、同期
            unityTagColors = colorManager.SyncAndGetTagColors();
            ClearTextureCache();
        }
    }
    
    // 1x1ピクセルのテクスチャを作成し、キャッシュするヘルパーメソッド
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

    // テクスチャをクリーンアップするメソッド
    private void ClearTextureCache()
    {
        foreach (var tex in colorTextureCache.Values)
        {
            DestroyImmediate(tex);
        }
        colorTextureCache.Clear();
    }

    private void RefreshSceneObjects()
    {
        // シーン内のオブジェクトリストを再構築する前に、必ず色情報を再同期する
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
        GUILayout.Label("Prefab Quick Replacement Tool", EditorStyles.boldLabel);
        
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
            "新しく置き変えたいPrefab",//Replace Target
            targetPrefab,
            typeof(GameObject),
            false
        );

        if (targetPrefab != null && PrefabUtility.GetPrefabAssetType(targetPrefab) == PrefabAssetType.NotAPrefab)
        {
            EditorGUILayout.HelpBox("選択されたアセットは有効なPrefabではありません。", MessageType.Warning);
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
        filterOnlyRootInstances = GUILayout.Toggle(filterOnlyRootInstances, "フィルター：Prefabのみ表示");//Filter: Prefab Root Instances Only

        // Unityタグフィルタリングのドロップダウン
        List<string> filterOptions = new List<string> { "All" };
        filterOptions.AddRange(InternalEditorUtility.tags); // Unityタグリストを使用
        
        int currentFilterIndex = filterOptions.IndexOf(currentUnityTagFilter);
        int newFilterIndex = EditorGUILayout.Popup(currentFilterIndex, filterOptions.ToArray(), GUILayout.Width(100));
        
        if (newFilterIndex != currentFilterIndex)
        {
            currentUnityTagFilter = filterOptions[newFilterIndex];
            Repaint();
        }
        
        // タグの色設定をPingするボタン
        if (GUILayout.Button("色の設定", EditorStyles.miniButton, GUILayout.Width(100)))//Tag Colors
        {
            // UnityTagColorManagerアセットをインスペクタで開く
            EditorGUIUtility.PingObject(colorManager);
            Selection.activeObject = colorManager;
        }
        
        EditorGUILayout.EndHorizontal();

        // 複数選択モード
        multiSelectMode = GUILayout.Toggle(multiSelectMode, "複数選択モード", EditorStyles.toolbarButton);//Multi-Select Mode
        EditorGUILayout.Space(5);

        // オブジェクト一覧
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - 240));

        var displayObjects = sceneObjects
            .Where(go =>
            {
                if (filterOnlyRootInstances && !PrefabUtility.IsAnyPrefabInstanceRoot(go))
                {
                    return false;
                }
                
                if (currentUnityTagFilter != "All")
                {
                    // Unityタグでフィルタリング
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
        // 永続化された色情報から色を取得
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
            // Tagの色を背景として設定
            itemStyle.normal.background = MakeTex(tagBgColor);
        }
        
        // テキスト色 (ステータス)
        GUIStyle objectNameStyle = new GUIStyle(EditorStyles.label);
        objectNameStyle.normal.textColor = statusColor; 
        
        // リストアイテム全体にスタイルを適用
        EditorGUILayout.BeginHorizontal(itemStyle);
        
        // チェックボックス (選択状態)
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

        // オブジェクト名とステータス
        EditorGUILayout.LabelField(go.name, statusText, objectNameStyle, GUILayout.Width(150)); 
        
        // Unityタグをプルダウンで設定
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

        // 選択ボタン (ヒエラルキーで選択)
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
        
        GUI.enabled = count > 0;
        if (GUILayout.Button($"{count}個のオブジェクトを置き替えます", EditorStyles.miniButton, GUILayout.Width(300)))//Set Tag to {count} Obj(s)　　120
        {
            SetUnityTagToSelectedObjects(tagToAssign);
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        // 既存のPrefab置換エリア
        GUILayout.Label("Prefab Replacement", EditorStyles.miniLabel);
        GUI.enabled = targetPrefab != null && count > 0;

        string replaceText = $"Replace {count} Selected Object(s) with Prefab";
        if (GUILayout.Button(replaceText, GUILayout.Height(30)))
        {
            ReplaceSelectedObjects();
        }
        
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }
    
    // Unityのタグを設定するメソッド
    private void SetUnityTagToSelectedObjects(string tag)
    {
        if (selectedObjects.Count == 0 || string.IsNullOrEmpty(tag)) return;
        
        Undo.IncrementCurrentGroup();
        
        foreach (var go in selectedObjects)
        {
            if (go == null) continue;
            
            Undo.RecordObject(go, $"Set Tag to {tag}"); 
            go.tag = tag; 
        }
        
        Debug.Log($"[Quick Prefab Replacer] {selectedObjects.Count} objects tagged as '{tag}'.");
        
        Undo.SetCurrentGroupName($"Set Unity Tag to {selectedObjects.Count} Objects");
        Repaint();
    }

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
                Debug.LogError($"Prefabのインスタンス化に失敗しました: {targetPrefab.name}");
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