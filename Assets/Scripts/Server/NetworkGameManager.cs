using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private HashSet<ulong> spawnedClients = new HashSet<ulong>();

    void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.OnClientConnectedCallback += HandleClientConnected;
            StartCoroutine(BindSceneManagerSafely());
        }
        else
        {
            Debug.LogError("NetworkManager null: OnEnable içinde tetiklenemez.");
        }
    }

    private IEnumerator BindSceneManagerSafely()
    {
        float timeout = 3f;
        float elapsed = 0f;

        while (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null)
        {
            yield return null;
            elapsed += Time.deltaTime;

            if (elapsed > timeout)
            {
                Debug.LogWarning("❌ SceneManager bağlanamadı (timeout).");
                yield break;
            }
        }

        NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleSceneLoadComplete;
        Debug.Log("✅ SceneManager event'e abone olundu.");
    }

    void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            // Unsubscribe güvenli bir şekilde, null kontrol gerekmez.
            NetworkManager.OnClientConnectedCallback -= HandleClientConnected;

            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleSceneLoadComplete;
            }
            else
            {
                Debug.LogWarning("OnDisable sırasında SceneManager null, unsubscribe yapılamadı.");
            }
        }
        else
        {
            Debug.LogWarning("OnDisable sırasında NetworkManager.Singleton null, unsubscribe yapılamadı.");
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("[Server] NetworkGameManager aktif oldu. Client bağlantıları ve sahne geçişleri dinleniyor.");
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"[Server] Yeni client bağlandı: {clientId}");

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
            Debug.LogError("[Server] playerPrefab Inspector'da tanımlı değil. Spawn işlemi yapılamaz.");
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