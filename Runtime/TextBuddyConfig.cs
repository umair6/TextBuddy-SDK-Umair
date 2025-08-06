using UnityEngine;


namespace TextBuddy
{
    [CreateAssetMenu(fileName = "TextBuddyConfig", menuName = "SDK/TextBuddy Config")]
    public class TextBuddyConfig : ScriptableObject
    {
        [Header("Game ID")]
        public string TextBuddyGameID;

        [Header("API Settings")]
        public string TextBuddyAPIKey;

        [Header("Enable Debug Logs")]
        public bool EnableDebugLogs;
    }
}