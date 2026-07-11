using UnityEditor;

namespace BlenderBridge
{
    [System.Serializable]
    public class BlenderBridgeSettingsPayload
    {
        public bool closeAfterQuickSave;
        public bool closeAfterManualSave;
        public bool includeCameras;
        public bool includeLights;
        public bool includeOther;
        public bool bakeAnimation;
        public bool loadTextures;
        public string texturePath;
        public string[] textureExtensions;
    }

    public static class BlenderBridgeSettings
    {
        private const string PREFIX = "BlenderBridge.";

        private const string DEFAULT_BLENDER_PATH = @"C:\Program Files (x86)\Steam\steamapps\common\Blender\blender.exe";
        private const string BLENDER_PATH_KEY = PREFIX + "BlenderPath";
        private const string FACTORY_STARTUP_KEY = PREFIX + "FactoryStartup";

        private const string CLOSE_AFTER_QUICK_SAVE_KEY = PREFIX + "CloseAfterQuickSave";
        private const string CLOSE_AFTER_MANUAL_SAVE_KEY = PREFIX + "CloseAfterManualSave";

        private const string LOAD_TEXTURES_KEY = PREFIX + "LoadTextures";
        private const string TEXTURE_PATH_KEY = PREFIX + "TexturePath";
        private const string TEXTURE_EXTENSIONS_KEY = PREFIX + "TextureExtensions";
        private const string DEFAULT_TEXTURE_EXTENSIONS = ".png,.jpg,.jpeg,.tga,.bmp";

        private const string INCLUDE_CAMERAS_KEY = PREFIX + "IncludeCameras";
        private const string INCLUDE_LIGHTS_KEY = PREFIX + "IncludeLights";
        private const string INCLUDE_OTHER_KEY = PREFIX + "IncludeOther";
        private const string BAKE_ANIMATION_KEY = PREFIX + "BakeAnimation";

        public static string BlenderPath
        {
            get => EditorPrefs.GetString(BLENDER_PATH_KEY, DEFAULT_BLENDER_PATH);
            set => EditorPrefs.SetString(BLENDER_PATH_KEY, value);
        }

        public static bool FactoryStartup
        {
            get => EditorPrefs.GetBool(FACTORY_STARTUP_KEY, false);
            set => EditorPrefs.SetBool(FACTORY_STARTUP_KEY, value);
        }

        public static bool CloseAfterQuickSave
        {
            get => EditorPrefs.GetBool(CLOSE_AFTER_QUICK_SAVE_KEY, true);
            set => EditorPrefs.SetBool(CLOSE_AFTER_QUICK_SAVE_KEY, value);
        }

        public static bool CloseAfterManualSave
        {
            get => EditorPrefs.GetBool(CLOSE_AFTER_MANUAL_SAVE_KEY, false);
            set => EditorPrefs.SetBool(CLOSE_AFTER_MANUAL_SAVE_KEY, value);
        }

        public static bool IncludeCameras
        {
            get => EditorPrefs.GetBool(INCLUDE_CAMERAS_KEY, false);
            set => EditorPrefs.SetBool(INCLUDE_CAMERAS_KEY, value);
        }

        public static bool IncludeLights
        {
            get => EditorPrefs.GetBool(INCLUDE_LIGHTS_KEY, false);
            set => EditorPrefs.SetBool(INCLUDE_LIGHTS_KEY, value);
        }

        public static bool IncludeOther
        {
            get => EditorPrefs.GetBool(INCLUDE_OTHER_KEY, false);
            set => EditorPrefs.SetBool(INCLUDE_OTHER_KEY, value);
        }

        public static bool BakeAnimation
        {
            get => EditorPrefs.GetBool(BAKE_ANIMATION_KEY, false);
            set => EditorPrefs.SetBool(BAKE_ANIMATION_KEY, value);
        }

        public static bool LoadTextures
        {
            get => EditorPrefs.GetBool(LOAD_TEXTURES_KEY, false);
            set => EditorPrefs.SetBool(LOAD_TEXTURES_KEY, value);
        }

        public static string TexturePath
        {
            get => EditorPrefs.GetString(TEXTURE_PATH_KEY, "");
            set => EditorPrefs.SetString(TEXTURE_PATH_KEY, value);
        }

        public static string TextureExtensions
        {
            get => EditorPrefs.GetString(TEXTURE_EXTENSIONS_KEY, DEFAULT_TEXTURE_EXTENSIONS);
            set => EditorPrefs.SetString(TEXTURE_EXTENSIONS_KEY, value);
        }

        public static string[] TextureExtensionsArray
        {
            get
            {
                string raw = TextureExtensions;
                if (string.IsNullOrEmpty(raw)) return new string[0];

                string[] parts = raw.Split(',');
                var result = new System.Collections.Generic.List<string>(parts.Length);
                foreach (string part in parts)
                {
                    string trimmed = part.Trim();
                    if (trimmed.Length > 0) result.Add(trimmed);
                }
                return result.ToArray();
            }
        }
    }
}