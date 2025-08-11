using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TextBuddy.core;

public class TBSignupController : MonoBehaviour
{

    [SerializeField] private Button _offerButton;
    [SerializeField] private TextMeshProUGUI _statusText;


    private void OnDestroy()
    {
        TextBuddySDK.OnSDKInitialized -= TextBuddy_OnSDKInitialized;
        TextBuddySDK.OnUserSubscribed -= TextBuddy_OnUserSubscribed;
        TextBuddySDK.OnUserSubscribeFail -= TextBuddy_OnUserSubscribeFail;

    }

    private void Awake()
    {
        _offerButton.onClick.AddListener(OnShowOfferPressed);
    }

    private void Start()
    {
        if (!TextBuddySDK.Instance.IsInitialized())
        {
            _offerButton.interactable = false;
            TextBuddySDK.OnSDKInitialized += TextBuddy_OnSDKInitialized;
            TextBuddySDK.Instance.Initialize();
        }
        UpdateUserStatus();
    }

    private void TextBuddy_OnSDKInitialized()
    {
        _offerButton.interactable = true;
        UpdateUserStatus();
        TextBuddySDK.OnUserSubscribed += TextBuddy_OnUserSubscribed;
        TextBuddySDK.OnUserSubscribeFail += TextBuddy_OnUserSubscribeFail;
    }


    public void OnShowOfferPressed()
    {
        Debug.Log("TBSignupController::OnShowOfferPressed");

        if (!TextBuddySDK.Instance.IsUserSubscribed())
        {
            UIManager.Instance.TBOfferPopup.Setup(null, null,
                () =>
                {
                    //YES
                    StartSignupProcess();
                    UpdateUserStatus();
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
        TextBuddySDK.Instance.Subscribe();
    }

    private void TextBuddy_OnUserSubscribeFail(string errorMessage)
    {
        UIManager.Instance.LoadingView.Close();
        UIManager.Instance.RegisterationFailedPopup.Setup(null, errorMessage, null, null);
        UpdateUserStatus();
    }

    private void TextBuddy_OnUserSubscribed()
    {
        UIManager.Instance.LoadingView.Close();
        UIManager.Instance.TBOfferAcceptPopup.Setup(null, null, null, null);
        UpdateUserStatus();
    }

    private void UpdateUserStatus()
    {
        string statusString = "";
        if (!TextBuddySDK.Instance.IsInitialized())
        {
            statusString = "Initialising";
        }
        else if (TextBuddySDK.Instance.IsUserSignupInProgress())
        {
            statusString = "Pending";
        }
        else if (TextBuddySDK.Instance.IsUserSubscribed())
        {
            statusString = "Subscribed::" + (TextBuddySDK.Instance.TextBuddyUserID());
        }
        else
        {
            statusString = "UnSubscribed";
        }

        SetStatusMessage("Status: " + statusString);
    }

    private void SetStatusMessage(string statusMessage)
    {
        _statusText.text = statusMessage;
    }


}
