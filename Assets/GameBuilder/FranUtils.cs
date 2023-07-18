using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class FranUtils 
{
    /// <summary>
    /// Returns the Assets file path for a Scriptable Object
    /// </summary>
    public static string GetScriptableObjectFilePath(ScriptableObject so)
    {
        MonoScript ms = MonoScript.FromScriptableObject(so);
        return AssetDatabase.GetAssetPath(ms);
    }

    public static string GetAssetFilePathFromName(string assetName)
    {
        return AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(assetName)[0]);
    }

}
