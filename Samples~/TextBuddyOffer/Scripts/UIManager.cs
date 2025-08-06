using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }


    [SerializeField] public BasicPopup TBOfferPopup;
    [SerializeField] public BasicPopup TBOfferAcceptPopup;
    [SerializeField] public BasicPopup AlreadyRegisteredPopup;
    [SerializeField] public BasicPopup RegisterationFailedPopup;
    [SerializeField] public BasicPopup LoadingView;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Avoid duplicates
            return;
        }
        Instance = this;
    }



}
