using UnityEngine;

public class PlayOnlineUI : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject lobbySetupPanel;

    public void HandlePlayOnlineClick()
    {
        mainPanel.SetActive(false);
        lobbySetupPanel.SetActive(true);
    }
}