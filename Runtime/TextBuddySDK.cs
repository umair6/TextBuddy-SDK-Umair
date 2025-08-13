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


        [SerializeField] private string mockServerURL;
        private TextBuddyConfig textBuddyConfig;

        public static TextBuddySDK Instance { get; private set; }
        public string TextBuddyPhoneNumber { get; private set; }
        public Initialization InitializationState { get; private set; }
        public Subscription SubscriptionState { get; private set; }
        public string TextBuddyUserID { get; private set; }

        public static event EventHandler<SDKInitializedEventArgs> OnSDKInitialized;
        public static event EventHandler<UserSubscribedEventArgs> OnUserSubscribed;
        public static event EventHandler<UserSubscribeFailedEventArgs> OnUserSubscribeFail;

        

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
            textBuddyConfig = TextBuddyRuntimeHelper.LoadConfig();
            if (textBuddyConfig == null)
            {
                TextBuddyLogger.Error("TextBuddyConfig Not Found", this);
            }
            else
            {
                TextBuddyLogger.EnableInfo = textBuddyConfig.EnableDebugLogs;
                TextBuddyLogger.EnableWarning = textBuddyConfig.EnableDebugLogs;
                TextBuddyLogger.EnableError = textBuddyConfig.EnableDebugLogs;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
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
            string baseURL = mockServerURL;
            var result = await TextBuddy.Core.TextBuddyWebConnect.ConnectAsync(
                baseUrl: baseURL,
                gameID: textBuddyConfig.TextBuddyGameID,
                userID: TextBuddyUserID,
                apiKey: textBuddyConfig.TextBuddyAPIKey,
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
                TextBuddyLogger.Info("Initialized", this);
            }
            else
            {
                SubscriptionState = Subscription.None;
                InitializationState = Initialization.NotStarted;

            }
            OnSDKInitialized?.Invoke(this, new SDKInitializedEventArgs(success, errorCode, errorMessage));
        }

        public void ProcessDeepLink(string url)
        {
            TextBuddyLogger.Info("ProcessDeepLink: " + url, this);

            if (string.IsNullOrEmpty(url))
            {
                TextBuddyLogger.Info("ProcessDeepLink: Empty URL.", this);
                return;
            }

            if (InitializationState != Initialization.Complete)
            {
                TextBuddyLogger.Info("ProcessDeepLink: SDK not initialized.", this);
                return;
            }

            if (SubscriptionState != Subscription.Pending)
            {
                TextBuddyLogger.Info("ProcessDeepLink: No pending subscription request.", this);
                return;
            }

            var parser = new DeepLinkParser(url);
            if (!TextBuddyRuntimeHelper.IsTextBuddyHostName(parser.HostName))
            {
                TextBuddyLogger.Info("ProcessDeepLink: Not a TextBuddy URL.", this);
                return;
            }

            Dictionary<string, string> queryParams = parser.ParseQuery();
            if (queryParams == null)
            {
                TextBuddyLogger.Info("ProcessDeepLink: No query params.", this);
                return;
            }

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
            return DeepLinkValidator.ValidateQueryParams(parameters, textBuddyConfig.TextBuddyAPIKey, "sig");
        }

        public void Subscribe()
        {
            if (SubscriptionState != Subscription.None)
                return;

            var info = new SignUpInfo
            {
                Action = "SUBSCRIBE",
                GameID = textBuddyConfig.TextBuddyGameID
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
    }
}
