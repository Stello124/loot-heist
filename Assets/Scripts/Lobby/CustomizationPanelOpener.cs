using UnityEngine;

public class CustomizationPanelOpener : MonoBehaviour
{
    [SerializeField] private GameObject customizationPanel;

    public void ToggleCustomizationPanel()
    {
        if (customizationPanel == null)
        {
            Debug.LogWarning("Customization Panel atanmamış.");
            return;
        }

        customizationPanel.SetActive(!customizationPanel.activeSelf);
    }
}
