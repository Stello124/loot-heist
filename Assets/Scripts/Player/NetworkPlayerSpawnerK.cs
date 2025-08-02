using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;

public class NetworkPlayerSpawnerK : NetworkBehaviour
{
    [SerializeField] private List<GameObject> spawnablePrefabs;
    private HashSet<ulong> spawnedClients = new HashSet<ulong>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("🚀 NetworkPlayerSpawnerK aktif - Server modunda");

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
        Debug.Log($"🌐 Yeni client bağlandı: {clientId}");
        SpawnPlayerForClient(clientId, "New Connection");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"❌ Client ayrıldı: {clientId}");
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

        Vector3 spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        GameObject obj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
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

        // Özelleştirme uygula
        ApplyCustomization(obj, clientId);

        Debug.Log($"✅ Spawn edildi: {selectedPrefab.name} → Client: {clientId} ({reason})");
    }

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
}