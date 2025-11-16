#region 
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
#endregion

// タグの色を永続化するためのScriptableObject
public class UnityTagColorManager : ScriptableObject
{
    [System.Serializable]
    public class TagColorData
    {
        public string tagName;
        public Color color;
    }

    [SerializeField]
    public List<TagColorData> tagColors = new List<TagColorData>();

    private static UnityTagColorManager _instance;
    private const string ManagerPath = "Assets/Editor/QuickReplacerUnityTagColorManager.asset";

    public static UnityTagColorManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = AssetDatabase.LoadAssetAtPath<UnityTagColorManager>(ManagerPath);

                if (_instance == null)
                {
                    _instance = CreateInstance<UnityTagColorManager>();
                    
                    string directory = System.IO.Path.GetDirectoryName(ManagerPath);
                    if (!System.IO.Directory.Exists(directory))
                    {
                        System.IO.Directory.CreateDirectory(directory);
                        AssetDatabase.Refresh();
                    }
                    
                    AssetDatabase.CreateAsset(_instance, ManagerPath);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"[UnityTagColorManager] New manager created at {ManagerPath}");
                }
            }
            return _instance;
        }
    }

    /// Unityの現在のタグリストに合わせてカラーリストを同期し、最新の色ディクショナリを返します。
    public Dictionary<string, Color> SyncAndGetTagColors()
    {
        string[] allUnityTags = InternalEditorUtility.tags;
        Dictionary<string, Color> currentColors = tagColors.ToDictionary(d => d.tagName, d => d.color);
        bool dirty = false;
        
        // プリセットカラー: アルファ値0.08fで視認性を確保
        Color[] presetColors = new Color[] 
        { 
            new Color(0.9f, 0.4f, 0.4f, 0.08f), // 赤系
            new Color(0.4f, 0.9f, 0.4f, 0.08f), // 緑系
            new Color(0.4f, 0.4f, 0.9f, 0.08f), // 青系
            new Color(0.9f, 0.9f, 0.4f, 0.08f), // 黄系
            new Color(0.4f, 0.9f, 0.9f, 0.08f)  // シアン系
        };
        
        int nextColorIndex = tagColors.Count(d => d.tagName != "Untagged"); 

        foreach (string tag in allUnityTags)
        {
            if (!currentColors.ContainsKey(tag))
            {
                Color newColor;
                if (tag == "Untagged")
                {
                    newColor = Color.gray * 0.2f; 
                }
                else
                {
                    newColor = presetColors[nextColorIndex % presetColors.Length];
                    nextColorIndex++;
                }

                tagColors.Add(new TagColorData { tagName = tag, color = newColor });
                currentColors.Add(tag, newColor);
                dirty = true;
            }
        }

        // 存在しないタグの色を削除
        List<TagColorData> toRemove = tagColors.Where(d => !allUnityTags.Contains(d.tagName)).ToList();
        foreach(var data in toRemove)
        {
            tagColors.Remove(data);
            dirty = true;
        }

        if (dirty)
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        return currentColors;
    }

    public void SetTagColor(string tagName, Color color)
    {
        var data = tagColors.FirstOrDefault(d => d.tagName == tagName);
        if (data != null)
        {
            data.color = color;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}