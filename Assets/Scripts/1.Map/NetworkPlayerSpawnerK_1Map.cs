using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class NetworkPlayerSpawnerK_1Map : NetworkBehaviour
{
    [Header("1.Map Race Settings")]
    [SerializeField] private List<GameObject> spawnablePrefabs;
    [SerializeField] private Transform[] raceSpawnPoints; // 1.map spawn point'leri
    [SerializeField] private int maxPlayers = 4;
    
    private HashSet<ulong> spawnedClients = new HashSet<ulong>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("🏁 NetworkPlayerSpawnerK_1Map aktif - 1.Map yarış sistemi!");

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
        Debug.Log($"🏁 Yeni yarışçı bağlandı: {clientId}");
        
        // Maksimum oyuncu kontrolü
        if (spawnedClients.Count >= maxPlayers)
        {
            Debug.Log($"⚠️ Maksimum yarışçı sayısı ({maxPlayers}) doldu - Client {clientId} beklemeye alındı");
            return;
        }
        
        SpawnPlayerForClient(clientId, "New Connection");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"❌ Yarışçı ayrıldı: {clientId}");
        spawnedClients.Remove(clientId);
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

        // 🏁 SADECE BURASINI DEĞİŞTİRDİM - Client ID'ye göre spawn point
        int clientSpawnIndex = GetSpawnIndexForClient(clientId);
        Vector3 spawnPos = GetRaceSpawnPosition(clientSpawnIndex);
        Quaternion spawnRot = GetRaceSpawnRotation(clientSpawnIndex);
        
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

        // Özelleştirme uygula (SENİN MANUEL KODUN - DEĞİŞTİRMEDİM!)
        ApplyCustomization(obj, clientId);

        Debug.Log($"🏁 Yarışçı spawn edildi: {selectedPrefab.name} → Pozisyon: {clientSpawnIndex} → Client: {clientId} ({reason})");
    }

    // 🏁 YENİ: Client ID'ye göre spawn index hesapla
    private int GetSpawnIndexForClient(ulong clientId)
    {
        // Host her zaman 0. pozisyonda (başlangıç çizgisi)
        if (clientId == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsHost)
        {
            return 0;
        }
        
        // Connected client'ları ID'ye göre sırala ve index ver
        var sortedClientIds = NetworkManager.Singleton.ConnectedClientsIds.OrderBy(id => id).ToList();
        int index = sortedClientIds.IndexOf(clientId);
        
        Debug.Log($"🏁 Client {clientId} spawn index: {index} (Sorted IDs: {string.Join(",", sortedClientIds)})");
        return index;
    }
    
    // 🏁 YENİ: 1.Map spawn point'leri için pozisyon hesaplama (index ile)
    private Vector3 GetRaceSpawnPosition(int spawnIndex)
    {
        if (raceSpawnPoints != null && raceSpawnPoints.Length > 0)
        {
            // Belirtilen index'teki spawn point kullan
            Transform spawnPoint = raceSpawnPoints[spawnIndex % raceSpawnPoints.Length];
            return spawnPoint.position;
        }
        else
        {
            // Fallback: Sıra halinde spawn (eski sistem)
            return new Vector3(spawnIndex * 2f, 0f, 0f);
        }
    }
    
    // 🏁 YENİ: 1.Map spawn point'leri için rotasyon (index ile)
    private Quaternion GetRaceSpawnRotation(int spawnIndex)
    {
        if (raceSpawnPoints != null && raceSpawnPoints.Length > 0)
        {
            Transform spawnPoint = raceSpawnPoints[spawnIndex % raceSpawnPoints.Length];
            return spawnPoint.rotation;
        }
        else
        {
            return Quaternion.identity;
        }
    }

    // SENİN MANUEL KODLARIN - HİÇ DEĞİŞTİRMEDİM! ✅
    private string GetPrefabIdForClient(ulong clientId)
    {
        // Bu metodda client'a özel prefab ID'sini al
        // Şimdilik GameState.LocalPlayerData kullanıyoruz ama
        // ileride client-specific data için RPC kullanabilirsin

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Local client (Host) için
            return GameState.LocalPlayerData?.PrefabId ?? "palyaco";
        }
        else
        {
            // Remote client için default (ileride RPC ile özelleştirilebilir)
            return "palyaco";
        }
    }

    private GameObject GetPrefab(string prefabId)
    {
        // Önce listeden ara
        GameObject selectedPrefab = spawnablePrefabs.Find(p => p.name == prefabId);

        // Listede yoksa Resources'tan yükle
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
                // Local client için kayıtlı özelleştirme
                builder.ApplyCustomization(GameState.LocalPlayerData);
                Debug.Log($"🎨 Client {clientId} için kayıtlı özelleştirme uygulandı.");
            }
            else
            {
                // Remote client için default özelleştirme
                Debug.Log($"🎨 Client {clientId} için default özelleştirme uygulanacak.");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ CharacterBuilder bulunamadı - Client: {clientId}");
        }
    }

    // Spawn pointleri görselleştirmek için
    void OnDrawGizmos()
    {
        if (raceSpawnPoints == null) return;
        
        for (int i = 0; i < raceSpawnPoints.Length; i++)
        {
            if (raceSpawnPoints[i] == null) continue;
            
            // Yarış başlangıç pozisyonları
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(raceSpawnPoints[i].position, 0.5f);
            
            // Start direction göster
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(raceSpawnPoints[i].position, raceSpawnPoints[i].forward * 2f);
            
#if UNITY_EDITOR
            UnityEditor.Handles.Label(raceSpawnPoints[i].position + Vector3.up, $"Start {i + 1}");
#endif
        }
    }
}