using UnityEngine;

namespace TextBuddy.core
{
    /// <summary>
    /// Simple logger for TextBuddy SDK with log level control. Logs only in debug or development builds.
    /// </summary>
    public static class TBLoger
    {
        public static bool EnableInfo = true;
        public static bool EnableWarning = true;
        public static bool EnableError = true;

        private const string Prefix = "[TextBuddy] ";

        public static void Info(string message, Object context = null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (EnableInfo)
                LogInternal(LogType.Log, message, context);
#endif
        }

        public static void Warning(string message, Object context = null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (EnableWarning)
                LogInternal(LogType.Warning, message, context);
#endif
        }

        public static void Error(string message, Object context = null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (EnableError)
                LogInternal(LogType.Error, message, context);
#endif
        }

        private static void LogInternal(LogType type, string message, Object context)
        {
            string formattedMessage = Prefix + message;

            switch (type)
            {
                case LogType.Log:
                    if (context != null)
                        Debug.Log(formattedMessage, context);
                    else
                        Debug.Log(formattedMessage);
                    break;

                case LogType.Warning:
                    if (context != null)
                        Debug.LogWarning(formattedMessage, context);
                    else
                        Debug.LogWarning(formattedMessage);
                    break;

                case LogType.Error:
                    if (context != null)
                        Debug.LogError(formattedMessage, context);
                    else
                        Debug.LogError(formattedMessage);
                    break;
            }
        }
    }
}
