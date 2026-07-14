// Original script: https://gist.github.com/FleshMobProductions/f598096b705f6a9c96beb58e284303f1

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace BlenderBridge
{
    public static class BlenderBridgeProcessor
    {
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
                OpenInBlender(assetPath);
                return true;
            }
            return false;
        }

        private static void OpenInBlender(string assetPath)
        {
            string blenderPath = BlenderBridgeSettings.BlenderPath;

            if (!File.Exists(blenderPath))
            {
                Debug.LogError($"Blender not found at {blenderPath}. Set the correct path in Edit > Preferences > Blender Bridge.");
                return;
            }

            string modelFullPath = Path.GetFullPath(assetPath);
            string pythonScript = PYTHON_SCRIPT_PATH;
            if (pythonScript == null) return;

            string settingsPath;
            try
            {
                settingsPath = WriteSettingsFile();
            }
            catch (Exception)
            {
                return;
            }

            string factoryStartupFlag = BlenderBridgeSettings.FactoryStartup ? "--factory-startup " : "";
            string arguments = $"{factoryStartupFlag}--python \"{pythonScript}\" -- \"{modelFullPath}\" \"{settingsPath}\"";
            StartBlenderWithArguments(blenderPath, arguments);
        }

        private static string WriteSettingsFile()
        {
            var payload = new BlenderBridgeSettings.Payload
            {
                closeAfterQuickSave = BlenderBridgeSettings.CloseAfterQuickSave,
                closeAfterManualSave = BlenderBridgeSettings.CloseAfterManualSave,
                includeCameras = BlenderBridgeSettings.IncludeCameras,
                includeLights = BlenderBridgeSettings.IncludeLights,
                includeOther = BlenderBridgeSettings.IncludeOther,
                bakeAnimation = BlenderBridgeSettings.BakeAnimation,
                loadTextures = BlenderBridgeSettings.LoadTextures,
                texturePath = BlenderBridgeSettings.TexturePath,
                textureExtensions = BlenderBridgeSettings.TextureExtensionsArray
            };

            string json = JsonUtility.ToJson(payload, true);
            string path = Path.Combine(Path.GetTempPath(), "blender-bridge-settings.json");
            File.WriteAllText(path, json);
            return path;
        }

        private static void StartBlenderWithArguments(string blenderPath, string arguments)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = blenderPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = arguments
            };
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}