using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject playerPrefab; // Karakter prefabýn
    public Transform[] spawnPoints; // Spawn noktalarý

    void Start()
    {
        // Rastgele bir spawn point seç
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        // Karakteri doður
        Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}

