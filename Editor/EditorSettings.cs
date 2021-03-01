using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor;

public class EditorSettings : ScriptableObject
{

    [SerializeField] public int number;
    [SerializeField] public int language;//0:ja/1:en
    public static void Create()
    {
        if (!Directory.Exists("Assets/Resources")) Directory.CreateDirectory("Assets/Resources");
        var ex = CreateInstance<EditorSettings>();
        AssetDatabase.CreateAsset(ex, "Assets/Resources/settings.asset");
        AssetDatabase.Refresh();
    }
}
