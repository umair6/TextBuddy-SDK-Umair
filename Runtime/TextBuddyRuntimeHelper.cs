using UnityEngine;


namespace TextBuddy.core
{
    public static class TextBuddyRuntimeHelper
    {
        public static TextBuddyConfig LoadConfig()
        {
            return Resources.Load<TextBuddyConfig>("TextBuddyConfig");
        }
    }
}
