using UnityEngine;
using UnityEditor;
using System.IO;

namespace TextBuddy.Editor
{
    [InitializeOnLoad]
    public static class TextBuddySetup
    {
        private const string ConfigFolderPath = "Assets/Resources";
        private const string ConfigAssetPath = ConfigFolderPath + "/TextBuddyConfig.asset";

        static TextBuddySetup()
        {
            EditorApplication.delayCall += EnsureConfigExists;
        }

        private static void EnsureConfigExists()
        {
            if (!File.Exists(ConfigAssetPath))
            {
                if (!Directory.Exists(ConfigFolderPath))
                    Directory.CreateDirectory(ConfigFolderPath);

                var config = ScriptableObject.CreateInstance<TextBuddyConfig>();

                // Add placeholder defaults
                config.TextBuddyGameID = "YOUR_GAME_ID_HERE";
                config.TextBuddyAPIKey = "YOUR_API_KEY_HERE";
                config.EnableDebugLogs = true;

                AssetDatabase.CreateAsset(config, ConfigAssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[TextBuddy] Config created at: " + ConfigAssetPath);
            }
        }

        public static TextBuddyConfig LoadConfig()
        {
            return AssetDatabase.LoadAssetAtPath<TextBuddyConfig>(ConfigAssetPath);
        }
    }
}
