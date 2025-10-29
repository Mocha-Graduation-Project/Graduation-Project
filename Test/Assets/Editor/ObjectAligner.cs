using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ObjectAligner : EditorWindow
{
    private Direction alignDirection = Direction.Right;
    private List<GameObject> selectedObjects = new List<GameObject>();
    private Vector2 scrollPos;

    private enum Direction {
        Right,
        Left
    }

    [MenuItem("Tools/整列")]//Tools/Object Aligner
    public static void ShowWindow()
    {
        GetWindow<ObjectAligner>("整列");//Object Aligner
    }

    private void OnSelectionChange()
    {
        // 選択されたオブジェクトのリストを更新
        selectedObjects = Selection.gameObjects.ToList();
        // NullのGameObjectを除去
        selectedObjects.RemoveAll(go => go == null);
        Repaint(); // GUIを再描画してリストを更新
    }

    private void OnGUI()
    {
        GUILayout.Label("オブジェクト整列ツール", EditorStyles.boldLabel);//Sequential Object Alignment Tool
        
        // 配置方向の選択
        alignDirection = (Direction)EditorGUILayout.EnumPopup("", alignDirection);//Align Direction

        EditorGUILayout.Space();

        // 選択されたオブジェクトのリスト表示
        EditorGUILayout.LabelField($"選択中のオブジェクト ({selectedObjects.Count}):", EditorStyles.miniBoldLabel);//Selected Objects
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));
        if (selectedObjects.Count > 0)
        {
            for (int i = 0; i < selectedObjects.Count; i++)
            {
                if (selectedObjects[i] != null)
                {
                    EditorGUILayout.ObjectField($"[{i + 1}]", selectedObjects[i], typeof(GameObject), true);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("シーンで整列したいオブジェクトを複数選択してください。", MessageType.Info);
        }
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space();

        // 整列ボタン
        GUI.enabled = selectedObjects.Count >= 2;
        if (GUILayout.Button("選択したオブジェクトを順番に整列"))//Sequentially Align Selected Objects
        {
            AlignSelectedObjectsSequentially();
        }
        GUI.enabled = true;
    }

    private void AlignSelectedObjectsSequentially()
    {
        if (selectedObjects.Count < 2)
        {
            Debug.LogError("整列には最低2つのオブジェクトを選択してください。");
            return;
        }

        // Undoグループを開始
        Undo.SetCurrentGroupName("Sequentially Align Objects");
        int groupIndex = Undo.GetCurrentGroup();

        // 最初のオブジェクトは動かさない（基準とする）
        // 2番目 (i=1) から最後のオブジェクトまで順に整列させる
        for (int i = 1; i < selectedObjects.Count; i++)
        {
            GameObject anchorObject = selectedObjects[i - 1]; // 基準となるオブジェクト
            GameObject targetObject = selectedObjects[i];     // 動かすオブジェクト

            if (anchorObject == null || targetObject == null)
            {
                Debug.LogWarning($"選択リストのオブジェクト {i} または {i + 1} がNullです。スキップします。");
                continue;
            }
            Bounds boundsAnchor = GetObjectBounds(anchorObject);
            Bounds boundsTarget = GetObjectBounds(targetObject);

            if (boundsAnchor.size == Vector3.zero || boundsTarget.size == Vector3.zero)
            {
                Debug.LogWarning($"オブジェクト {anchorObject.name} または {targetObject.name} に有効なRenderer/Colliderがありません。スキップします。");
                continue;
            }

            // targetObjectの新しいローカル座標
            Vector3 newPosition = targetObject.transform.position;

            // anchorObjectの整列基準X座標を取得
            float anchorEdgeX = (alignDirection == Direction.Right) 
                                 ? boundsAnchor.max.x // 右端
                                 : boundsAnchor.min.x; // 左端

            // targetObjectを接触させるためのオフセットを計算
            float targetObjectEdgeX;
            float targetCenterToEdgeOffset;

            if (alignDirection == Direction.Right)
            {
                // anchorObjectの右端にtargetObjectの左端を合わせる
                targetObjectEdgeX = boundsTarget.min.x; 
            }
            else // Direction.Left
            {
                // anchorObjectの左端にtargetObjectの右端を合わせる
                targetObjectEdgeX = boundsTarget.max.x;
            }
            
            // オブジェクトの中心から、接触させる側の端までの距離
            targetCenterToEdgeOffset = targetObjectEdgeX - targetObject.transform.position.x;

            // 新しいX座標の計算
            // anchorObjectの端 + targetObjectの中心までのオフセット
            newPosition.x = anchorEdgeX - targetCenterToEdgeOffset;

            // Y軸とZ軸は、targetObjectの中心に合わせて整列
            newPosition.y = anchorObject.transform.position.y;
            newPosition.z = anchorObject.transform.position.z;

            // 変更をUndo可能にするために登録
            Undo.RecordObject(targetObject.transform, "Align Object " + targetObject.name);

            // targetObjectの位置を更新
            targetObject.transform.position = newPosition;
        }

        // Undoグループを終了
        Undo.CollapseUndoOperations(groupIndex);
    }

    /// オブジェクトのRendererまたはColliderからBoundsを取得
    private Bounds GetObjectBounds(GameObject go)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }
        
        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds;
        }

        // 子オブジェクトを含めたBoundsを取得する
        // Bounds bounds = new Bounds(go.transform.position, Vector3.zero);
        // bool hasBounds = false;
        // foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        // {
        //     if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
        //     else { bounds.Encapsulate(r.bounds); }
        // }
        // if (hasBounds) return bounds;

        return new Bounds(go.transform.position, Vector3.zero);
    }
}