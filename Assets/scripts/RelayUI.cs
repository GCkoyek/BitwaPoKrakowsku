using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelayUI : MonoBehaviour
{
    public RelayManager relayManager;

    [Header("UI")]
    public Button hostButton;
    public Button joinButton;
    public TMP_InputField joinCodeInput;
    public TMP_Text joinCodeDisplay;

    void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    async void OnHostClicked()
    {
        if (relayManager == null)
        {
            Debug.LogError("RelayUI: relayManager nie przypisany!");
            return;
        }

        string code = await relayManager.CreateRelay();
        if (!string.IsNullOrEmpty(code))
        {
            SetJoinCode(code);
        }
    }

    async void OnJoinClicked()
    {
        if (relayManager == null)
        {
            Debug.LogError("RelayUI: relayManager nie przypisany!");
            return;
        }

        string code = joinCodeInput.text.Trim();
        if (!string.IsNullOrEmpty(code))
        {
            await relayManager.JoinRelay(code);
            joinCodeDisplay.text = "Kod: " + code;
        }
    }

    public void SetJoinCode(string code)
    {
        if (joinCodeDisplay != null)
            joinCodeDisplay.text = "Kod: " + code;
    }
}
