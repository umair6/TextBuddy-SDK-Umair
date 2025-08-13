using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TextBuddy.Core;

public class TBSignupController : MonoBehaviour
{
    [SerializeField] private Button _initButton;
    [SerializeField] private Button _offerButton;
    [SerializeField] private TextMeshProUGUI _sdkStatusText;
    [SerializeField] private TextMeshProUGUI _subscriptionStatusText;

    private void OnDestroy()
    {
        Application.deepLinkActivated -= OnDeepLinkActivated;
        TextBuddySDK.OnSDKInitialized -= TextBuddySDK_OnSDKInitialized;
        UnregisterSubscriptionCalls();
    }

    private void Awake()
    {
        _initButton.onClick.AddListener(InitializeTextBuddySDK);
        _offerButton.onClick.AddListener(OnShowOfferPressed);
    }

    private void Start()
    {
        _initButton.interactable = true;
        _offerButton.gameObject.SetActive(false);
        UpdateSDKStatus();
        UpdateSubscriptionStatus();
        SetupDeeplinkListener();
    }

    public void InitializeTextBuddySDK()
    {
        _initButton.interactable = false;
        TextBuddySDK.OnSDKInitialized += TextBuddySDK_OnSDKInitialized;
        TextBuddySDK.Instance.Initialize();
    }



    private void SetupDeeplinkListener()
    {
        Application.deepLinkActivated += OnDeepLinkActivated;
    }

    private void OnDeepLinkActivated(string url)
    {
        Debug.Log("OnDeepLinkActivated", this);
        if (!string.IsNullOrEmpty(url))
        {
            TextBuddySDK.Instance.ProcessDeepLink(url);
        }
    }

    private void TextBuddySDK_OnSDKInitialized(object sender, SDKInitializedEventArgs e)
    {
        _initButton.interactable = !e.Status;
        _offerButton.gameObject.SetActive(e.Status);
        UpdateSDKStatus();
        UpdateSubscriptionStatus();
    }

    public void OnShowOfferPressed()
    {
        Debug.Log("TBSignupController::OnShowOfferPressed");

        if (TextBuddySDK.Instance.SubscriptionState != TextBuddySDK.Subscription.Active)
        {
            UIManager.Instance.TBOfferPopup.Setup(null, null,
                () =>
                {
                    //YES
                    StartSignupProcess();
                }, null);
        }
        else
        {
            UIManager.Instance.AlreadyRegisteredPopup.Setup(null, null, null, null);
        }
    }

    private void StartSignupProcess()
    {
        UIManager.Instance.LoadingView.Setup(null, null, null, null);
        RegisterSubscriptionCalls();
        TextBuddySDK.Instance.Subscribe();
        UpdateSubscriptionStatus();
    }

    private void RegisterSubscriptionCalls()
    {
        TextBuddySDK.OnUserSubscribed += TextBuddySDK_OnUserSubscribed;
        TextBuddySDK.OnUserSubscribeFail += TextBuddySDK_OnUserSubscribeFail;
    }

    private void UnregisterSubscriptionCalls()
    {
        TextBuddySDK.OnUserSubscribed -= TextBuddySDK_OnUserSubscribed;
        TextBuddySDK.OnUserSubscribeFail -= TextBuddySDK_OnUserSubscribeFail;
    }


    private void TextBuddySDK_OnUserSubscribeFail(object sender, UserSubscribeFailedEventArgs e)
    {
        UnregisterSubscriptionCalls();
        UIManager.Instance.LoadingView.Close();
        UIManager.Instance.RegisterationFailedPopup.Setup(null, e.ErrorMessage, null, null);
        UpdateSubscriptionStatus();
    }

    private void TextBuddySDK_OnUserSubscribed(object sender, UserSubscribedEventArgs e)
    {
        UnregisterSubscriptionCalls();
        UIManager.Instance.LoadingView.Close();
        UIManager.Instance.TBOfferAcceptPopup.Setup(null, null, null, null);
        UpdateSubscriptionStatus();
    }



    private void UpdateSDKStatus()
    {
        string sdkStatus = "SDK: Not Initialized";
        if (TextBuddySDK.Instance.InitializationState == TextBuddySDK.Initialization.InProgress)
        {
            sdkStatus = "SDK: Initializing";
        }
        else if (TextBuddySDK.Instance.InitializationState == TextBuddySDK.Initialization.Complete)
        {
            sdkStatus = "SDK: Initialized";
        }
        _sdkStatusText.text = sdkStatus;
    }


    private void UpdateSubscriptionStatus()
    {
        string subscriptionStatus = "Subscription: None";
        if (TextBuddySDK.Instance.SubscriptionState == TextBuddySDK.Subscription.Pending)
        {
            subscriptionStatus = "Subscription: Pending";
        }
        else if (TextBuddySDK.Instance.SubscriptionState == TextBuddySDK.Subscription.Active)
        {
            subscriptionStatus = "Subscription: Active";
        }
        _subscriptionStatusText.text = subscriptionStatus;
    }


}
