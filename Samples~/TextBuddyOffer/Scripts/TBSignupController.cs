using UnityEngine;
using UnityEngine.UI;
using TextBuddy;
using TMPro;

public class TBSignupController : MonoBehaviour
{

    [SerializeField] private Button _offerButton;
    [SerializeField] private Button _unsubscribeButton;
    [SerializeField] private TextMeshProUGUI _statusText;


    private void OnDestroy()
    {
        TextBuddy.TextBuddy.OnSDKInitialized -= TextBuddy_OnSDKInitialized;
        TextBuddy.TextBuddy.OnUserSubscribed -= TextBuddy_OnUserSubscribed;
        TextBuddy.TextBuddy.OnUserSubscribeFail -= TextBuddy_OnUserSubscribeFail;
        TextBuddy.TextBuddy.OnUserUnSubscribed -= TextBuddy_OnUserUnSubscribed;

    }

    private void TextBuddy_OnUserUnSubscribed()
    {
        UpdateUnsubscribeButton();
    }

    private void Awake()
    {
        _offerButton.onClick.AddListener(OnShowOfferPressed);
        _unsubscribeButton.onClick.AddListener(OnUnsubscribe);
        _unsubscribeButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (!TextBuddy.TextBuddy.Instance.IsInitialized())
        {
            _offerButton.interactable = false;
            TextBuddy.TextBuddy.OnSDKInitialized += TextBuddy_OnSDKInitialized;
            TextBuddy.TextBuddy.Instance.InitialiseTextBuddy();
        }
        UpdateUserStatus();
    }

    private void TextBuddy_OnSDKInitialized()
    {
        _offerButton.interactable = true;
        UpdateUserStatus();
        UpdateUnsubscribeButton();
        TextBuddy.TextBuddy.OnUserSubscribed += TextBuddy_OnUserSubscribed;
        TextBuddy.TextBuddy.OnUserUnSubscribed += TextBuddy_OnUserUnSubscribed;
        TextBuddy.TextBuddy.OnUserSubscribeFail += TextBuddy_OnUserSubscribeFail;
    }

    private void UpdateUnsubscribeButton()
    {
        _unsubscribeButton.gameObject.SetActive(TextBuddy.TextBuddy.Instance.IsUserSubscribed());
    }

    public void OnUnsubscribe()
    {
        Debug.Log("TBSignupController::OnUnsubscribe");

        if (TextBuddy.TextBuddy.Instance.IsUserSubscribed())
        {
            TextBuddy.TextBuddy.Instance.UnSubscribe();
        }

    }

    public void OnShowOfferPressed()
    {
        Debug.Log("TBSignupController::OnShowOfferPressed");

        if (!TextBuddy.TextBuddy.Instance.IsUserSubscribed())
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
        TextBuddy.TextBuddy.Instance.Subscribe();
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
        UpdateUnsubscribeButton();
    }

    private void UpdateUserStatus()
    {
        string statusString = "";
        if (!TextBuddy.TextBuddy.Instance.IsInitialized())
        {
            statusString = "Initialising";
        }
        else if (TextBuddy.TextBuddy.Instance.IsUserSignupInProgress())
        {
            statusString = "Pending";
        }
        else if (TextBuddy.TextBuddy.Instance.IsUserSubscribed())
        {
            statusString = "Subscribed";
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
