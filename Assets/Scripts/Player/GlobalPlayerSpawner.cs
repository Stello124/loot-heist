using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using Controller;

public class GlobalPlayerSpawner : NetworkBehaviour
{
    [Header("Global Spawn Settings")]
    [SerializeField] private List<GameObject> spawnablePrefabs;
    
    // Singleton - Tek instance
    public static GlobalPlayerSpawner Instance;
    
    // Tüm spawn edilmiş oyuncular
    private static List<GameObject> allPlayers = new List<GameObject>();
    private HashSet<ulong> spawnedClients = new HashSet<ulong>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("🌍 GlobalPlayerSpawner oluşturuldu - DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("🗑️ Duplicate GlobalPlayerSpawner destroyed");
            Destroy(gameObject);
            return;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("🚀 GlobalPlayerSpawner Server aktif!");

        // Network event'lere bağlan
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
        // Scene değişim event'i
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Mevcut client'ları spawn et
        StartCoroutine(SpawnExistingClientsDelayed());
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private IEnumerator SpawnExistingClientsDelayed()
    {
        yield return new WaitForSeconds(0.2f);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!spawnedClients.Contains(clientId))
            {
                SpawnPlayerForClient(clientId, "Initial Spawn");
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"🌐 Client bağlandı: {clientId}");
        
        if (!spawnedClients.Contains(clientId))
        {
            SpawnPlayerForClient(clientId, "Client Connected");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"❌ Client ayrıldı: {clientId}");
        spawnedClients.Remove(clientId);
        RemovePlayerFromList(clientId);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsServer) return;
        
        Debug.Log($"🌍 Scene yüklendi: {scene.name} - Oyuncuları yeniden spawn ediliyor");
        
        // Scene değişince tüm oyuncuları yeniden spawn et
        StartCoroutine(RespawnAllPlayersAfterSceneLoad());
    }

    private IEnumerator RespawnAllPlayersAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.3f); // Scene tamamen yüklensin
        
        // Mevcut oyuncuları temizle
        ClearPlayerList();
        spawnedClients.Clear();
        
        // Herkesi yeniden spawn et
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayerForClient(clientId, "Scene Respawn");
        }
    }

    private void SpawnPlayerForClient(ulong clientId, string reason)
    {
        Debug.Log($"🚀 SpawnPlayerForClient çağrıldı! ClientID: {clientId}, Reason: {reason}");
        
        if (spawnedClients.Contains(clientId))
        {
            Debug.Log($"⚠️ Client {clientId} zaten spawn edildi ({reason})");
            return;
        }

        // PlayerObject varsa skip
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData) &&
            clientData.PlayerObject != null)
        {
            Debug.Log($"⚠️ Client {clientId} zaten PlayerObject'e sahip ({reason})");
            return;
        }

        // Prefab al
        string prefabId = GetPrefabIdForClient(clientId);
        GameObject selectedPrefab = GetPrefab(prefabId);

        if (selectedPrefab == null)
        {
            Debug.LogError($"❌ Prefab bulunamadı: {prefabId} (Client: {clientId})");
            return;
        }

        // Spawn pozisyonu hesapla
        Debug.Log($"📍 Spawn pozisyonu hesaplanıyor... ClientID: {clientId}, Scene: {SceneManager.GetActiveScene().name}");
        Vector3 spawnPos = GetSpawnPositionForCurrentScene(clientId);
        Quaternion spawnRot = GetSpawnRotationForCurrentScene(clientId);
        Debug.Log($"📍 Final spawn pozisyonu: {spawnPos}");

        // Instantiate ve spawn
        GameObject obj = Instantiate(selectedPrefab, spawnPos, spawnRot);
        NetworkObject netObj = obj.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError($"❌ NetworkObject eksik: {selectedPrefab.name}");
            Destroy(obj);
            return;
        }

        // Network spawn
        netObj.SpawnAsPlayerObject(clientId, true);
        spawnedClients.Add(clientId);

        // Listeye ekle
        AddPlayerToList(obj);

        // Customization uygula
        ApplyPlayerCustomization(obj, clientId);
        
        // Scene-specific component'ler ekle
        AddSceneSpecificComponents(obj);

        Debug.Log($"✅ Player spawn edildi: {selectedPrefab.name} → Scene: {SceneManager.GetActiveScene().name} → Client: {clientId} ({reason})");
        
        // RaceGameManager'a yeni oyuncu bildirimi (1.map için)
        NotifyRaceGameManager(clientId);
    }

    private void NotifyRaceGameManager(ulong clientId)
    {
        // Sadece 1.map'te çalışsın
        if (SceneManager.GetActiveScene().name != "1.map") return;

        var raceGameManager = FindObjectOfType<RaceGameManager>();
        if (raceGameManager != null)
        {
            Debug.Log($"🏁 RaceGameManager'a yeni oyuncu bildiriliyor: {clientId}");
            raceGameManager.OnNewPlayerSpawned(clientId);
        }
        else
        {
            Debug.Log("⚠️ RaceGameManager bulunamadı (1.map değil veya henüz yüklenmedi)");
        }
    }



    // 🎨 CUSTOMIZATION UYGULA (SENİN KODUN)
    private void ApplyPlayerCustomization(GameObject playerObj, ulong clientId)
    {
        var builder = playerObj.GetComponent<CharacterBuilder>();

        if (builder != null)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId && GameState.LocalPlayerData != null)
            {
                // Local client için özelleştirme
                builder.ApplyCustomization(GameState.LocalPlayerData);
                Debug.Log($"🎨 Local client customization uygulandı: {clientId}");
            }
            else
            {
                // Remote client için default
                Debug.Log($"🎨 Remote client default customization: {clientId}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ CharacterBuilder bulunamadı: {playerObj.name}");
        }
    }

    // 📦 PREFAB ALMA (SENİN KODUN)
    private string GetPrefabIdForClient(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId && GameState.LocalPlayerData != null)
        {
            return GameState.LocalPlayerData.PrefabId ?? "palyaco";
        }
        return "palyaco";
    }

    private GameObject GetPrefab(string prefabId)
    {
        // Önce listeden bul
        GameObject selectedPrefab = spawnablePrefabs.Find(p => p.name == prefabId);

        // Listede yoksa Resources'tan yükle
        if (selectedPrefab == null)
        {
            selectedPrefab = Resources.Load<GameObject>($"Characters/{prefabId}");
            if (selectedPrefab != null)
            {
                Debug.Log($"📦 Resources'tan yüklendi: {prefabId}");
            }
        }

        return selectedPrefab;
    }

    // 🌍 SCENE'E GÖRE SPAWN POSİTİON
    private Vector3 GetSpawnPositionForCurrentScene(ulong clientId)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int playerIndex = GetPlayerIndex(clientId);

        switch (sceneName)
        {
            case "1.map": // Yarış
                return GetRaceSpawnPosition(playerIndex);
                
            case "3.map": // Bomba
                return GetBombSpawnPosition(playerIndex);
                
            case "4.map": // Köprü
                return GetBridgeSpawnPosition(playerIndex);
                
            case "DeneyK2": // Tırmanma
                return GetClimbSpawnPosition(playerIndex);
                
            default:
                return GetDefaultSpawnPosition(playerIndex);
        }
    }

    private Quaternion GetSpawnRotationForCurrentScene(ulong clientId)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int playerIndex = GetPlayerIndex(clientId);

        switch (sceneName)
        {
            case "1.map":
                return GetRaceSpawnRotation(playerIndex);
            case "3.map":
                return GetBombSpawnRotation(playerIndex);
            case "4.map":
                return GetBridgeSpawnRotation(playerIndex);
            case "DeneyK2":
                return GetClimbSpawnRotation(playerIndex);
            default:
                return Quaternion.identity;
        }
    }

    // 📊 PLAYER INDEX HESAPLA
    private int GetPlayerIndex(ulong clientId)
    {
        var sortedClientIds = NetworkManager.Singleton.ConnectedClientsIds.OrderBy(id => id).ToList();
        int index = sortedClientIds.IndexOf(clientId);
        
        Debug.Log($"🔢 GetPlayerIndex - ClientID: {clientId}");
        Debug.Log($"🔢 Connected IDs: [{string.Join(", ", sortedClientIds)}]");
        Debug.Log($"🔢 Calculated Index: {index}");
        
        return Mathf.Max(0, index);
    }

    // 🏁 1.MAP - YARIŞ SPAWN POSİTİONLARI
    private Vector3 GetRaceSpawnPosition(int playerIndex)
    {
        Transform[] spawnPoints = FindSpawnPointsInScene("RaceSpawnPoints");
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[playerIndex % spawnPoints.Length].position;
        }
        // Fallback: Start line pozisyonları
        return new Vector3(playerIndex * 2f, 0f, 0f);
    }

    private Quaternion GetRaceSpawnRotation(int playerIndex)
    {
        Transform[] spawnPoints = FindSpawnPointsInScene("RaceSpawnPoints");
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[playerIndex % spawnPoints.Length].rotation;
        }
        return Quaternion.identity;
    }

    // 💣 3.MAP - BOMBA SPAWN POSİTİONLARI
    private Vector3 GetBombSpawnPosition(int playerIndex)
    {
        Transform[] spawnPoints = FindSpawnPointsInScene("BombSpawnPoints");
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[playerIndex % spawnPoints.Length].position;
        }
        // Fallback: Daire şeklinde spawn
        float angle = (360f / 4f) * playerIndex;
        float radius = 3f;
        return new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
            0f,
            Mathf.Sin(angle * Mathf.Deg2Rad) * radius
        );
    }

    private Quaternion GetBombSpawnRotation(int playerIndex)
    {
        Transform[] spawnPoints = FindSpawnPointsInScene("BombSpawnPoints");
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[playerIndex % spawnPoints.Length].rotation;
        }
        return Quaternion.identity;
    }

    // 🌉 4.MAP - KÖPRÜ SPAWN POSİTİONLARI
    private Vector3 GetBridgeSpawnPosition(int playerIndex)
    {
        Transform[] spawnPoints = FindSpawnPointsInScene("BridgeSpawnPoints");
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[playerIndex % spawnPoints.Length].position;
        }
        return new Vector3(playerIndex * 2f, 0f, 0f);
    }

    private Quaternion GetBridgeSpawnRotation(int playerIndex)
    {
        Transform[] spawnPoints = FindSpawnPointsInScene("BridgeSpawnPoints");
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[playerIndex % spawnPoints.Length].rotation;
        }
        return Quaternion.identity;
    }

    // 🧗 DENEYK2 - TIRMANMA SPAWN POSİTİONLARI
    private Vector3 GetClimbSpawnPosition(int playerIndex)
    {
        Transform[] spawnPoints = FindSpawnPointsInScene("ClimbSpawnPoints");
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[playerIndex % spawnPoints.Length].position;
        }
        return new Vector3(playerIndex * 2f, 0f, 0f);
    }

    private Quaternion GetClimbSpawnRotation(int playerIndex)
    {
        Transform[] spawnPoints = FindSpawnPointsInScene("ClimbSpawnPoints");
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[playerIndex % spawnPoints.Length].rotation;
        }
        return Quaternion.identity;
    }

    // 🔧 DEFAULT SPAWN POSİTİON
    private Vector3 GetDefaultSpawnPosition(int playerIndex)
    {
        return new Vector3(playerIndex * 2f, 0f, 0f);
    }

    // 🔍 SPAWN POINT'LERİ BULMAK
    private Transform[] FindSpawnPointsInScene(string parentName)
    {
        Debug.Log($"🔍 FindSpawnPointsInScene başladı: '{parentName}'");
        
        // Specific parent ara
        GameObject parent = GameObject.Find(parentName);
        if (parent != null && parent.transform.childCount > 0)
        {
            Debug.Log($"✅ '{parentName}' bulundu! Child count: {parent.transform.childCount}");
            Transform[] points = new Transform[parent.transform.childCount];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = parent.transform.GetChild(i);
                Debug.Log($"   📍 Spawn Point {i}: {points[i].name} at {points[i].position}");
            }
            return points;
        }
        else
        {
            Debug.LogWarning($"⚠️ '{parentName}' bulunamadı veya child yok");
        }

        // Genel SpawnPoints ara
        parent = GameObject.Find("SpawnPoints");
        if (parent != null && parent.transform.childCount > 0)
        {
            Debug.Log($"✅ Genel 'SpawnPoints' bulundu! Child count: {parent.transform.childCount}");
            Transform[] points = new Transform[parent.transform.childCount];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = parent.transform.GetChild(i);
                Debug.Log($"   📍 Spawn Point {i}: {points[i].name} at {points[i].position}");
            }
            return points;
        }
        else
        {
            Debug.LogWarning("⚠️ Genel 'SpawnPoints' bulunamadı veya child yok");
        }

        // Tag ile ara
        GameObject[] spawnObjs = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnObjs.Length > 0)
        {
            Debug.Log($"✅ Tag ile spawn point bulundu! Count: {spawnObjs.Length}");
            System.Array.Sort(spawnObjs, (a, b) => a.name.CompareTo(b.name));
            Transform[] points = new Transform[spawnObjs.Length];
            for (int i = 0; i < spawnObjs.Length; i++)
            {
                points[i] = spawnObjs[i].transform;
                Debug.Log($"   📍 Spawn Point {i}: {points[i].name} at {points[i].position}");
            }
            return points;
        }
        else
        {
            Debug.LogWarning("⚠️ Hiçbir 'SpawnPoint' tag'i bulunamadı");
        }

        Debug.LogError($"❌ Hiçbir spawn point bulunamadı: '{parentName}'");
        return null;
    }

    // 🔧 SCENE-SPECİFİC COMPONENT'LER
    private void AddSceneSpecificComponents(GameObject playerObj)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "3.map":
                // Bomba oyunu için BoostReceiver ekle
                if (playerObj.GetComponent<BoostReceiver>() == null)
                {
                    playerObj.AddComponent<BoostReceiver>();
                    Debug.Log($"💣 BoostReceiver eklendi: {playerObj.name}");
                }
                break;
                
            case "1.map":
                // Yarış için özel component'ler (şimdilik yok)
                break;
                
            case "4.map":
                // Köprü için özel component'ler (şimdilik yok)
                break;
                
            case "DeneyK2":
                // Tırmanma için özel component'ler (şimdilik yok)
                break;
        }
    }

    // 📋 PLAYER LIST YÖNETİMİ
    private void AddPlayerToList(GameObject player)
    {
        if (!allPlayers.Contains(player))
        {
            allPlayers.Add(player);
            Debug.Log($"➕ Player listeye eklendi: {player.name} (Toplam: {allPlayers.Count})");
        }
    }

    private void RemovePlayerFromList(ulong clientId)
    {
        for (int i = allPlayers.Count - 1; i >= 0; i--)
        {
            if (allPlayers[i] == null)
            {
                allPlayers.RemoveAt(i);
                continue;
            }

            NetworkObject netObj = allPlayers[i].GetComponent<NetworkObject>();
            if (netObj != null && netObj.OwnerClientId == clientId)
            {
                allPlayers.RemoveAt(i);
                Debug.Log($"➖ Player listeden çıkarıldı: ClientID {clientId}");
                break;
            }
        }
    }

    private void ClearPlayerList()
    {
        allPlayers.Clear();
        Debug.Log("🗑️ Player list temizlendi");
    }

    // 🔧 STATIC ACCESSOR (ESKİ SİSTEM UYUMLULUĞU)
    public static List<GameObject> GetAllPlayers()
    {
        return allPlayers;
    }
}