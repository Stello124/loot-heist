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
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        introPanel.SetActive(true);
        introText.text = "Bomba kimin elindeyse, 15 saniye içinde birine pasla yoksa patlarsýn!";
        introText.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(false);

        yield return new WaitForSeconds(introDuration);

        introText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);

        string[] countdownWords = { "3", "2", "1", "BAÞLA!" };
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



