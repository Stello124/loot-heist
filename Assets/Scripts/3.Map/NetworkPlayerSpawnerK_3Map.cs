using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class NetworkPlayerSpawnerK_3Map : NetworkBehaviour
{
    [Header("3.Map Bomb Game Settings")]
    [SerializeField] private List<GameObject> spawnablePrefabs;
    [SerializeField] private Transform[] bombSpawnPoints; // 3.map spawn point'leri
    [SerializeField] private int maxPlayers = 4;
    
    private HashSet<ulong> spawnedClients = new HashSet<ulong>();
    
    // NetworkSpawnManager3Map uyumluluğu için static liste
    private static List<GameObject> networkPlayersList = new List<GameObject>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("💣 NetworkPlayerSpawnerK_3Map aktif - 3.Map bomba sistemi!");

        // Event'lere abone ol
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // Mevcut client'ları spawn et
        StartCoroutine(SpawnExistingClients());
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private IEnumerator SpawnExistingClients()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayerForClient(clientId, "Existing Client");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"💣 Yeni bomba oyuncusu bağlandı: {clientId}");
        
        if (spawnedClients.Count >= maxPlayers)
        {
            Debug.Log($"⚠️ Maksimum bomba oyuncusu sayısı ({maxPlayers}) doldu - Client {clientId} beklemeye alındı");
            return;
        }
        
        SpawnPlayerForClient(clientId, "New Connection");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"❌ Bomba oyuncusu ayrıldı: {clientId}");
        spawnedClients.Remove(clientId);
        
        // Static listeden de çıkar
        RemovePlayerFromStaticList(clientId);
    }

    private void SpawnPlayerForClient(ulong clientId, string reason)
    {
        if (spawnedClients.Contains(clientId))
        {
            Debug.Log($"⚠️ Client {clientId} zaten spawn edildi. ({reason})");
            return;
        }

        // PlayerObject varsa spawn etme
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData) &&
            clientData.PlayerObject != null)
        {
            Debug.Log($"⚠️ Client {clientId} zaten PlayerObject'e sahip. ({reason})");
            return;
        }

        string prefabId = GetPrefabIdForClient(clientId);
        GameObject selectedPrefab = GetPrefab(prefabId);

        if (selectedPrefab == null)
        {
            Debug.LogError($"❌ Prefab bulunamadı: {prefabId} (Client: {clientId})");
            return;
        }

        // 💣 3.Map spawn point'leri kullan  
        int clientSpawnIndex = GetSpawnIndexForClient(clientId);
        Vector3 spawnPos = GetBombSpawnPosition(clientSpawnIndex);
        Quaternion spawnRot = GetBombSpawnRotation(clientSpawnIndex);
        
        GameObject obj = Instantiate(selectedPrefab, spawnPos, spawnRot);
        NetworkObject netObj = obj.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError($"❌ NetworkObject eksik: {selectedPrefab.name}");
            Destroy(obj);
            return;
        }

        // PlayerObject olarak spawn et
        netObj.SpawnAsPlayerObject(clientId, true);
        spawnedClients.Add(clientId);

        // Static listeye ekle (BombManager uyumluluğu için)
        AddPlayerToStaticList(obj);

        // Özelleştirme uygula (SENİN MANUEL KODUN!)
        ApplyCustomization(obj, clientId);
        
        // 💣 3.Map için BoostReceiver ekle
        Add3MapSpecificComponents(obj);

        Debug.Log($"💣 Bomba oyuncusu spawn edildi: {selectedPrefab.name} → Pozisyon: {clientSpawnIndex} → Client: {clientId} ({reason})");
    }

    // 💣 3.Map için client ID'ye göre spawn index
    private int GetSpawnIndexForClient(ulong clientId)
    {
        var sortedClientIds = NetworkManager.Singleton.ConnectedClientsIds.OrderBy(id => id).ToList();
        int index = sortedClientIds.IndexOf(clientId);
        
        Debug.Log($"💣 Client {clientId} bomba spawn index: {index}");
        return index;
    }
    
    private Vector3 GetBombSpawnPosition(int spawnIndex)
    {
        if (bombSpawnPoints != null && bombSpawnPoints.Length > 0)
        {
            Transform spawnPoint = bombSpawnPoints[spawnIndex % bombSpawnPoints.Length];
            return spawnPoint.position;
        }
        else
        {
            // Fallback: Bomba arena'sında daire şeklinde spawn
            float angle = (360f / maxPlayers) * spawnIndex;
            float radius = 3f;
            return new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );
        }
    }
    
    private Quaternion GetBombSpawnRotation(int spawnIndex)
    {
        if (bombSpawnPoints != null && bombSpawnPoints.Length > 0)
        {
            Transform spawnPoint = bombSpawnPoints[spawnIndex % bombSpawnPoints.Length];
            return spawnPoint.rotation;
        }
        else
        {
            return Quaternion.identity;
        }
    }

    // SENİN MANUEL KODLARIN - AYNI! ✅
    private string GetPrefabIdForClient(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            return GameState.LocalPlayerData?.PrefabId ?? "palyaco";
        }
        else
        {
            return "palyaco";
        }
    }

    private GameObject GetPrefab(string prefabId)
    {
        GameObject selectedPrefab = spawnablePrefabs.Find(p => p.name == prefabId);

        if (selectedPrefab == null)
        {
            selectedPrefab = Resources.Load<GameObject>($"Characters/{prefabId}");
            if (selectedPrefab != null)
            {
                Debug.Log($"📦 Resources'tan prefab yüklendi: {prefabId}");
            }
        }

        return selectedPrefab;
    }

    private void ApplyCustomization(GameObject playerObj, ulong clientId)
    {
        var builder = playerObj.GetComponent<CharacterBuilder>();

        if (builder != null)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId && GameState.LocalPlayerData != null)
            {
                builder.ApplyCustomization(GameState.LocalPlayerData);
                Debug.Log($"🎨 Client {clientId} için kayıtlı özelleştirme uygulandı.");
            }
            else
            {
                Debug.Log($"🎨 Client {clientId} için default özelleştirme uygulanacak.");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ CharacterBuilder bulunamadı - Client: {clientId}");
        }
    }
    
    // 💣 3.Map için özel component'ler ekle
    private void Add3MapSpecificComponents(GameObject playerObj)
    {
        if (playerObj.GetComponent<BoostReceiver>() == null)
        {
            var boostReceiver = playerObj.AddComponent<BoostReceiver>();
            Debug.Log($"💣 BoostReceiver eklendi (3.Map için): {playerObj.name}");
        }
    }
    
    // BombManager uyumluluğu için static liste
    private void AddPlayerToStaticList(GameObject player)
    {
        if (!networkPlayersList.Contains(player))
        {
            networkPlayersList.Add(player);
            Debug.Log($"➕ Player static listeye eklendi: {player.name} (Toplam: {networkPlayersList.Count})");
        }
    }
    
    private void RemovePlayerFromStaticList(ulong clientId)
    {
        for (int i = networkPlayersList.Count - 1; i >= 0; i--)
        {
            if (networkPlayersList[i] == null)
            {
                networkPlayersList.RemoveAt(i);
                continue;
            }
            
            NetworkObject netObj = networkPlayersList[i].GetComponent<NetworkObject>();
            if (netObj != null && netObj.OwnerClientId == clientId)
            {
                networkPlayersList.RemoveAt(i);
                Debug.Log($"➖ Player static listeden çıkarıldı: ClientID {clientId}");
                break;
            }
        }
    }
    
    // NetworkSpawnManager3Map uyumluluğu için
    public static List<GameObject> GetAllNetworkPlayers()
    {
        return networkPlayersList;
    }

    // Spawn pointleri görselleştirmek için
    void OnDrawGizmos()
    {
        if (bombSpawnPoints == null) return;
        
        for (int i = 0; i < bombSpawnPoints.Length; i++)
        {
            if (bombSpawnPoints[i] == null) continue;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bombSpawnPoints[i].position, 0.5f);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(bombSpawnPoints[i].position, bombSpawnPoints[i].forward * 2f);
            
#if UNITY_EDITOR
            UnityEditor.Handles.Label(bombSpawnPoints[i].position + Vector3.up, $"Bomb {i + 1}");
#endif
        }
    }
}