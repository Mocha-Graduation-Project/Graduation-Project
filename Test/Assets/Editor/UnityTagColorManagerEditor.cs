using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnityTagColorManager))]
public class UnityTagColorManagerEditor : Editor
{
    // カスタムインスペクターの描画
    public override void OnInspectorGUI()
    {
        // 元の描画（タグと色のリスト）
        base.OnInspectorGUI();

        // 区切り線
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Tag Synchronization", EditorStyles.boldLabel);
        
        // ターゲットのマネージャースクリプトを取得
        UnityTagColorManager manager = (UnityTagColorManager)target;

        // タグの同期ボタン
        if (GUILayout.Button("Sync Tags Now"))
        {
            // SyncAndGetTagColors()を呼び出してタグリストを強制的に更新
            manager.SyncAndGetTagColors();
            
            // 変更をアセットに保存
            EditorUtility.SetDirty(manager);
            AssetDatabase.SaveAssets();

            Debug.Log("[UnityTagColorManager] Tag list synchronization complete.");
        }
        
        // Hierarchyの色を再描画するボタン (HierarchyTagColorizerで作成したLoadTagColorsを呼び出す)
        if (GUILayout.Button("Repaint Hierarchy (Apply New Colors)"))
        {
            // Hierarchyの色を更新するためにウィンドウの再描画を要求
            EditorApplication.RepaintHierarchyWindow();
            Debug.Log("[UnityTagColorManager] Hierarchy window repainted.");
        }
    }
}