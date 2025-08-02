using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SceneLoadDebugger : MonoBehaviour
{
    void Start()
    {
        // Scene load event'lerini dinle
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        
        Debug.Log("🔍 SceneLoadDebugger aktif - Scene geçişlerini izliyor");
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"✅ Scene yüklendi: {scene.name} | Mode: {mode} | Build Index: {scene.buildIndex}");
        
        // Network Manager var mı?
        if (NetworkManager.Singleton != null)
        {
            Debug.Log($"🌐 Network durumu: IsHost={NetworkManager.Singleton.IsHost}, IsClient={NetworkManager.Singleton.IsClient}");
        }
        
        // Spawn point'ler var mı?
        CheckSpawnPoints();
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"🗑️ Scene unload edildi: {scene.name}");
    }
    
    private void CheckSpawnPoints()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        // Scene'e göre spawn point'leri kontrol et
        switch (sceneName)
        {
            case "4.map":
                CheckSpawnPointsForScene("BridgeSpawnPoints", "4.map Köprü");
                break;
            case "3.map":
                CheckSpawnPointsForScene("BombSpawnPoints", "3.map Bomba");
                break;
            case "1.map":
                CheckSpawnPointsForScene("RaceSpawnPoints", "1.map Yarış");
                break;
            case "DeneyK2":
                CheckSpawnPointsForScene("ClimbSpawnPoints", "DeneyK2 Tırmanma");
                break;
        }
    }
    
    private void CheckSpawnPointsForScene(string spawnPointParentName, string sceneName)
    {
        // Belirli parent ara
        GameObject parent = GameObject.Find(spawnPointParentName);
        if (parent != null)
        {
            Debug.Log($"✅ {sceneName} spawn points bulundu: {spawnPointParentName} ({parent.transform.childCount} adet)");
            return;
        }
        
        // Genel SpawnPoints ara
        parent = GameObject.Find("SpawnPoints");
        if (parent != null)
        {
            Debug.Log($"✅ {sceneName} genel spawn points bulundu: SpawnPoints ({parent.transform.childCount} adet)");
            return;
        }
        
        // Tag ile ara
        GameObject[] spawnObjs = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnObjs.Length > 0)
        {
            Debug.Log($"✅ {sceneName} tag ile spawn points bulundu: {spawnObjs.Length} adet");
            return;
        }
        
        Debug.LogWarning($"⚠️ {sceneName} spawn points bulunamadı!");
    }
}