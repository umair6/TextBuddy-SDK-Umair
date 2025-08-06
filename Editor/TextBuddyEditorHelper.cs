#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TextBuddy.Editor
{
    public static class TextBuddyEditorHelper
    {
#if UNITY_EDITOR
        public static  TextBuddyConfig LoadConfigFromResources()
        {
            string[] guids = AssetDatabase.FindAssets("t:TextBuddyConfig");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<TextBuddyConfig>(path);
                if (config != null)
                    return config;
            }
            return null;
        }
#endif
    }
}
