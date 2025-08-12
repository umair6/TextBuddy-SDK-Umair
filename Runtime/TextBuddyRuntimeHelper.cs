using System;
using UnityEngine;


namespace TextBuddy.Core
{
    public static class TextBuddyRuntimeHelper
    {
        private const string UserIDStorageKey = "TEXTBUDDY_USER_ID_KEY";
        private const string TextBuddyHostName = "textbuddy";


        public static TextBuddyConfig LoadConfig()
        {
            return Resources.Load<TextBuddyConfig>("TextBuddyConfig");
        }

        public static string GetStoredUserID()
        {
            return TBDataStorage.GetString(UserIDStorageKey, "");
        }

        public static void StoreTextBuddyUserID(string userID)
        {
            TBDataStorage.SetString(UserIDStorageKey, userID);
        }

        public static string GetTextBuddyDeeplinkHostName()
        {
            return TextBuddyHostName;
        }

        public static bool IsTextBuddyHostName(string hostName)
        {
            return string.Equals(hostName, TextBuddyHostName, StringComparison.OrdinalIgnoreCase);
        }
    }




}
