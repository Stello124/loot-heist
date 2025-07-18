using UnityEngine;

public class CustomizationUIController : MonoBehaviour
{
    public static CustomizationUIController Instance;

    [SerializeField] private GameObject customizationPanel;

    void Awake()
    {
        Instance = this;
    }

    public void OpenCustomizationPanel()
    {
        customizationPanel.SetActive(true);
    }

    public void CloseCustomizationPanel()
    {
        customizationPanel.SetActive(false);
    }
}