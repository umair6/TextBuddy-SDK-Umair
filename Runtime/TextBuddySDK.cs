using System;
using System.Collections.Generic;
using UnityEngine;

namespace TextBuddy.core
{
    public class TextBuddySDK : MonoBehaviour

    {
        public enum InitializationStatus { NotInitialized, Initializing, Initialized }
        public enum SubscriptionStatus { UnSubscribed, Pending, Subscribed }

        private const string TextBuddyHostName = "textbuddy";
        private const string UserIDStorageKey = "TEXTBUDDY_USER_ID_KEY";

        public static TextBuddySDK Instance { get; private set; }

        private TextBuddyConfig config;

        [SerializeField] private string textBuddyDemoPhoneNumber;
        public string TextBuddyPhoneNumber => textBuddyDemoPhoneNumber;

        private InitializationStatus sdkInitializationStatus = InitializationStatus.NotInitialized;
        private SubscriptionStatus subscriptionStatus = SubscriptionStatus.UnSubscribed;
        private string textBuddyUserID = "";

        public bool IsInitialized() => sdkInitializationStatus == InitializationStatus.Initialized;
        public bool IsUserSubscribed() => subscriptionStatus == SubscriptionStatus.Subscribed;
        public bool IsUserSignupInProgress() => subscriptionStatus == SubscriptionStatus.Pending;
        public string TextBuddyUserID() => textBuddyUserID;

        public static event Action OnSDKInitialized;
        public static event Action OnUserSubscribed;
        public static event Action<string> OnUserSubscribeFail;
        private static event Action OnUserUnsubscribed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            config = TextBuddyRuntimeHelper.LoadConfig();
            if (config == null)
            {
                TBLogger.Error("TextBuddyConfig Not Found", this);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Application.deepLinkActivated -= OnDeepLinkActivated;
                Instance = null;
            }
        }

        public void Initialize()
        {
            if (sdkInitializationStatus != InitializationStatus.NotInitialized)
                return;

            sdkInitializationStatus = InitializationStatus.Initializing;
            TBLogger.Info("Initializing...", this);

            // Simulate async initialization
            Invoke(nameof(FinishInitialization), 2f);
        }

        private void FinishInitialization()
        {
            textBuddyUserID = TBDataStorage.GetString(UserIDStorageKey, "");

            if (!string.IsNullOrEmpty(textBuddyUserID))
                subscriptionStatus = SubscriptionStatus.Subscribed;

            sdkInitializationStatus = InitializationStatus.Initialized;

            SetupDeeplinkListener();

            TBLogger.Info("Initialized", this);
            OnSDKInitialized?.Invoke();
        }

        private void SetupDeeplinkListener()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
            Application.deepLinkActivated += OnDeepLinkActivated;
        }

        private void OnDeepLinkActivated(string url)
        {
            TBLogger.Info("OnDeepLinkActivated", this);
            if (!string.IsNullOrEmpty(url))
            {
                HandleDeepLink(url);
            }
        }

        public void CheckPendingDeepLink()
        {
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                HandleDeepLink(Application.absoluteURL);
            }
        }

        private void HandleDeepLink(string url)
        {
            TBLogger.Info("HandleDeepLink: " + url, this);

            if (subscriptionStatus != SubscriptionStatus.Pending)
                return;

            var parser = new TBDeepLinkParser(url);

            if (!IsTextBuddyURL(parser.HostName))
                return;

            Dictionary<string, string> queryParams = parser.ParseQuery();
            if (queryParams == null)
                return;

            if (!queryParams.TryGetValue("status", out string status) || string.IsNullOrWhiteSpace(status))
            {
                subscriptionStatus = SubscriptionStatus.UnSubscribed;
                OnUserSubscribeFail?.Invoke("Missing or invalid status");
                return;
            }

            if (!status.Equals("success", StringComparison.OrdinalIgnoreCase))
            {
                subscriptionStatus = SubscriptionStatus.UnSubscribed;
                OnUserSubscribeFail?.Invoke("Signup failed");
                return;
            }

            if (!ValidateResponse(queryParams))
            {
                subscriptionStatus = SubscriptionStatus.UnSubscribed;
                OnUserSubscribeFail?.Invoke("Signature validation failed");
                return;
            }

            if (!queryParams.TryGetValue("id", out string id) || string.IsNullOrWhiteSpace(id))
            {
                subscriptionStatus = SubscriptionStatus.UnSubscribed;
                OnUserSubscribeFail?.Invoke("Missing or invalid ID");
                return;
            }

            SetTextBuddyUserID(id);
            subscriptionStatus = SubscriptionStatus.Subscribed;
            OnUserSubscribed?.Invoke();
        }

        private bool IsTextBuddyURL(string hostName)
        {
            return string.Equals(hostName, TextBuddyHostName, StringComparison.OrdinalIgnoreCase);
        }

        private void SetTextBuddyUserID(string playerID)
        {
            textBuddyUserID = playerID;
            TBDataStorage.SetString(UserIDStorageKey, textBuddyUserID);
        }

        private bool ValidateResponse(Dictionary<string, string> parameters)
        {
            return TBDeepLinkValidator.ValidateQueryParams(parameters, config.TextBuddyAPIKey, "sig");
        }

        public void Subscribe()
        {
            if (subscriptionStatus != SubscriptionStatus.UnSubscribed)
                return;

            var info = new TBSignUpInfo
            {
                Action = "SUBSCRIBE",
                GameID = config.TextBuddyGameID
            };

            TBLogger.Info("Subscribe", this);
            SubscribeInternal(info.ToString(), TextBuddyPhoneNumber);
        }

        private void SubscribeInternal(string message, string number)
        {
            TBLogger.Info("SubscribeInternal: " + message, this);
            subscriptionStatus = SubscriptionStatus.Pending;
            TBSMSSender.SendSms(number, message);
        }

        private void Unsubscribe()
        {
            if (subscriptionStatus != SubscriptionStatus.Subscribed)
                return;

            var info = new TBSignUpInfo
            {
                Action = "UNSUBSCRIBE",
                GameID = config.TextBuddyGameID
            };

            TBLogger.Info("UnSubscribe", this);
            UnSubscribeInternal(info.ToString(), TextBuddyPhoneNumber);
        }

        private void UnSubscribeInternal(string message, string number)
        {
            TBLogger.Info("UnSubscribeInternal: " + message, this);
            TBSMSSender.SendSms(number, message);
            subscriptionStatus = SubscriptionStatus.UnSubscribed;
            SetTextBuddyUserID("");
            OnUserUnsubscribed?.Invoke();
        }
    }
}
