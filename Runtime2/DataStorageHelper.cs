using UnityEngine;

namespace TextBuddy.Core
{
    public static class DataStorageHelper
    {

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
        public static string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
    }
}
