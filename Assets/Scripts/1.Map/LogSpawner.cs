using UnityEngine;

public class LogSpawner : MonoBehaviour
{
    public GameObject logPrefab;     // Kütük prefabı
    public float spawnDelay = 5f;    // Kaç saniyede bir spawn etsin
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnDelay)
        {
            SpawnLog();
            timer = 0f;
        }
    }

    void SpawnLog()
    {
        Instantiate(logPrefab, transform.position, transform.rotation);
    }
}
