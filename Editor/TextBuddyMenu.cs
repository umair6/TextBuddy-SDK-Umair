// Editor/TextBuddyMenu.cs
using UnityEditor;
using UnityEngine;
using TextBuddy.core;
namespace TextBuddy.Editor
{
    public class TextBuddyMenu : EditorWindow
    {
        private TextBuddyConfig config;

        [MenuItem("TextBuddy/Configure")]
        public static void ShowWindow()
        {
            var window = GetWindow<TextBuddyMenu>("TextBuddy Setup");
            window.Show();
        }

        private void OnEnable()
        {
            config = TextBuddySetup.LoadConfig();
        }

        private void OnGUI()
        {
            if (config == null)
            {
                EditorGUILayout.HelpBox("Config file not found!", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("TextBuddy Configuration", EditorStyles.boldLabel);

            config.TextBuddyGameID = EditorGUILayout.TextField("Game ID", config.TextBuddyGameID);
            config.TextBuddyAPIKey = EditorGUILayout.TextField("API Key", config.TextBuddyAPIKey);
            config.EnableDebugLogs = EditorGUILayout.Toggle("Enable Logs", config.EnableDebugLogs);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
