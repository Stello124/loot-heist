using UnityEngine;
using TMPro;

public class PlayerNameBinder : MonoBehaviour
{
    public TextMeshProUGUI nameText;

    void Start()
    {
        Invoke(nameof(UpdateNameLate), 1f); // 1 saniye sonra çalýþýr
    }

    void UpdateNameLate()
    {
        if (nameText != null)
        {
            nameText.text = StartupManager.PlayerName;
            Debug.Log("GEÇ BAÐLAMA: " + nameText.text);
        }
    }
}