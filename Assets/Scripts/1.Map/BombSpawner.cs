using UnityEngine;

public class BombSpawner : MonoBehaviour
{
    public GameObject bombPrefab;         // Bomba prefabı
    public float minDelay = 4f;           // Minimum spawn süresi
    public float maxDelay = 6f;           // Maksimum spawn süresi

    private float timer = 0f;
    private float nextSpawnTime = 0f;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnBomb();
            timer = 0f;
            SetNextSpawnTime(); // Yeni rastgele süre belirle
        }
    }

    void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minDelay, maxDelay);
    }

    void SpawnBomb()
    {
        Instantiate(bombPrefab, transform.position, transform.rotation);
    }
}
