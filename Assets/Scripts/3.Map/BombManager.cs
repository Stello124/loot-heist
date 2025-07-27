using UnityEngine;
using System.Collections;
using System.Linq;

public class BombManager : MonoBehaviour
{
    public static BombManager Instance;

    public GameObject bombPrefab;
    public float bombTimer = 15f;

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
            Debug.LogError("RightHand bulunamadý! Prefabda doðru isimli nesne olduðundan emin ol.");
            return;
        }

        // Bombayý spawn et ve ele yapýþtýr
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

        // Patlat
        Debug.Log(currentBombHolder.name + "patladý!");
        Destroy(currentBombHolder);
        currentBombHolder = null;

        yield return new WaitForSeconds(1.5f);
        AssignBombToRandomPlayer();
    }

    public GameObject GetCurrentBombHolder()
    {
        return currentBombHolder;
    }
}


