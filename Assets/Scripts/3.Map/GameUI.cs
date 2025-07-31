using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    public GameObject winPanel;
    public Text winText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Singleton kontrolü için
    }

    private void Start()
    {
        winPanel.SetActive(false);
        winText.gameObject.SetActive(false);
    }

    public void ShowWinText(string winnerName)
    {
        winPanel.SetActive(true);
        winText.text = "Kazandýn!";
        winText.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }
}



