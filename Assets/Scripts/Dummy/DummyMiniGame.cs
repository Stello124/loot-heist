using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class DummyMiniGame : NetworkBehaviour
{
    public float gameDuration = 5f;       // Oyun süresi (saniye)
    public float resultsDuration = 35f;   // Sýralama ekraný süresi (saniye)

    private bool gameEnded = false;

    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(RunGame());
        }
    }

    private IEnumerator RunGame()
    {
        Debug.Log("Mini oyun baþladý.");
        yield return new WaitForSeconds(gameDuration);

        gameEnded = true;
        Debug.Log("Mini oyun bitti, sýralama ekraný baþladý.");

        // Sýralama ekraný için bekle
        yield return new WaitForSeconds(resultsDuration);

        Debug.Log("Sýralama ekraný bitti, diðer sahneye geçiliyor.");

        // Burada sýradaki sahneye geçiþ tetiklenir, örneðin:
        TournamentManager.Instance.SonrakiSahneyeGec();
    }
}
