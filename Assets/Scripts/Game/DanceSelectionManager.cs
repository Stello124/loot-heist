using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DanceSelectionManager : MonoBehaviour
{
    public List<string> selectedDances = new List<string>();
    private const int maxSelection = 4;

    public void OnDanceButtonClicked(Button button, string danceName)
    {
        Debug.Log($"[DanceSelectionManager] Dance selected: {danceName}");

        if (selectedDances.Contains(danceName))
        {
            int index = selectedDances.IndexOf(danceName);
            selectedDances.Remove(danceName);
            DanceSlotManager.Instance.AssignDance(index, null); // slotu boþalt
            SetButtonColor(button, false);
            Debug.Log("[Dance Deselected] " + danceName);
        }
        else
        {
            if (selectedDances.Count >= maxSelection)
            {
                Debug.LogWarning("[Dance Selection Limit Reached]");
                return;
            }

            selectedDances.Add(danceName);
            int slotIndex = selectedDances.Count - 1;
            DanceSlotManager.Instance.AssignDance(slotIndex, danceName);
            SetButtonColor(button, true);
            Debug.Log("[Dance Selected] " + danceName);
        }
    }

    private void SetButtonColor(Button button, bool selected)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = selected ? new Color(1f, 0.5f, 0f) : Color.white; // turuncu / beyaz
        button.colors = colors;
    }
}