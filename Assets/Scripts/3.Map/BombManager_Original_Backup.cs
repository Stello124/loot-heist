// ORIJINAL OFFLINE BOMBMANAGER YEDEGI - MULTIPLAYER DÖNÜŞÜM ÖNCESİ
// Bu dosya güvenlik amaçlı oluşturulmuştur.

using UnityEngine;
using System.Collections;
using System.Linq;

public class BombManager_Original_Backup : MonoBehaviour
{
    public static BombManager_Original_Backup Instance;

    public GameObject bombPrefab;
    public float bombTimer = 555f;

    private GameObject currentBombHolder;
    private GameObject bombVisual;
    private Coroutine countdownCoroutine;

    void Awake()
    {
        Instance = this;
    }

    public void AssignBombToRandomPlayer()
    {
        int randomIndex = Random.Range(0, SpawnManager.allPlayers.Count);
        SetBombHolder(SpawnManager.allPlayers[randomIndex]);
    }

    public void SetBombHolder(GameObject newHolder)
    {
        if (currentBombHolder == newHolder) return;

        if (bombVisual != null)
            Destroy(bombVisual);

        currentBombHolder = newHolder;

        // Karakterin içindeki "RightHand" objesini bul
        Transform hand = currentBombHolder.GetComponentsInChildren<Transform>()
            .FirstOrDefault(t => t.name == "RightHand");

        if (hand == null)
        {
            Debug.LogError("RightHand bulunamadı! Prefabda doğru isimli nesne olduğundan emin ol.");
            return;
        }

        // Bombayı spawn et ve ele yapıştır
        bombVisual = Instantiate(bombPrefab, hand.position, hand.rotation, hand);
        bombVisual.transform.localPosition = Vector3.zero;

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(BombCountdown());
    }

    IEnumerator BombCountdown()
    {
        float time = bombTimer;
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        // Bomba patladı
        Debug.Log(currentBombHolder.name + " patladı!");
        SpawnManager.allPlayers.Remove(currentBombHolder);  // Listeden çıkar
        Destroy(currentBombHolder);
        currentBombHolder = null;

        yield return new WaitForSeconds(1f);

        // Kalan oyuncu sayısını kontrol et
        if (SpawnManager.allPlayers.Count == 1)
        {
            GameObject winner = SpawnManager.allPlayers[0];
            Debug.Log(winner.name + " kazandı!");

            GameUI.Instance.ShowWinText(winner.name); // UI'ya haber ver
            Time.timeScale = 0f; // Oyunu durdur
        }
        else
        {
            AssignBombToRandomPlayer();
        }
    }

    // Erişim için public getter
    public GameObject GetCurrentBombHolder()
    {
        return currentBombHolder;
    }
}