using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class NetworkPlayerSpawnerK : NetworkBehaviour
{
    [Header("Global Spawn Settings")]
    [SerializeField] private List<GameObject> spawnablePrefabs;
    [SerializeField] private bool dontDestroyOnLoad = true;
    
    // Static referans - global sistem için
    public static NetworkPlayerSpawnerK Instance;
    
    // Spawn edilmiş oyuncular - static liste (tüm managerlar için)
    private static List<GameObject> globalPlayerList = new List<GameObject>();
    
    private HashSet<ulong> spawnedClients = new HashSet<ulong>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.LogWarning("⚠️ NetworkPlayerSpawnerK DEPRECATED! GlobalPlayerSpawner kullanın.");
        return; // Devre dışı
        
        Debug.Log("🚀 GLOBAL NetworkPlayerSpawnerK aktif - Server modunda");

        // Event'lere abone ol
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // Scene değişim event'i
        SceneManager.sceneLoaded += OnSceneLoaded;

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
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
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

        // Scene'e göre spawn pozisyonu ve rotasyonu al
        Vector3 spawnPos = GetSpawnPositionForScene(clientId);
        Quaternion spawnRot = GetSpawnRotationForScene(clientId);
        
        GameObject obj = Instantiate(selectedPrefab, spawnPos, spawnRot);
        
        // 🔧 CLONE ANİMATOR DÜZELTMESİ!
        FixAnimatorOnClone(obj);
        
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

        // Global listeye ekle
        AddToGlobalPlayerList(obj);

        // Özelleştirme uygula
        ApplyCustomization(obj, clientId);
        
        // Scene-specific component'ler ekle
        AddSceneSpecificComponents(obj);

        Debug.Log($"✅ GLOBAL Spawn edildi: {selectedPrefab.name} → Scene: {SceneManager.GetActiveScene().name} → Client: {clientId} ({reason})");
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
    
    // 🎮 SCENE-SPECIFIC SPAWN POSİTİON ALMA
    private Vector3 GetSpawnPositionForScene(ulong clientId)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        switch (sceneName)
        {
            case "1.map":
                return GetRaceSpawnPosition(clientId);
            case "3.map":
                return GetBombSpawnPosition(clientId);
            default:
                return GetDefaultSpawnPosition(clientId);
        }
    }
    
    private Quaternion GetSpawnRotationForScene(ulong clientId)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        switch (sceneName)
        {
            case "1.map":
                return GetRaceSpawnRotation(clientId);
            case "3.map":
                return GetBombSpawnRotation(clientId);
            default:
                return Quaternion.identity;
        }
    }
    
    // 🏁 1.MAP - YARIŞ SPAWN POSİTİONLARI
    private Vector3 GetRaceSpawnPosition(ulong clientId)
    {
        Transform[] raceSpawnPoints = FindRaceSpawnPoints();
        if (raceSpawnPoints != null && raceSpawnPoints.Length > 0)
        {
            int index = GetClientSpawnIndex(clientId);
            return raceSpawnPoints[index % raceSpawnPoints.Length].position;
        }
        return GetDefaultSpawnPosition(clientId);
    }
    
    private Quaternion GetRaceSpawnRotation(ulong clientId)
    {
        Transform[] raceSpawnPoints = FindRaceSpawnPoints();
        if (raceSpawnPoints != null && raceSpawnPoints.Length > 0)
        {
            int index = GetClientSpawnIndex(clientId);
            return raceSpawnPoints[index % raceSpawnPoints.Length].rotation;
        }
        return Quaternion.identity;
    }
    
    // 💣 3.MAP - BOMBA SPAWN POSİTİONLARI
    private Vector3 GetBombSpawnPosition(ulong clientId)
    {
        Transform[] bombSpawnPoints = FindBombSpawnPoints();
        if (bombSpawnPoints != null && bombSpawnPoints.Length > 0)
        {
            int index = GetClientSpawnIndex(clientId);
            return bombSpawnPoints[index % bombSpawnPoints.Length].position;
        }
        return GetDefaultSpawnPosition(clientId);
    }
    
    private Quaternion GetBombSpawnRotation(ulong clientId)
    {
        Transform[] bombSpawnPoints = FindBombSpawnPoints();
        if (bombSpawnPoints != null && bombSpawnPoints.Length > 0)
        {
            int index = GetClientSpawnIndex(clientId);
            return bombSpawnPoints[index % bombSpawnPoints.Length].rotation;
        }
        return Quaternion.identity;
    }
    
    // 🔍 SPAWN POINT'LERİ BULMAK
    private Transform[] FindRaceSpawnPoints()
    {
        // SpawnPoints parent objesi ara
        GameObject spawnPointsParent = GameObject.Find("SpawnPoints");
        if (spawnPointsParent != null)
        {
            Transform[] points = new Transform[spawnPointsParent.transform.childCount];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = spawnPointsParent.transform.GetChild(i);
            }
            return points;
        }
        
        // Alternatif: Tag ile ara
        GameObject[] spawnObjs = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnObjs.Length > 0)
        {
            Transform[] points = new Transform[spawnObjs.Length];
            for (int i = 0; i < spawnObjs.Length; i++)
            {
                points[i] = spawnObjs[i].transform;
            }
            return points;
        }
        
        return null;
    }
    
    private Transform[] FindBombSpawnPoints()
    {
        // 3.map için özel spawn point'ler
        return FindRaceSpawnPoints(); // Şimdilik aynı sistemi kullan
    }
    
    // 📊 CLIENT SPAWN INDEX HESAPLA
    private int GetClientSpawnIndex(ulong clientId)
    {
        var sortedClientIds = NetworkManager.Singleton.ConnectedClientsIds.OrderBy(id => id).ToList();
        int index = sortedClientIds.IndexOf(clientId);
        return Mathf.Max(0, index);
    }
    
    // 🔧 DEFAULT SPAWN POSİTİON
    private Vector3 GetDefaultSpawnPosition(ulong clientId)
    {
        int index = GetClientSpawnIndex(clientId);
        return new Vector3(index * 2f, 0f, 0f);
    }
    
    // 🎮 SCENE-SPECİFİC COMPONENT'LER EKLE
    private void AddSceneSpecificComponents(GameObject playerObj)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        switch (sceneName)
        {
            case "3.map":
                // BoostReceiver sadece 3.map için
                if (playerObj.GetComponent<BoostReceiver>() == null)
                {
                    playerObj.AddComponent<BoostReceiver>();
                    Debug.Log($"💣 BoostReceiver eklendi (3.Map için): {playerObj.name}");
                }
                break;
                
            case "1.map":
                // 1.map için özel component'ler buraya
                break;
        }
    }
    
    // 📋 GLOBAL PLAYER LIST YÖNETİMİ
    private void AddToGlobalPlayerList(GameObject player)
    {
        if (!globalPlayerList.Contains(player))
        {
            globalPlayerList.Add(player);
            Debug.Log($"➕ Global player listesine eklendi: {player.name} (Toplam: {globalPlayerList.Count})");
        }
    }
    
    private void RemoveFromGlobalPlayerList(ulong clientId)
    {
        for (int i = globalPlayerList.Count - 1; i >= 0; i--)
        {
            if (globalPlayerList[i] == null)
            {
                globalPlayerList.RemoveAt(i);
                continue;
            }
            
            NetworkObject netObj = globalPlayerList[i].GetComponent<NetworkObject>();
            if (netObj != null && netObj.OwnerClientId == clientId)
            {
                globalPlayerList.RemoveAt(i);
                Debug.Log($"➖ Global player listesinden çıkarıldı: ClientID {clientId}");
                break;
            }
        }
    }
    
    // 🌍 SCENE LOAD EVENT
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsServer) return;
        
        Debug.Log($"🌍 Scene yüklendi: {scene.name} - Player'lar yeniden spawn edilecek");
        
        // Scene değişince mevcut oyuncuları yeniden spawn et
        StartCoroutine(RespawnPlayersAfterSceneLoad());
    }
    
    private IEnumerator RespawnPlayersAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.5f); // Scene tamamen yüklensin
        
        // Mevcut spawn edilmiş client'ları temizle ve yeniden spawn et
        spawnedClients.Clear();
        
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayerForClient(clientId, "Scene Loaded");
        }
    }
    
    // 🔧 STATIC ACCESSOR (ESKİ SİSTEM UYUMLULUĞU)
    public static List<GameObject> GetAllNetworkPlayers()
    {
        return globalPlayerList;
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"❌ Client ayrıldı: {clientId}");
        spawnedClients.Remove(clientId);
        RemoveFromGlobalPlayerList(clientId);
    }
    
    // 🔧 CLONE ANİMATOR DÜZELTMESİ
    private void FixAnimatorOnClone(GameObject clonedObject)
    {
        Animator animator = clonedObject.GetComponent<Animator>();
        if (animator != null)
        {
            // Mevcut controller'ı kaydet
            RuntimeAnimatorController currentController = animator.runtimeAnimatorController;
            
            if (currentController != null)
            {
                // Controller'ı yeniden ata (referansları yenile)
                animator.runtimeAnimatorController = null;
                animator.runtimeAnimatorController = currentController;
                
                Debug.Log($"🔧 CLONE ANİMATOR DÜZELDİ! Controller: {currentController.name}");
                
                // Avatar kontrolü
                if (animator.avatar == null)
                {
                    Debug.LogWarning($"⚠️ Clone'da Avatar eksik: {clonedObject.name}");
                }
                else
                {
                    Debug.Log($"✅ Clone Avatar OK: {animator.avatar.name}");
                }
                
                // Animator state kontrolü  
                if (animator.layerCount > 0)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    Debug.Log($"🎬 Clone Animator State: {stateInfo.fullPathHash} (Length: {stateInfo.length})");
                }
            }
            else
            {
                Debug.LogWarning($"❌ Clone'da AnimatorController NULL: {clonedObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"❌ Clone'da Animator eksik: {clonedObject.name}");
        }
    }
}