using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MiniGameManager : NetworkBehaviour
{
    private IMiniGame currentMiniGame;
    private bool gameEnded = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(CheckGameState());
    }

    IEnumerator CheckGameState()
    {
        yield return new WaitForSeconds(3f); // Mini oyun başlamış olsun

        currentMiniGame = FindObjectOfType<MonoBehaviour>() as IMiniGame;

        while (!gameEnded)
        {
            if (currentMiniGame != null && currentMiniGame.IsGameOver())
            {
                gameEnded = true;
                Dictionary<ulong, int> scores = currentMiniGame.GetPlayerScores();
                ApplyScore(scores);

                yield return StartCoroutine(ShowRankingUI(35f));
                TournamentManager.Instance.SonrakiSahneyeGec();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void ApplyScore(Dictionary<ulong, int> playerScores)
    {
        foreach (var pair in playerScores)
        {
            Debug.Log($"Oyuncu {pair.Key} puan aldı: {pair.Value}");
            // 🔥 Burada oyuncuya özel skor verisini bir yere yazacağız (ileride eklenecek)
        }
    }

    IEnumerator ShowRankingUI(float duration)
    {
        // TODO: Sıralama UI gösterilecek
        Debug.Log("🏁 Sıralama ekranı gösteriliyor...");
        yield return new WaitForSeconds(duration);
    }
}
