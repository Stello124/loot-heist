using UnityEngine;
using TMPro;
using System.Collections;
using UnityEditor.PackageManager;

public class CountdownManager : MonoBehaviour
{
    public GameObject panelUI;
    public TMP_Text introText;
    public TMP_Text countdownText;
    public GameObject rotatingObject;


    void Start()
    {
        rotatingObject.GetComponent<Rotator>().enabled = false;
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        panelUI.SetActive(true);
        introText.text = "Dikkatli ol, düþme!";
        countdownText.text = "";

        yield return new WaitForSeconds(2f);

        introText.gameObject.SetActive(false);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        countdownText.text = "BAÞLA!";
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);

        panelUI.SetActive(false);
        rotatingObject.GetComponent<Rotator>().enabled = true;
    }
}

