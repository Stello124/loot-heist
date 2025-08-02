using UnityEngine;
using UnityEngine.UI;

public class DanceButtonHandler : MonoBehaviour
{
    public string danceName;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        DanceSelectionManager manager = FindObjectOfType<DanceSelectionManager>();
        if (manager != null)
        {
            manager.OnDanceButtonClicked(button, danceName);
        }
        else
        {
            Debug.LogError("[DanceButtonHandler] DanceSelectionManager bulunamadý.");
        }
    }
}