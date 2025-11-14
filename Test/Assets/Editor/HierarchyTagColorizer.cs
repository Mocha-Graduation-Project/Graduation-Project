#region
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
#endregion

[InitializeOnLoad]
public static class HierarchyTagColorizer
{
    private static Dictionary<string, Color> tagColors;

    // クラスがロードされたとき（Unity起動時やスクリプトコンパイル時）に実行
    static HierarchyTagColorizer()
    {
        // Hierarchyウィンドウのアイテム描画イベントにメソッドを登録
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        LoadTagColors();
    }
    
    /// TagColorManagerから最新の色設定をロードし、キャッシュします。
    private static void LoadTagColors()
    {
        tagColors = UnityTagColorManager.Instance.SyncAndGetTagColors();
        // 描画を強制的に更新
        EditorApplication.RepaintHierarchyWindow();
    }
    
    // Unityのメニュー項目から手動で色をリロードするオプション
    [MenuItem("Tools/Tag Colorizer/Reload Tag Colors")]
    public static void ReloadTagColorsMenuItem()
    {
        LoadTagColors();
        Debug.Log("[HierarchyTagColorizer] Tag colors reloaded.");
    }
    
    // Hierarchyウィンドウの各アイテムを描画する際に呼び出されるメソッド
    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        // instanceIDから対応するGameObjectを取得
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        // GameObjectが存在し、かつタグの色がロードされていることを確認
        if (go != null && tagColors != null)
        {
            string tag = go.tag;

            if (tagColors.TryGetValue(tag, out Color color))
            {
                if (tag == "Untagged" || color.a < 0.01f)
                {
                    return;
                }
                
                Color originalColor = GUI.backgroundColor;
                
                // 設定された色をGUIの背景色に設定
                GUI.backgroundColor = color;
                
                // アイテムの描画範囲全体を覆うようにボックスを描画
                Rect backgroundRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width + 100, selectionRect.height);
                
                // 背景を塗りつぶす
                EditorGUI.DrawRect(backgroundRect, color);
                
                // GUIの背景色を元に戻す
                GUI.backgroundColor = originalColor;
            }
        }
    }
}