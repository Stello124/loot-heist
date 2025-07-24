using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject playerPrefab;    // Karakter prefabý
    public Transform[] spawnPoints;    // Spawn noktalarý
    public int playerCount = 4;        // Spawn olacak oyuncu sayýsý

    void Start()
    {
        for (int i = 0; i < playerCount; i++)
        {
            int spawnIndex = i % spawnPoints.Length;    // Spawn noktalarýný sýrayla seç
            Transform spawnPoint = spawnPoints[spawnIndex];

            Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}



