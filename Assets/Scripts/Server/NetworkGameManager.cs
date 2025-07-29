using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private HashSet<ulong> spawnedClients = new HashSet<ulong>();

    void OnEnable()
    {
        NetworkManager.OnClientConnectedCallback += HandleClientConnected;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleSceneLoadComplete;
        }
    }

    void OnDisable()
    {
        NetworkManager.OnClientConnectedCallback -= HandleClientConnected;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleSceneLoadComplete;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("[Server] NetworkGameManager aktif oldu. Client baðlantýlarý ve sahne geçiþleri dinleniyor.");
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"[Server] Yeni client baðlandý: {clientId}");

        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            Debug.Log($"[Server] Client {clientId} zaten bir PlayerObject'e sahip, yeniden spawn edilmeyecek.");
            return;
        }

        TrySpawnClient(clientId, "HandleClientConnected");
    }

    private void HandleSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        Debug.Log($"[Server] Client {clientId} sahneyi yükledi: {sceneName}");

        TrySpawnClient(clientId, "HandleSceneLoadComplete");
    }

    private void TrySpawnClient(ulong clientId, string reason)
    {
        if (spawnedClients.Contains(clientId))
        {
            Debug.Log($"[Server] Client {clientId} zaten spawn edildi. ({reason})");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("[Server] playerPrefab Inspector'da tanýmlý deðil. Spawn iþlemi yapýlamaz.");
            return;
        }

        GameObject player = Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity);
        var netObj = player.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError("[Server] playerPrefab'ta NetworkObject component yok.");
            return;
        }

        netObj.SpawnAsPlayerObject(clientId, true);
        spawnedClients.Add(clientId);

        Debug.Log($"[Server] Client {clientId} için karakter spawn edildi. ({reason})");
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
    }
}