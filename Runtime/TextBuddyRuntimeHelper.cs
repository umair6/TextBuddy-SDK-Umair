using UnityEngine;


namespace TextBuddy
{
    public static class TextBuddyRuntimeHelper
    {
        public static TextBuddyConfig LoadConfig()
        {
            return Resources.Load<TextBuddyConfig>("TextBuddyConfig");
        }
    }
}
