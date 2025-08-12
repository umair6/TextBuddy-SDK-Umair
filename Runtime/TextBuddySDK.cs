using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace TextBuddy.Core
{

    public class TextBuddySDK : MonoBehaviour

    {

        public enum Initialization { NotStarted, InProgress, Complete }
        public enum Subscription { None, Pending, Active }

        [SerializeField] private string MockServerURL;
        private TextBuddyConfig config;

        public static TextBuddySDK Instance { get; private set; }
        public string TextBuddyPhoneNumber { get; private set; }
        public Initialization InitializationState { get; private set; }
        public Subscription SubscriptionState { get; private set; }
        public string TextBuddyUserID { get; private set; }

        public static event EventHandler<SDKInitializedEventArgs> OnSDKInitialized;
        public static event EventHandler<UserSubscribedEventArgs> OnUserSubscribed;
        public static event EventHandler<UserSubscribeFailedEventArgs> OnUserSubscribeFail;
        private static event EventHandler<UserUnsubscribedEventArgs> OnUserUnsubscribed;


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
            InitializationState = Initialization.NotStarted;
            SubscriptionState = Subscription.None;
            config = TextBuddyRuntimeHelper.LoadConfig();
            if (config == null)
            {
                TextBuddyLogger.Error("TextBuddyConfig Not Found", this);
            }
            else
            {
                TextBuddyLogger.EnableInfo = config.EnableDebugLogs;
                TextBuddyLogger.EnableWarning = config.EnableDebugLogs;
                TextBuddyLogger.EnableError = config.EnableDebugLogs;
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
            if (InitializationState != Initialization.NotStarted)
            {
                return;
            }
            InitializationState = Initialization.InProgress;
            TextBuddyUserID = TextBuddyRuntimeHelper.GetStoredUserID();
            TextBuddyLogger.Info("Initializing...", this);
            SendConnectRequest();
        }


        private async void SendConnectRequest()
        {
            string baseURL = MockServerURL;
            var result = await TextBuddy.Core.TextBuddyWebConnect.ConnectAsync(
                baseUrl: baseURL,
                gameID: config.TextBuddyGameID,
                userID: TextBuddyUserID,
                apiKey: config.TextBuddyAPIKey,
                timeoutSeconds: 10
            );

            if (result.Success)
            {
                TextBuddyPhoneNumber = result.PhoneNumber;
                if (!result.IsSubscribed) SetTextBuddyUserID("");
                FinishInitialization(true, result.Error, "");
            }
            else
            {

                FinishInitialization(false, result.Error, result.Message);
            }

        }

        private void FinishInitialization(bool success, ConnectError errorCode, string errorMessage)
        {
            if (success)
            {
                if (!string.IsNullOrEmpty(TextBuddyUserID))
                    SubscriptionState = Subscription.Active;

                InitializationState = Initialization.Complete;

                SetupDeeplinkListener();
                TextBuddyLogger.Info("Initialized", this);
            }
            else
            {
                SubscriptionState = Subscription.None;
                InitializationState = Initialization.NotStarted;

            }
            OnSDKInitialized?.Invoke(this, new SDKInitializedEventArgs(success, errorCode, errorMessage));
        }

        private void SetupDeeplinkListener()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
            Application.deepLinkActivated += OnDeepLinkActivated;
        }

        private void OnDeepLinkActivated(string url)
        {
            TextBuddyLogger.Info("OnDeepLinkActivated", this);
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
            TextBuddyLogger.Info("HandleDeepLink: " + url, this);

            if (SubscriptionState != Subscription.Pending)
                return;

            var parser = new DeepLinkParser(url);

            if (!TextBuddyRuntimeHelper.IsTextBuddyHostName(parser.HostName))
                return;

            Dictionary<string, string> queryParams = parser.ParseQuery();
            if (queryParams == null)
                return;

            if (!queryParams.TryGetValue("status", out string status) || string.IsNullOrWhiteSpace(status))
            {
                SubscriptionState = Subscription.None;
                int errorCode = 1;
                OnUserSubscribeFail?.Invoke(this, new UserSubscribeFailedEventArgs(errorCode, "Missing or invalid status"));
                return;
            }

            if (!status.Equals("success", StringComparison.OrdinalIgnoreCase))
            {
                SubscriptionState = Subscription.None;
                int errorCode = 2;
                OnUserSubscribeFail?.Invoke(this, new UserSubscribeFailedEventArgs(errorCode, "Signup failed"));

                return;
            }

            if (!ValidateResponse(queryParams))
            {
                SubscriptionState = Subscription.None;
                int errorCode = 3;
                OnUserSubscribeFail?.Invoke(this, new UserSubscribeFailedEventArgs(errorCode, "Signature validation failed"));

                return;
            }

            if (!queryParams.TryGetValue("id", out string id) || string.IsNullOrWhiteSpace(id))
            {
                SubscriptionState = Subscription.None;
                int errorCode = 4;
                OnUserSubscribeFail?.Invoke(this, new UserSubscribeFailedEventArgs(errorCode, "Missing or invalid ID"));
                return;
            }

            SetTextBuddyUserID(id);
            SubscriptionState = Subscription.Active;
            OnUserSubscribed?.Invoke(this, new UserSubscribedEventArgs(id));
        }

        private void SetTextBuddyUserID(string playerID)
        {
            TextBuddyUserID = playerID;
            TextBuddyRuntimeHelper.StoreTextBuddyUserID(TextBuddyUserID);
        }

        private bool ValidateResponse(Dictionary<string, string> parameters)
        {
            return DeepLinkValidator.ValidateQueryParams(parameters, config.TextBuddyAPIKey, "sig");
        }

        public void Subscribe()
        {
            if (SubscriptionState != Subscription.None)
                return;

            var info = new SignUpInfo
            {
                Action = "SUBSCRIBE",
                GameID = config.TextBuddyGameID
            };

            TextBuddyLogger.Info("Subscribe", this);
            SubscribeInternal(info.ToString(), TextBuddyPhoneNumber);
        }

        private void SubscribeInternal(string message, string number)
        {
            TextBuddyLogger.Info("SubscribeInternal: " + message, this);
            SubscriptionState = Subscription.Pending;

            SMSSender.SendSms(number, message);
        }

        private void Unsubscribe()
        {
            if (SubscriptionState != Subscription.Active)
                return;

            var info = new SignUpInfo
            {
                Action = "UNSUBSCRIBE",
                GameID = config.TextBuddyGameID
            };

            TextBuddyLogger.Info("UnSubscribe", this);
            UnSubscribeInternal(info.ToString(), TextBuddyPhoneNumber);
        }

        private void UnSubscribeInternal(string message, string number)
        {
            TextBuddyLogger.Info("UnSubscribeInternal: " + message, this);
            string userID = TextBuddyUserID;
            SMSSender.SendSms(number, message);
            SubscriptionState = Subscription.None;
            SetTextBuddyUserID("");
            OnUserUnsubscribed?.Invoke(this, new UserUnsubscribedEventArgs(userID));
        }
    }
}
