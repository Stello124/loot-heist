using UnityEngine;
using TMPro;

public class CustomizationNameChanger : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI playerNameText;

    void Start()
    {
        inputField.text = StartupManager.PlayerName;
        playerNameText.text = StartupManager.PlayerName;
    }

    public void OnConfirmClick()
    {
        string newName = inputField.text.Trim();
        if (!string.IsNullOrEmpty(newName))
        {
            StartupManager.SetNewPlayerName(newName);
            playerNameText.text = newName;
        }
    }
}