using UnityEngine;

namespace TextBuddy
{
    public static class TBDataStorage
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
