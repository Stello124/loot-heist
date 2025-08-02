// NOT: Bu script artık kullanılmıyor!
// NetworkSpawnManager3Map kullanın.
// Sadece geriye uyumluluk için bırakılmıştır.

using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;
    public int playerCount = 4;

    // DEPRECATED: NetworkSpawnManager3Map.allPlayers kullanın
    public static List<GameObject> allPlayers
    {
        get { return NetworkSpawnManager3Map.GetAllNetworkPlayers(); }
    }

    void Start()
    {
        Debug.LogWarning("⚠️ SpawnManager DEPRECATED! NetworkSpawnManager3Map kullanın.");
        
        // Offline modda çalışır ama multiplayer'da devre dışı
        if (Unity.Netcode.NetworkManager.Singleton == null)
        {
            // Offline mode
            for (int i = 0; i < playerCount; i++)
            {
                int spawnIndex = i % spawnPoints.Length;
                Transform spawnPoint = spawnPoints[spawnIndex];

                GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                player.name = "Player" + (i + 1);
            }
            
            if (BombManager.Instance != null)
            {
                BombManager.Instance.StartBombGame();
            }
        }
    }
}







