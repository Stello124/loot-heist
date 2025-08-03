using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NetworkSpawnManager3Map : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<GameObject> spawnablePrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool useSpawnPoints = true;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    // Network liste - tüm spawn edilmiş oyuncular
    public static NetworkList<NetworkObjectReference> networkPlayers;
    
    // Spawn edilmiş client'ları takip et
    private HashSet<ulong> spawnedClients = new HashSet<ulong>();
    private int currentSpawnPointIndex = 0;

    void Awake()
    {
        // NetworkList'i başlat
        if (networkPlayers == null)
        {
            networkPlayers = new NetworkList<NetworkObjectReference>();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.LogWarning("⚠️ NetworkSpawnManager3Map DEPRECATED! GlobalPlayerSpawner kullanın.");
        return; // Devre dışı
        
        DebugLog("🚀 NetworkSpawnManager3Map aktif - Server modunda");

        // Event'lere abone ol
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // NetworkList değişikliklerini dinle
        networkPlayers.OnListChanged += OnPlayerListChanged;

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

        if (networkPlayers != null)
        {
            networkPlayers.OnListChanged -= OnPlayerListChanged;
        }
    }

    private IEnumerator SpawnExistingClients()
    {
        yield return new WaitForSeconds(0.2f);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayerForClient(clientId, "Existing Client");
        }

        // BombManager artık otomatik başlıyor, manuel çağrı gerekmiyor
        yield return new WaitForSeconds(0.5f);
        Debug.Log("🎮 NetworkSpawnManager3Map: BombManager otomatik olarak başlayacak");
    }

    private void OnClientConnected(ulong clientId)
    {
        DebugLog($"🌐 Yeni client bağlandı: {clientId}");
        SpawnPlayerForClient(clientId, "New Connection");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        DebugLog($"❌ Client ayrıldı: {clientId}");
        spawnedClients.Remove(clientId);
        
        // Network listesinden de çıkar
        RemovePlayerFromNetworkList(clientId);
    }

    private void SpawnPlayerForClient(ulong clientId, string reason)
    {
        if (spawnedClients.Contains(clientId))
        {
            DebugLog($"⚠️ Client {clientId} zaten spawn edildi. ({reason})");
            return;
        }

        // PlayerObject kontrolü
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData) && 
            clientData.PlayerObject != null)
        {
            DebugLog($"⚠️ Client {clientId} zaten PlayerObject'e sahip. ({reason})");
            return;
        }

        string prefabId = GetPrefabIdForClient(clientId);
        GameObject selectedPrefab = GetPrefab(prefabId);

        if (selectedPrefab == null)
        {
            Debug.LogError($"❌ Prefab bulunamadı: {prefabId} (Client: {clientId})");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        Quaternion spawnRot = GetSpawnRotation();
        
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

        // Network listesine ekle
        AddPlayerToNetworkList(netObj);

        // Özelleştirme uygula
        ApplyCustomization(obj, clientId);
        
        // 3.Map için BoostReceiver ekle (sadece bu map için!)
        Add3MapSpecificComponents(obj);

        DebugLog($"✅ Spawn edildi: {selectedPrefab.name} → Client: {clientId} ({reason})");
    }

    private Vector3 GetSpawnPosition()
    {
        if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[currentSpawnPointIndex % spawnPoints.Length];
            currentSpawnPointIndex++;
            return spawnPoint.position;
        }
        else
        {
            // Random pozisyon
            return new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        }
    }

    private Quaternion GetSpawnRotation()
    {
        if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = (currentSpawnPointIndex - 1) % spawnPoints.Length;
            if (index < 0) index = 0;
            return spawnPoints[index].rotation;
        }
        else
        {
            return Quaternion.identity;
        }
    }

    private string GetPrefabIdForClient(ulong clientId)
    {
        // Her zaman palyaco prefab kullan - cloud'dan sadece customization çekiliyor
        string prefabId = "palyaco";
        
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Local client için GameState'den prefab kontrol et ama fallback palyaco
            prefabId = GameState.LocalPlayerData?.PrefabId ?? "palyaco";
            DebugLog($"📦 Local client prefab: {prefabId} (Client: {clientId})");
        }
        else
        {
            // Remote client için de palyaco - customization RPC ile gelecek
            DebugLog($"📦 Remote client prefab: {prefabId} (Client: {clientId})");
        }
        
        return prefabId;
    }

    private GameObject GetPrefab(string prefabId)
    {
        // Önce listeden ara
        GameObject selectedPrefab = spawnablePrefabs?.Find(p => p.name == prefabId);

        // Listede yoksa Resources'tan yükle
        if (selectedPrefab == null)
        {
            selectedPrefab = Resources.Load<GameObject>($"Characters/{prefabId}");
            if (selectedPrefab != null)
            {
                DebugLog($"📦 Resources'tan prefab yüklendi: {prefabId}");
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
                DebugLog($"🎨 Client {clientId} için kayıtlı özelleştirme uygulandı.");
            }
            else
            {
                // Remote client için default özelleştirme
                DebugLog($"🎨 Client {clientId} için default özelleştirme uygulanacak.");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ CharacterBuilder bulunamadı - Client: {clientId}");
        }
    }
    
    private void Add3MapSpecificComponents(GameObject playerObj)
    {
        // Sadece 3.Map için gereken component'leri ekle
        if (playerObj.GetComponent<BoostReceiver>() == null)
        {
            var boostReceiver = playerObj.AddComponent<BoostReceiver>();
            DebugLog($"🔧 BoostReceiver eklendi (sadece 3.Map için): {playerObj.name}");
        }
    }

    private void AddPlayerToNetworkList(NetworkObject playerNetObj)
    {
        if (IsServer && networkPlayers != null)
        {
            networkPlayers.Add(playerNetObj);
            DebugLog($"➕ Oyuncu network listesine eklendi. Toplam: {networkPlayers.Count}");
        }
    }

    private void RemovePlayerFromNetworkList(ulong clientId)
    {
        if (!IsServer || networkPlayers == null) return;

        for (int i = networkPlayers.Count - 1; i >= 0; i--)
        {
            if (networkPlayers[i].TryGet(out NetworkObject netObj))
            {
                if (netObj.OwnerClientId == clientId)
                {
                    networkPlayers.RemoveAt(i);
                    DebugLog($"➖ Oyuncu network listesinden çıkarıldı. Toplam: {networkPlayers.Count}");
                    break;
                }
            }
            else
            {
                // Geçersiz referans, temizle
                networkPlayers.RemoveAt(i);
            }
        }
    }

    private void OnPlayerListChanged(NetworkListEvent<NetworkObjectReference> changeEvent)
    {
        DebugLog($"🔄 Player listesi değişti. Yeni sayı: {networkPlayers.Count}");
        
        // BombManager artık otomatik çalışıyor, manuel player count değişikliği gerekmiyor
        Debug.Log("🎮 NetworkSpawnManager3Map: Player listesi değişti, BombManager otomatik yönetiyor");
    }

    private void StartBombGame()
    {
        if (!IsServer) return;

        DebugLog($"💣 Bomba oyunu başlatılıyor. Oyuncu sayısı: {networkPlayers.Count}");
        
        if (BombManager.Instance != null && networkPlayers.Count > 0)
        {
            // BombManager artık otomatik başlıyor
            Debug.Log("🎮 NetworkSpawnManager3Map: BombManager otomatik başlatıldı");
        }
    }

    // Public getter metodlar
    public static List<GameObject> GetAllNetworkPlayers()
    {
        List<GameObject> players = new List<GameObject>();
        
        if (networkPlayers != null)
        {
            foreach (var playerRef in networkPlayers)
            {
                if (playerRef.TryGet(out NetworkObject netObj) && netObj != null)
                {
                    players.Add(netObj.gameObject);
                }
            }
        }
        
        return players;
    }

    public static int GetPlayerCount()
    {
        return networkPlayers?.Count ?? 0;
    }

    public static GameObject GetRandomPlayer()
    {
        var players = GetAllNetworkPlayers();
        if (players.Count > 0)
        {
            return players[Random.Range(0, players.Count)];
        }
        return null;
    }

    // Geriye uyumluluk için eski SpawnManager interface'i
    public static List<GameObject> allPlayers
    {
        get { return GetAllNetworkPlayers(); }
    }

    private void DebugLog(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[NetworkSpawnManager3Map] {message}");
        }
    }

    // Spawn pointleri görselleştirmek için
    void OnDrawGizmos()
    {
        if (spawnPoints == null || !useSpawnPoints) return;
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null) continue;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(spawnPoints[i].position, spawnPoints[i].forward * 2f);
            
#if UNITY_EDITOR
            UnityEditor.Handles.Label(spawnPoints[i].position + Vector3.up, $"SP {i}");
#endif
        }
    }
}