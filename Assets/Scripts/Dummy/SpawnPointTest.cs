using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpawnPointTest : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<Transform> spawnPoints = new List<Transform>();
    public GameObject playerPrefab;
    public float spawnDelay = 1f;
    public bool randomSpawn = true;
    
    [Header("Test Settings")]
    public bool autoSpawnOnStart = false;
    public int maxSpawnCount = 10;
    
    private int currentSpawnIndex = 0;
    private int spawnedCount = 0;
    
    void Start()
    {
        if (autoSpawnOnStart)
        {
            StartSpawning();
        }
        
        // Debug bilgileri
        Debug.Log($"[SpawnPointTest] {spawnPoints.Count} spawn point bulundu");
    }
    
    void Update()
    {
        // Test için tuş kontrolleri
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnPlayer();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetSpawnPoints();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllSpawned();
        }
    }
    
    public void StartSpawning()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("[SpawnPointTest] Hiç spawn point tanımlanmamış!");
            return;
        }
        
        InvokeRepeating(nameof(SpawnPlayer), 0f, spawnDelay);
    }
    
    public void StopSpawning()
    {
        CancelInvoke(nameof(SpawnPlayer));
    }
    
    public void SpawnPlayer()
    {
        if (spawnedCount >= maxSpawnCount)
        {
            Debug.Log("[SpawnPointTest] Maksimum spawn sayısına ulaşıldı");
            StopSpawning();
            return;
        }
        
        if (spawnPoints.Count == 0 || playerPrefab == null)
        {
            Debug.LogWarning("[SpawnPointTest] Spawn point veya prefab eksik!");
            return;
        }
        
        Transform spawnPoint = GetNextSpawnPoint();
        
        if (spawnPoint != null)
        {
            GameObject spawnedPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            spawnedPlayer.name = $"TestPlayer_{spawnedCount}";
            spawnedCount++;
            
            Debug.Log($"[SpawnPointTest] Oyuncu spawn edildi: {spawnedPlayer.name} - Pozisyon: {spawnPoint.position}");
        }
    }
    
    private Transform GetNextSpawnPoint()
    {
        if (randomSpawn)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Count)];
        }
        else
        {
            Transform point = spawnPoints[currentSpawnIndex];
            currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Count;
            return point;
        }
    }
    
    public void ResetSpawnPoints()
    {
        currentSpawnIndex = 0;
        spawnedCount = 0;
        Debug.Log("[SpawnPointTest] Spawn sistemı sıfırlandı");
    }
    
    public void ClearAllSpawned()
    {
        GameObject[] testPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in testPlayers)
        {
            if (player.name.Contains("TestPlayer_"))
            {
                DestroyImmediate(player);
            }
        }
        
        spawnedCount = 0;
        Debug.Log("[SpawnPointTest] Tüm test oyuncuları temizlendi");
    }
    
    // Spawn pointleri görselleştirmek için
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] == null) continue;
            
            // Spawn point'i işaretle
            Gizmos.color = randomSpawn ? Color.yellow : (i == currentSpawnIndex ? Color.green : Color.red);
            Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);
            
            // Yön okunu göster
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(spawnPoints[i].position, spawnPoints[i].forward * 2f);
            
            // Index numarasını göster (Scene view'da)
#if UNITY_EDITOR
            UnityEditor.Handles.Label(spawnPoints[i].position + Vector3.up, $"SP {i}");
#endif
        }
    }
    
    // Inspector'da bilgi göster
    [ContextMenu("Test Spawn")]
    void TestSingleSpawn()
    {
        SpawnPlayer();
    }
    
    [ContextMenu("Test Info")]
    void ShowTestInfo()
    {
        Debug.Log($"[SpawnPointTest] Aktif Spawn Point Sayısı: {spawnPoints.Count}");
        Debug.Log($"[SpawnPointTest] Spawn Edilmiş Oyuncu Sayısı: {spawnedCount}");
        Debug.Log($"[SpawnPointTest] Mevcut Spawn Index: {currentSpawnIndex}");
        Debug.Log($"[SpawnPointTest] Random Spawn: {randomSpawn}");
    }
}