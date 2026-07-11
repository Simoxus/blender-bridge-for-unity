using UnityEditor;
using UnityEngine;

namespace BlenderBridge
{
    internal static class BlenderBridgeSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Blender Bridge", SettingsScope.User)
            {
                label = "Blender Bridge",
                guiHandler = (searchContext) =>
                {
                    // It seems like every SettingsProvider has different padding? Really stupid design in general, because
                    // even tooltips in the NATIVE SettingsProviders are inconsistent: some have punctuation, some don't.
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(10); // Top

                        using (new GUILayout.VerticalScope())
                        {
                            EditorGUILayout.Space(8); // Left

                            EditorGUILayout.LabelField("Blender", EditorStyles.boldLabel);

                            EditorGUILayout.BeginHorizontal();
                            BlenderBridgeSettings.BlenderPath = EditorGUILayout.TextField("Blender Path", BlenderBridgeSettings.BlenderPath);
                            if (GUILayout.Button("Browse", GUILayout.Width(60)))
                            {
                                string selected = EditorUtility.OpenFilePanel("Select Blender Executable", "", "exe");
                                if (!string.IsNullOrEmpty(selected))
                                {
                                    BlenderBridgeSettings.BlenderPath = selected;
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                            BlenderBridgeSettings.FactoryStartup = EditorGUILayout.Toggle(
                                new GUIContent("Factory Startup", "Skips loading your startup file and addons."),
                                BlenderBridgeSettings.FactoryStartup
                            );

                            EditorGUILayout.Space(8);

                            EditorGUILayout.LabelField("Save Behavior", EditorStyles.boldLabel);
                            BlenderBridgeSettings.CloseAfterQuickSave = EditorGUILayout.Toggle(
                                new GUIContent("Close After Quick Save", "Closes Blender after using the save shortcut."),
                                BlenderBridgeSettings.CloseAfterQuickSave
                            );
                            BlenderBridgeSettings.CloseAfterManualSave = EditorGUILayout.Toggle(
                                new GUIContent("Close After Manual Save", "Closes Blender after using File > Export."),
                                BlenderBridgeSettings.CloseAfterManualSave
                            );

                            EditorGUILayout.Space(8);

                            EditorGUILayout.LabelField("FBX Export", EditorStyles.boldLabel);
                            BlenderBridgeSettings.IncludeCameras = EditorGUILayout.Toggle("Include Cameras", BlenderBridgeSettings.IncludeCameras);
                            BlenderBridgeSettings.IncludeLights = EditorGUILayout.Toggle("Include Lights", BlenderBridgeSettings.IncludeLights);
                            BlenderBridgeSettings.IncludeOther = EditorGUILayout.Toggle(
                                new GUIContent("Include Other", "Curves, surfaces, metaballs, etc."),
                                BlenderBridgeSettings.IncludeOther
                            );
                            BlenderBridgeSettings.BakeAnimation = EditorGUILayout.Toggle("Bake Animation", BlenderBridgeSettings.BakeAnimation);

                            EditorGUILayout.Space(8);

                            EditorGUILayout.LabelField("Textures", EditorStyles.boldLabel);
                            BlenderBridgeSettings.LoadTextures = EditorGUILayout.Toggle("Load Textures", BlenderBridgeSettings.LoadTextures);

                            EditorGUI.BeginDisabledGroup(!BlenderBridgeSettings.LoadTextures);

                            EditorGUILayout.BeginHorizontal();
                            BlenderBridgeSettings.TexturePath = EditorGUILayout.TextField("Texture Path", BlenderBridgeSettings.TexturePath);
                            if (GUILayout.Button("Browse", GUILayout.Width(60)))
                            {
                                string selected = EditorUtility.OpenFolderPanel("Select Texture Folder", "", "");
                                if (!string.IsNullOrEmpty(selected))
                                {
                                    BlenderBridgeSettings.TexturePath = selected;
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            BlenderBridgeSettings.TextureExtensions = EditorGUILayout.TextField(
                                new GUIContent("Texture Extensions"),
                                BlenderBridgeSettings.TextureExtensions
                            );

                            EditorGUI.EndDisabledGroup();
                        }
                    }
                }
            };

            return provider;
        }
    }
}