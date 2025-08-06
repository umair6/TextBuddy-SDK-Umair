using System;
using UnityEngine;

namespace TextBuddy.core
{
    /// <summary>
    /// Sends SMS using the platform's native messaging app via URL schemes.
    /// </summary>
    public static class TBSMSSender
    {
        /// <summary>
        /// Opens the native SMS composer with the specified phone number and message.
        /// </summary>
        /// <param name="phoneNumber">Recipient's phone number.</param>
        /// <param name="message">Text message body.</param>
        public static void SendSms(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
            {
                TBLoger.Warning("[BasicSMSSender] Phone number or message is empty.");
                return;
            }

            string url;

#if UNITY_ANDROID
            url = $"sms:{phoneNumber}?body={Uri.EscapeDataString(message)}";
#elif UNITY_IOS
            url = $"sms:{phoneNumber}&body={Uri.EscapeDataString(message)}";
#else
            TBLoger.Warning("[BasicSMSSender] SMS sending not supported on this platform.");
            return;
#endif

            TBLoger.Info($"[BasicSMSSender] Opening SMS URL: {url}");
            Application.OpenURL(url);
        }
    }
}
