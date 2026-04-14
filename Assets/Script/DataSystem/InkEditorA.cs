using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class InkEditorA : EditorWindow
{
    private string mainFilePath;
    private Dictionary<string, List<string>> fileData = new Dictionary<string, List<string>>();
    private Dictionary<string, List<KnotData>> fileKnots = new Dictionary<string, List<KnotData>>();

    private string selectedFile;
    private int selectedKnotIndex = -1;
    private Vector2 fileScroll, knotScroll, editorScroll;

    private Dictionary<string, string[]> tagDefinitions = new Dictionary<string, string[]>() {
        { "PlayBGM", new[] { "路徑", "音量" } },
        { "ChangeSprite", new[] { "角色ID", "表情" } },
        { "Shake", new[] { "強度", "時間" } }
    };

    struct KnotData { public string name; public int startLine, endLine; }

    [MenuItem("Tools/Ink 極簡緊湊編輯器")]
    public static void ShowWindow() => GetWindow<InkEditorA>("Ink Compact Editor");

    private void OnGUI()
    {
        DrawDropArea();
        if (fileData.Count == 0) { EditorGUILayout.HelpBox("請拖入 .ink 檔案。", MessageType.Info); return; }

        EditorGUILayout.BeginHorizontal();
        DrawSidebar();
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        DrawEditor();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSidebar()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(180));
        GUILayout.Label("📁 檔案清單", EditorStyles.boldLabel);
        fileScroll = EditorGUILayout.BeginScrollView(fileScroll, GUILayout.Height(100));
        foreach (var path in fileData.Keys)
        {
            if (GUILayout.Toggle(selectedFile == path, Path.GetFileName(path), "Button"))
            {
                if (selectedFile != path) { selectedFile = path; selectedKnotIndex = -1; }
            }
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Label("🌿 分支", EditorStyles.boldLabel);
        if (GUILayout.Toggle(selectedKnotIndex == -1, "全部顯示", "Button")) selectedKnotIndex = -1;
        knotScroll = EditorGUILayout.BeginScrollView(knotScroll);
        if (!string.IsNullOrEmpty(selectedFile) && fileKnots.ContainsKey(selectedFile))
        {
            var knots = fileKnots[selectedFile];
            for (int i = 0; i < knots.Count; i++)
                if (GUILayout.Toggle(selectedKnotIndex == i, knots[i].name, "Button")) selectedKnotIndex = i;
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawEditor()
    {
        if (string.IsNullOrEmpty(selectedFile)) return;

        // --- 極簡樣式設定 ---
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;
        textAreaStyle.fontSize = 12; // 縮小一點字體
        textAreaStyle.padding = new RectOffset(4, 4, 4, 4); // 縮減內邊距

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label($"{Path.GetFileName(selectedFile)}");
        if (GUILayout.Button("💾 儲存", EditorStyles.toolbarButton, GUILayout.Width(50))) SaveAll();
        GUILayout.EndHorizontal();

        editorScroll = EditorGUILayout.BeginScrollView(editorScroll);
        var lines = fileData[selectedFile];
        int start = 0, end = lines.Count;
        if (selectedKnotIndex != -1) { start = fileKnots[selectedFile][selectedKnotIndex].startLine; end = fileKnots[selectedFile][selectedKnotIndex].endLine; }

        float viewWidth = position.width - 240;

        for (int i = start; i < end; i++)
        {
            // 使用 HelpBox 取代 Window，高度會更緊湊
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            // 行號與分支標記
            string content = lines[i].Split('#')[0].TrimEnd();
            var lineNumStyle = new GUIStyle(EditorStyles.miniLabel) { fixedWidth = 25 };
            if (content.Trim().StartsWith("==")) lineNumStyle.normal.textColor = Color.yellow;
            GUILayout.Label($"{i + 1}", lineNumStyle);

            // 計算高度：現在最小高度設為 20 (約一行高)
            float h = textAreaStyle.CalcHeight(new GUIContent(content), viewWidth);
            float finalH = Mathf.Max(20, h);

            string newContent = EditorGUILayout.TextArea(content, textAreaStyle, GUILayout.Height(finalH), GUILayout.ExpandWidth(true));

            if (GUILayout.Button("+", GUILayout.Width(22))) ShowTagMenu(i);
            EditorGUILayout.EndHorizontal();

            // 標籤列表
            List<string> tagList = ParseTagsFromLine(lines[i]);
            int tagToDelete = -1;
            if (tagList.Count > 0)
            {
                for (int t = 0; t < tagList.Count; t++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
                    EditorGUILayout.BeginHorizontal("box");
                    GUI.backgroundColor = Color.white;

                    string rawTag = tagList[t];
                    string cmd = rawTag.Contains(":") ? rawTag.Split(':')[0] : rawTag;
                    string[] args = rawTag.Contains(":") ? rawTag.Split(':')[1].Split(',') : new string[0];

                    GUILayout.Label($"#{cmd}", EditorStyles.miniBoldLabel, GUILayout.Width(70));

                    if (tagDefinitions.ContainsKey(cmd))
                    {
                        string[] schema = tagDefinitions[cmd];
                        for (int j = 0; j < schema.Length; j++)
                        {
                            EditorGUILayout.LabelField(schema[j], EditorStyles.miniLabel, GUILayout.Width(35));
                            if (args.Length <= j) System.Array.Resize(ref args, j + 1);
                            args[j] = EditorGUILayout.TextField(args[j] ?? "", GUILayout.MinWidth(40), GUILayout.Height(16));
                        }
                        tagList[t] = $"{cmd}:{string.Join(",", args)}";
                    }
                    else
                    {
                        string newVal = EditorGUILayout.TextField(rawTag.Contains(":") ? rawTag.Split(':')[1] : "", GUILayout.Height(16));
                        tagList[t] = string.IsNullOrEmpty(newVal) ? cmd : $"{cmd}:{newVal}";
                    }

                    if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(18))) tagToDelete = t;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (tagToDelete != -1) tagList.RemoveAt(tagToDelete);
            string finalLine = newContent;
            foreach (var t in tagList) finalLine += " #" + t;
            if (lines[i] != finalLine)
            {
                lines[i] = finalLine;
                if (finalLine.Contains("==")) ParseKnots(selectedFile);
            }

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }

    private List<string> ParseTagsFromLine(string line)
    {
        List<string> tags = new List<string>();
        MatchCollection matches = Regex.Matches(line, @"#([^#]+)");
        foreach (Match m in matches) tags.Add(m.Groups[1].Value.Trim());
        return tags;
    }

    private void ShowTagMenu(int lineIndex)
    {
        GenericMenu menu = new GenericMenu();
        foreach (var def in tagDefinitions)
        {
            menu.AddItem(new GUIContent(def.Key), false, () => {
                fileData[selectedFile][lineIndex] += $" #{def.Key}:";
                Repaint();
            });
        }
        menu.ShowAsContext();
    }

    // --- 基礎邏輯 (讀檔、掃描) 保持不變 ---
    private void DrawDropArea()
    {
        Event evt = Event.current;
        if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && new Rect(0, 0, position.width, position.height).Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (path.ToLower().EndsWith(".ink")) { mainFilePath = Path.GetFullPath(path); ScanIncludes(mainFilePath); break; }
                }
            }
            evt.Use();
        }
    }

    private void ScanIncludes(string path)
    {
        fileData.Clear(); fileKnots.Clear(); LoadRecursive(path); selectedFile = path; selectedKnotIndex = -1; Repaint();
    }

    private void LoadRecursive(string path)
    {
        if (fileData.ContainsKey(path) || !File.Exists(path)) return;
        string[] rawLines = File.ReadAllLines(path);
        fileData.Add(path, new List<string>(rawLines));
        ParseKnots(path);
        foreach (string line in rawLines)
        {
            Match m = Regex.Match(line, @"^\s*INCLUDE\s+(.+)");
            if (m.Success) LoadRecursive(Path.Combine(Path.GetDirectoryName(path), m.Groups[1].Value.Trim()));
        }
    }

    private void ParseKnots(string path)
    {
        if (!fileData.ContainsKey(path)) return;
        var lines = fileData[path]; List<KnotData> knots = new List<KnotData>();
        for (int i = 0; i < lines.Count; i++)
        {
            Match m = Regex.Match(lines[i], @"^\s*==+\s*([a-zA-Z0-9_]+)");
            if (m.Success)
            {
                if (knots.Count > 0) { var last = knots[knots.Count - 1]; last.endLine = i; knots[knots.Count - 1] = last; }
                knots.Add(new KnotData { name = m.Groups[1].Value, startLine = i, endLine = lines.Count });
            }
        }
        fileKnots[path] = knots;
    }

    private void SaveAll()
    {
        foreach (var kvp in fileData) File.WriteAllLines(kvp.Key, kvp.Value.ToArray());
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("成功", "標籤已拆分儲存！", "OK");
    }
}