using UnityEngine;
using UnityEngine.UI;

public class DanceUIController : MonoBehaviour
{
    public GameObject panel;
    public Button[] danceButtons;

    void Start()
    {
        panel.SetActive(false);

        for (int i = 0; i < danceButtons.Length; i++)
        {
            int index = i;
            danceButtons[i].onClick.AddListener(() => TriggerDance(index));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            panel.SetActive(!panel.activeSelf);
            UpdateDanceButtons();
        }
    }

    void UpdateDanceButtons()
    {
        for (int i = 0; i < danceButtons.Length; i++)
        {
            string danceName = DanceSlotManager.Instance.danceSlots[i];
            danceButtons[i].GetComponentInChildren<Text>().text = danceName;
            danceButtons[i].interactable = !string.IsNullOrEmpty(danceName);
        }
    }

    void TriggerDance(int index)
    {
        string danceName = DanceSlotManager.Instance.danceSlots[index];
        if (!string.IsNullOrEmpty(danceName))
        {
            Debug.Log($"[Dance Triggered] {danceName}");
            panel.SetActive(false);
            // Animator trigger burada çaðrýlýr
        }
    }
}