/*
 * Original script: https://gist.github.com/FleshMobProductions/f598096b705f6a9c96beb58e284303f1
*/

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class BlenderBridgeProcessor
{
    private static readonly bool DEBUG = false; // If false it'll only log errors

    private static readonly string BLENDER_PATH = @"C:\Program Files (x86)\Steam\steamapps\common\Blender\blender.exe";
    private static readonly string[] SUPPORTED_EXTENSIONS = { ".fbx", ".obj", ".dae" };

    private static string _cachedPythonScriptPath;
    private static string PYTHON_SCRIPT_PATH
    {
        get
        {
            if (_cachedPythonScriptPath == null)
            {
                string[] guids = AssetDatabase.FindAssets("blender-bridge-injector t:DefaultAsset");
                if (guids.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _cachedPythonScriptPath = Path.GetFullPath(assetPath);
                }
            }
            return _cachedPythonScriptPath;
        }
    }

#if UNITY_6000_3_OR_NEWER
    [OnOpenAsset]
    public static bool OnOpenAsset(EntityId entityId, int line)
    {
        UnityEngine.Object obj = EditorUtility.EntityIdToObject(entityId);
        string assetPath = AssetDatabase.GetAssetPath(entityId);
        string extension = Path.GetExtension(assetPath).ToLowerInvariant();
#else
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceId);
        string assetPath = AssetDatabase.GetAssetPath(instanceId);
        string extension = Path.GetExtension(assetPath).ToLowerInvariant();
#endif

        if (Array.Exists(SUPPORTED_EXTENSIONS, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase))
            && obj is GameObject)
        {
            if (DEBUG)
            {
                Debug.Log($"Opening {extension.ToUpper()} '{assetPath}' in Blender");
            }

            OpenInBlender(assetPath);
            return true;
        }
        return false;
    }

    private static void OpenInBlender(string assetPath)
    {
        if (!File.Exists(BLENDER_PATH))
        {
            Debug.LogError($"Blender not found at {BLENDER_PATH}.");
            return;
        }

        string modelFullPath = Path.GetFullPath(assetPath);

        string pythonScript = PYTHON_SCRIPT_PATH;
        if (pythonScript == null)
        {
            Debug.LogError("Python script not found");
            return;
        }

        string arguments = $"--python \"{pythonScript}\" -- \"{modelFullPath}\"";
        StartBlenderWithArguments(arguments);
    }

    private static void StartBlenderWithArguments(string arguments)
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = BLENDER_PATH,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            Arguments = arguments
        };
        process.StartInfo = startInfo;
        process.Start();
    }
}