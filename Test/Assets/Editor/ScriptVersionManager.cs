#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ScriptVersionManagerWindow : EditorWindow
{
    private string[] baseScriptFiles;
    private string[] versionScriptFiles;
    private int currentScriptIndex = 0;
    private int versionScriptIndex = 0;

    private string diffResult = "";
    private bool showDiff = true;
    private Vector2 scroll;
    private string backupScriptName = "";
    private string backupMemo = "";

    [MenuItem("Tools/Script Version Manager")]
    public static void ShowWindow()
    {
        GetWindow<ScriptVersionManagerWindow>("Script Version Manager");
    }

    private void OnEnable()
    {
        RefreshScriptList();
    }

    void OnGUI()
    {
        GUILayout.Label("🔧 スクリプトバージョン比較＆切り替え", EditorStyles.boldLabel);

        if (baseScriptFiles.Length == 0 || versionScriptFiles.Length == 0)
        {
            EditorGUILayout.HelpBox("スクリプトが見つかりません。Scripts/とScripts/Versioned/ に.csファイルを置いてください。", MessageType.Warning);
            if (GUILayout.Button("🔄 スクリプト一覧を更新"))
            {
                RefreshScriptList();
            }
            return;
        }

        EditorGUILayout.BeginHorizontal();
        currentScriptIndex = EditorGUILayout.Popup("現在のスクリプト (Scripts)", currentScriptIndex, baseScriptFiles);
        versionScriptIndex = EditorGUILayout.Popup("比較バージョン (Versioned)", versionScriptIndex, versionScriptFiles);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("🔍 差分を表示"))
        {
            ShowDiff();
        }

        if (GUILayout.Button(showDiff ? "👻 差分を非表示" : "👁️ 差分を表示"))
        {
            showDiff = !showDiff;
        }

        if (showDiff && !string.IsNullOrEmpty(diffResult))
        {
            GUILayout.Label("🧪 差分結果：", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(400));
            DrawDiffGUI(diffResult);
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("🗄️ バックアップ設定", EditorStyles.boldLabel);
        backupScriptName = EditorGUILayout.TextField("スクリプト名（任意）", backupScriptName);
        backupMemo = EditorGUILayout.TextField("📝メモ（任意）", backupMemo);

        if (GUILayout.Button("🗄️ 現在のスクリプトをバックアップ"))
        {
            BackupScript(baseScriptFiles[currentScriptIndex]);
        }
        
        if (GUILayout.Button("🔁 選択バージョンに切り替える"))
        {
            SwitchScriptVersion(baseScriptFiles[currentScriptIndex], versionScriptFiles[versionScriptIndex]);
        }

        if (GUILayout.Button("🔄 スクリプト一覧を更新"))
        {
            RefreshScriptList();
        }
    }

    void RefreshScriptList()
    {
        baseScriptFiles = GetScriptsInPath("Assets/Scripts");
        versionScriptFiles = GetScriptsInPath("Assets/Scripts/Versioned");
    }

    string[] GetScriptsInPath(string path)
    {
        if (!Directory.Exists(path)) return new string[0];

        // path以下のすべての.csファイルを取得し、相対パス付きに変換
        return Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories)
            .Select(fullPath => Path.GetRelativePath(path, fullPath)) // 相対パス化
            .Distinct()
            .ToArray();
    }
    void ShowDiff()
    {
        string basePath = $"Assets/Scripts/{baseScriptFiles[currentScriptIndex]}";
        string versionPath = Path.Combine("Assets/Scripts/Versioned", versionScriptFiles[versionScriptIndex]);

        if (!File.Exists(basePath) || !File.Exists(versionPath))
        {
            Debug.LogError("スクリプトファイルが見つかりません！");
            return;
        }

        string[] baseLines = File.ReadAllLines(basePath);
        string[] versionLines = File.ReadAllLines(versionPath);

        List<string> diffLines = new List<string>();
        int max = Mathf.Max(baseLines.Length, versionLines.Length);

        for (int i = 0; i < max; i++)
        {
            string cur = i < baseLines.Length ? baseLines[i] : "";
            string ver = i < versionLines.Length ? versionLines[i] : "";

            if (cur != ver)
            {
                diffLines.Add($"- {HighlightDiff(cur, ver)}");
                diffLines.Add($"+ {HighlightDiff(ver, cur)}");
            }
            else
            {
                diffLines.Add($"  {cur}");
            }
        }

        diffResult = string.Join("\n", diffLines);
    }

    string HighlightDiff(string lineA, string lineB)
    {
        int len = Mathf.Min(lineA.Length, lineB.Length);
        string result = "";

        for (int i = 0; i < len; i++)
        {
            if (lineA[i] != lineB[i])
                result += $"<color=yellow>{lineA[i]}</color>";
            else
                result += lineA[i];
        }

        if (lineA.Length > len)
            result += $"<color=yellow>{lineA.Substring(len)}</color>";

        return result;
    }

    void DrawDiffGUI(string diff)
    {
        var lines = diff.Split('\n');
        foreach (var line in lines)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            if (line.StartsWith("-"))
                style.normal.textColor = Color.red;
            else if (line.StartsWith("+"))
                style.normal.textColor = Color.green;
            else
                style.normal.textColor = Color.gray;
            
            style.richText = true;
            EditorGUILayout.LabelField(line, style);
        }
    }

    void BackupScript(string scriptFile)
    {
        string versionedPath = "Assets/Scripts/Versioned";
        string time = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        string originalName = Path.GetFileNameWithoutExtension(scriptFile);
        string baseName = string.IsNullOrEmpty(backupScriptName)
            ? $"{originalName}_backup_{time}"
            : backupScriptName;

        string folderPath = Path.Combine(versionedPath, baseName);
        string destScriptPath = Path.Combine(folderPath, baseName + ".cs");

        // フォルダ作成
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
            Debug.Log($"📁 バックアップフォルダ作成: {folderPath}");
        }

        string srcPath = $"Assets/Scripts/{scriptFile}";
        string content = File.ReadAllText(srcPath);
        
        content = System.Text.RegularExpressions.Regex.Replace(content, @"namespace\s+\w+", "");
        int usingEndIndex = content.LastIndexOf("using ");
        if (usingEndIndex != -1)
        {
            int insertPos = content.IndexOf("\n", usingEndIndex) + 1;
            content = content.Insert(insertPos, $"namespace Backup_{baseName} ");
        }
        else
        {
            content = $"namespace Backup_{baseName} " + content;
        }

        File.WriteAllText(destScriptPath, content);
        
        if (!string.IsNullOrEmpty(backupMemo))
        {
            string memoPath = Path.Combine(folderPath, $"{baseName}_memo.txt");
            File.WriteAllText(memoPath, backupMemo);
        }

        AssetDatabase.Refresh();
        Debug.Log($"✅ バックアップ完了: {destScriptPath}");
    }

    void SwitchScriptVersion(string targetScript, string versionScript)
    {
        if (targetScript == versionScript)
        {
            Debug.LogWarning("同じスクリプトが選ばれています。");
            return;
        }

        string targetPath = $"Assets/Scripts/{targetScript}";
        string versionPath = $"Assets/Scripts/Versioned/{versionScript}";

        if (!File.Exists(targetPath) || !File.Exists(versionPath))
        {
            Debug.LogError("スクリプトファイルが見つかりません！");
            return;
        }

        string content = File.ReadAllText(versionPath);
        
        content = System.Text.RegularExpressions.Regex.Replace(content, @"namespace\s+Backup_\w+\s*", "");
        
        string folderName = Path.GetFileName(Path.GetDirectoryName(targetPath));
        string namespaceLine = $"namespace {folderName}";
        
        if (!content.Contains("namespace"))
        {
            int lastUsingIndex = content.LastIndexOf("using ");
            if (lastUsingIndex != -1)
            {
                int insertPos = content.IndexOf("\n", lastUsingIndex) + 1;
                content = content.Insert(insertPos, namespaceLine + "\n");
            }
            else
            {
                content = namespaceLine + "\n" + content;
            }
        }

        File.WriteAllText(targetPath, content);
        AssetDatabase.Refresh();
        Debug.Log($"✅ スクリプト切り替え完了: {targetScript} ← {versionScript}");
    }

}
#endif