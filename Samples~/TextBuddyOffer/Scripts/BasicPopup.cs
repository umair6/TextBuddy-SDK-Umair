using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class BasicPopup : MonoBehaviour
{

    public TextMeshProUGUI messageText;
    public TextMeshProUGUI titleText;

    public Button yesButton;
    public Button noButton;

    private Action onYesAction;
    private Action onNoAction;




    public void Setup(string title, string message, Action onYes, Action onNo)
    {
        if (title != null)
        {
            titleText.text = title;
        }
        if (message != null)
        {
            messageText.text = message;
        }
        onYesAction = onYes;
        onNoAction = onNo;

        if (yesButton)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() => { onYesAction?.Invoke(); Close(); });
        }
        if (noButton)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() => { onNoAction?.Invoke(); Close(); });
        }
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

}
