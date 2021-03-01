using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Runtime.CompilerServices;

public class ChangeLang : EditorWindow
{
    /*
     * 最終的にはEditorの言語設定に連動させたい
     * どこに格納？Registry?
     * ->どのキーか？
     * https://docs.unity3d.com/ja/current/ScriptReference/EditorPrefs.html
     */
    [MenuItem("Tools/change language(ja <-> en)")]
    static void Open()
    {
        EditorSettings setting = Resources.Load<EditorSettings>("settings");
        if (setting == null)
        {
            EditorSettings.Create();
            setting = Resources.Load<EditorSettings>("settings");
            setting.language = 0;
        }
        if (setting.language == 0) setting.language = 1;
        else setting.language = 0;
        AssetDatabase.Refresh();
        EditorApplication.update();

    }
}