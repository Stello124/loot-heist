using System.Collections;
using UnityEngine;
using TMPro;

public class GameStartManager : MonoBehaviour
{
    public GameObject introPanel;
    public TMP_Text introText;
    public TMP_Text countdownText;
    public float introDuration = 2.5f;

    void Start()
    {
        // BombManager artık kendi sistemini kullanıyor, bu UI devre dışı
        Debug.Log("🎮 GameStartManager: BombManager otomatik çalışıyor, UI sistemi devre dışı");
        // StartCoroutine(StartSequence()); // Devre dışı
    }

    IEnumerator StartSequence()
    {
        introPanel.SetActive(true);
        introText.text = "Bomba kimin elindeyse, 15 saniye i�inde birine pasla yoksa patlars�n!";
        introText.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(false);

        yield return new WaitForSeconds(introDuration);

        introText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);

        string[] countdownWords = { "3", "2", "1", "BA�LA!" };
        foreach (string word in countdownWords)
        {
            countdownText.text = word;
            yield return new WaitForSeconds(1f);
        }

        introPanel.SetActive(false);

        var controller = Object.FindFirstObjectByType<GameFlowController>();
        if (controller != null)
            controller.StartGame();
    }
}



