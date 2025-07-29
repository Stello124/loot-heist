using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("[Server] NetworkGameManager aktif oldu. Client baðlantýlarý dinleniyor.");
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"[Server] Yeni client baðlandý: {clientId}");

        // Eðer zaten spawn ettiysek tekrar etme
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            Debug.Log($"[Server] Client {clientId} zaten bir PlayerObject'e sahip, yeniden spawn edilmeyecek.");
            return;
        }

        // Prefab null kontrolü
        if (playerPrefab == null)
        {
            Debug.LogError("[Server] playerPrefab Inspector'da tanýmlý deðil. Spawn iþlemi yapýlamaz.");
            return;
        }

        // Oyuncu prefabýný spawn et
        GameObject player = Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity);

        var netObj = player.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[Server] playerPrefab'ta NetworkObject component yok. SpawnAsPlayerObject çalýþmaz.");
            return;
        }

        netObj.SpawnAsPlayerObject(clientId, true);
        Debug.Log($"[Server] Client {clientId} için karakter spawn edildi.");
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
    }
}