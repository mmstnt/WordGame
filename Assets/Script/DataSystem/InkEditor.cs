using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Burst.Intrinsics.X86.Avx;

public class InkEditor : EditorWindow
{
    private string mainFilePath; 
    private Dictionary<string, List<string>> fileData = new Dictionary<string, List<string>>();
    private Dictionary<string, List<KnotData>> fileKnots = new Dictionary<string, List<KnotData>>();

    private string selectFile;
    private int selectKnotIndex = -1;
    private Vector2 fileScroll;
    private Vector2 knotScroll;
    private Vector2 editorScroll;

    private Dictionary<string, string[]> tagCommand = new Dictionary<string, string[]>() {
        { "backgroung", new[] { "背景ID" } },
        { "name", new[] { "對話名稱" } },
        { "show", new[] { "角色物件", "立繪ID", "X座標", "Y座標", "方向" } },
        { "high", new[] { "角色物件", } },
        { "exit", new[] { "角色物件" } },
        { "battle", new[] { "戰鬥ID" } }
    };

    struct KnotData 
    { 
        public string name;
        public int startLine;
        public int endLine;
    }

    [MenuItem("Tools/Ink劇本編輯器")]
    public static void showWindow() => GetWindow<InkEditor>("Ink劇本編輯器");

    private void OnGUI()
    {
        drawDropArea();
        if (fileData.Count == 0) 
        { 
            EditorGUILayout.HelpBox("請拖入 .ink 檔案。", MessageType.Info);
            return; 
        }

        EditorGUILayout.BeginHorizontal();
        drawSidebar();
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        DrawEditor();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }


    private void drawSidebar()
    {
        //繪製檔案
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(160));
        GUILayout.Label("檔案清單", EditorStyles.boldLabel);
        fileScroll = EditorGUILayout.BeginScrollView(fileScroll, GUILayout.Height(160));

        foreach (string path in fileData.Keys)
        {
            if (GUILayout.Toggle(selectFile == path, Path.GetFileName(path), "Button"))
            {
                if (selectFile != path) 
                {
                    selectFile = path; selectKnotIndex = -1; 
                }
            }
        }

        EditorGUILayout.EndScrollView();

        //繪製節點分支
        GUILayout.Label("節點分支", EditorStyles.boldLabel);

        if (GUILayout.Toggle(selectKnotIndex == -1, "全部顯示", "Button"))
        { 
            selectKnotIndex = -1; 
        }
        knotScroll = EditorGUILayout.BeginScrollView(knotScroll);

        if (!string.IsNullOrEmpty(selectFile) && fileKnots.ContainsKey(selectFile))
        {
            List<KnotData> knots = fileKnots[selectFile];
            for (int i = 0; i < knots.Count; i++)
            {
                if (GUILayout.Toggle(selectKnotIndex == i, knots[i].name, "Button"))
                {
                    selectKnotIndex = i;
                }
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawEditor()
    {
        if (string.IsNullOrEmpty(selectFile)) return;

        //設定字型
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;
        textAreaStyle.stretchWidth = true;
        textAreaStyle.clipping = TextClipping.Clip;
        textAreaStyle.fontSize = 12;
        textAreaStyle.padding = new RectOffset(4, 4, 4, 4);

        //繪製當前文件名稱和儲存按鈕
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label($"{Path.GetFileName(selectFile)}");
        if (GUILayout.Button("儲存", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            SaveAll();
        }
        GUILayout.EndHorizontal();

        //繪製文字內容框
        editorScroll = EditorGUILayout.BeginScrollView(editorScroll);
        List<string> lineList = fileData[selectFile];
        int start = (selectKnotIndex != -1) ? start = fileKnots[selectFile][selectKnotIndex].startLine : 0;
        int end = (selectKnotIndex != -1) ? end = fileKnots[selectFile][selectKnotIndex].endLine : lineList.Count;

        int indexToMoveUp = -1;
        int indexToMoveDown = -1;
        int indexToAddAfter = -1;
        int indexToDelete = -1;

        float viewWidth = Mathf.Max(430, position.width - 360);

        //繪製文字內容
        for (int i = start; i < end; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            //繪製每行基礎按鈕
            EditorGUILayout.BeginHorizontal();

            //將文字和標籤分開
            string content = lineList[i].Split('#')[0].TrimEnd();
            string nameText = content.Contains(":") ? content.Split(":")[0].TrimEnd() : "";
            string dialogText = content.Contains(":") ? content.Split(":")[1].TrimEnd() : content;

            //節點變黃色
            GUIStyle lineNumStyle = new GUIStyle(EditorStyles.miniLabel) { fixedWidth = 25 };
            if (content.Trim().StartsWith("=="))
            {
                lineNumStyle.normal.textColor = Color.yellow;
            }
            GUILayout.Label($"{i + 1}", lineNumStyle);

            //上下移動按鈕
            GUI.enabled = i > start; //第一行不能往上
            if (GUILayout.Button("▲", EditorStyles.miniButtonLeft, GUILayout.Width(20))) indexToMoveUp = i;
            GUI.enabled = i < end - 1; //最後一行不能往下
            if (GUILayout.Button("▼", EditorStyles.miniButtonRight, GUILayout.Width(20))) indexToMoveDown = i;
            GUI.enabled = true;

            GUILayout.Space(5);

            //插入按鈕
            if (GUILayout.Button("+ 插入", EditorStyles.miniButton, GUILayout.Width(45))) indexToAddAfter = i;

            //刪除按鈕
            GUI.contentColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("刪除", EditorStyles.miniButton, GUILayout.Width(40))) indexToDelete = i;
            GUI.contentColor = Color.white;

            EditorGUILayout.EndHorizontal();

            //繪製每行內容
            EditorGUILayout.BeginHorizontal();

            //計算文字框高度
            float nameH = textAreaStyle.CalcHeight(new GUIContent(nameText), 60);
            float maxNameH = Mathf.Max(20, nameH);

            float dialogTextH = textAreaStyle.CalcHeight(new GUIContent(dialogText), viewWidth);
            float maxDialogTextH = Mathf.Max(20, dialogTextH);

            GUILayout.Label("名稱", EditorStyles.miniBoldLabel, GUILayout.Width(20));
            string newNameText = EditorGUILayout.TextArea(nameText, textAreaStyle, GUILayout.Height(maxNameH), GUILayout.Width(60));
            GUILayout.Label("對話", EditorStyles.miniBoldLabel, GUILayout.Width(20));
            string newDialogText = EditorGUILayout.TextArea(dialogText, textAreaStyle, GUILayout.Height(maxDialogTextH), GUILayout.ExpandWidth(true));

            //繪製添加標籤按鈕
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                ShowTagMenu(i);
            }
            EditorGUILayout.EndHorizontal();

            // 標籤列表
            List<string> tagList = getLineTag(lineList[i]);
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

                    //拆分指令和參數
                    string rawTag = tagList[t];
                    string cmd = rawTag.Contains(":") ? rawTag.Split(':')[0] : rawTag;
                    string[] args = rawTag.Contains(":") ? rawTag.Split(':')[1].Split(',') : new string[0];

                    GUILayout.Label($"#{cmd}", EditorStyles.miniBoldLabel, GUILayout.Width(80));

                    if (tagCommand.ContainsKey(cmd))
                    {
                        string[] schema = tagCommand[cmd];
                        for (int j = 0; j < schema.Length; j++)
                        {
                            EditorGUILayout.LabelField(schema[j], EditorStyles.miniLabel, GUILayout.Width(50));
                            if (args.Length <= j)
                            {
                                System.Array.Resize(ref args, j + 1);
                            }
                            args[j] = EditorGUILayout.TextField(args[j] ?? "", GUILayout.MinWidth(40), GUILayout.Height(16));
                        }
                        tagList[t] = $"{cmd}:{string.Join(",", args)}";
                    }
                    else
                    {
                        string newVal = EditorGUILayout.TextField(rawTag.Contains(":") ? rawTag.Split(':')[1] : "", GUILayout.Height(16));
                        tagList[t] = string.IsNullOrEmpty(newVal) ? cmd : $"{cmd}:{newVal}";
                    }

                    if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(18)))
                    {
                        tagToDelete = t;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                }
            }

            //標籤和文字變更
            if (tagToDelete != -1)
            {
                tagList.RemoveAt(tagToDelete);
            }
            string finalLine = (newNameText == "") ? newDialogText : newNameText + ":" + newDialogText;
            foreach (var t in tagList)
            {
                finalLine += " #" + t;
            }
            if (lineList[i] != finalLine)
            {
                lineList[i] = finalLine;
                if (finalLine.Contains("==")) getKnot(selectFile);
            }

            EditorGUILayout.EndVertical();
        }

        // --- 執行列表變更作業 ---
        if (indexToMoveUp != -1)
        {
            string temp = lineList[indexToMoveUp];
            lineList.RemoveAt(indexToMoveUp);
            lineList.Insert(indexToMoveUp - 1, temp);
        }
        if (indexToMoveDown != -1)
        {
            string temp = lineList[indexToMoveDown];
            lineList.RemoveAt(indexToMoveDown);
            lineList.Insert(indexToMoveDown + 1, temp);
        }
        if (indexToAddAfter != -1)
        {
            lineList.Insert(indexToAddAfter + 1, ":");
        }
        if (indexToDelete != -1)
        {
            lineList.RemoveAt(indexToDelete);
        }
        getKnot(selectFile); // 重新計算節點位置
        Repaint();
        EditorGUILayout.EndScrollView();
    }

    private void drawContent(int i, List<string> lineList)
    {

    }

    private List<string> getLineTag(string line)
    {
        List<string> tagList = new List<string>();
        MatchCollection matches = Regex.Matches(line, @"#([^#]+)");
        foreach (Match m in matches)
        {
            tagList.Add(m.Groups[1].Value.Trim());
        }
        return tagList;
    }

    private void ShowTagMenu(int lineIndex)
    {
        GenericMenu menu = new GenericMenu();
        foreach (var def in tagCommand)
        {
            menu.AddItem(new GUIContent(def.Key), false, () => 
            {
                fileData[selectFile][lineIndex] += $" #{def.Key}:";
                Repaint();
            });
        }
        menu.ShowAsContext();
    }

    private void drawDropArea() 
    {
        Event evt = Event.current;
        Rect dropArea = new Rect(0, 0, position.width, position.height);

        //判斷拖曳檔案
        if(evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) 
        {
            if (!dropArea.Contains(evt.mousePosition)) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if(evt.type == EventType.DragPerform) 
            {
                DragAndDrop.AcceptDrag();

                //歷遍所有檔案
                foreach(Object obj in DragAndDrop.objectReferences) 
                {
                    string path = AssetDatabase.GetAssetPath(obj);

                    //判斷副檔名為ink
                    if (path.ToLower().EndsWith(".ink")) 
                    {
                        mainFilePath = Path.GetFullPath(path);
                        initializeInk(mainFilePath);
                        break;
                    }
                }
            }
            evt.Use();
        }
    }

    private void initializeInk(string path)
    {
        fileData.Clear();
        fileKnots.Clear();
        loadAllInk(path);
        selectFile = path;
        selectKnotIndex = -1;
        Repaint();
    }

    private void loadAllInk(string path) 
    {
        //確定檔案存在
        if (fileData.ContainsKey(path) || !File.Exists(path)) 
            return;

        //讀取檔案每一行
        string[] rawLines = File.ReadAllLines(path);
        fileData.Add(path, new List<string>(rawLines));
        getKnot(path);

        foreach (string line in rawLines)
        {
            //取得其他匯入的INK
            Match m = Regex.Match(line, @"^\s*INCLUDE\s+(.+)");

            //如果有，就繼續找此INK的節點
            if (m.Success)
            {
                loadAllInk(Path.Combine(Path.GetDirectoryName(path), m.Groups[1].Value.Trim()));
            }    
        }
    }

    private void getKnot(string path)
    {
        //確定檔案存在
        if (!fileData.ContainsKey(path)) 
            return;

        List<string> lineList = fileData[path];
        List<KnotData> knotList = new List<KnotData>();

        for (int i = 0; i < lineList.Count; i++)
        {
            //取得節點
            Match m = Regex.Match(lineList[i], @"^\s*==+\s*([a-zA-Z0-9_]+)");

            //保留，可能會改，目前為獲取節點到下一節點之間每一行
            if (m.Success)
            {
                if (knotList.Count > 0) 
                {
                    KnotData last = knotList[knotList.Count - 1];
                    last.endLine = i;
                    knotList[knotList.Count - 1] = last;
                }
                knotList.Add(new KnotData 
                    { 
                        name = m.Groups[1].Value,
                        startLine = i,
                        endLine = lineList.Count 
                    });
            }
        }
        fileKnots[path] = knotList;
    }

    private void SaveAll()
    {
        foreach (var kvp in fileData) File.WriteAllLines(kvp.Key, kvp.Value.ToArray());
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("成功", "標籤已拆分儲存！", "OK");
    }
}
