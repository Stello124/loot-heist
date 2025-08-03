using UnityEngine;
using Unity.Netcode;
using System.Linq;

/// <summary>
/// 1.map'te aşağı düşen oyuncuları başlangıç noktasına ışınlar
/// Invisible trigger zone olarak kullanılır
/// </summary>
public class RespawnTrigger : NetworkBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private bool useSpawnPoints = true;
    [SerializeField] private Transform fallbackSpawnPoint; // Manuel spawn point (opsiyonel)
    
    [Header("Effects (Opsiyonel)")]
    [SerializeField] private GameObject teleportEffectPrefab;
    [SerializeField] private AudioClip teleportSound;

    private void OnTriggerEnter(Collider other)
    {
        // Sadece player'ları kontrol et
        if (!other.CompareTag("Player"))
            return;

        // Player'ın NetworkObject'ini al
        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj == null)
            return;

        // Bu player'ın sahibi olan client'a respawn yap
        ulong clientId = playerNetObj.OwnerClientId;
        
        Debug.Log($"🏃‍♂️ Player düştü! Respawn ediliyor: Client {clientId}");
        
        // Client'a respawn pozisyonunu gönder
        RespawnPlayerClientRpc(clientId);
    }

    [ClientRpc]
    private void RespawnPlayerClientRpc(ulong targetClientId)
    {
        // Sadece hedef client bu kodu çalıştırsın
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        // Kendi player objesini bul
        NetworkObject[] allNetworkObjects = FindObjectsOfType<NetworkObject>();
        GameObject localPlayer = null;

        foreach (var netObj in allNetworkObjects)
        {
            if (netObj.OwnerClientId == targetClientId && netObj.CompareTag("Player"))
            {
                localPlayer = netObj.gameObject;
                break;
            }
        }

        if (localPlayer == null)
        {
            Debug.LogError("❌ Local player bulunamadı!");
            return;
        }

        // Respawn pozisyonunu hesapla
        Vector3 respawnPos = GetRespawnPosition(targetClientId);
        
        // Player'ı teleport et
        TeleportPlayer(localPlayer, respawnPos);
        
        // Effect ve ses (opsiyonel)
        PlayTeleportEffects(respawnPos);

        Debug.Log($"✨ Player respawn edildi: {respawnPos}");
    }

    private Vector3 GetRespawnPosition(ulong clientId)
    {
        if (useSpawnPoints)
        {
            // GlobalPlayerSpawner'ın spawn point sistemini kullan
            return GetSpawnPointPosition(clientId);
        }
        else if (fallbackSpawnPoint != null)
        {
            // Manuel spawn point kullan
            return fallbackSpawnPoint.position;
        }
        else
        {
            // Varsayılan pozisyon
            return new Vector3(0, 2, 0);
        }
    }

    private Vector3 GetSpawnPointPosition(ulong clientId)
    {
        // GlobalPlayerSpawner'ın mantığını kullan
        var sortedClientIds = NetworkManager.Singleton.ConnectedClientsIds.OrderBy(id => id).ToList();
        int playerIndex = sortedClientIds.IndexOf(clientId);
        playerIndex = Mathf.Max(0, playerIndex);

        // 1.map için RaceSpawnPoints ara
        Transform[] spawnPoints = FindRaceSpawnPoints();
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[playerIndex % spawnPoints.Length].position;
        }

        // Spawn point bulunamazsa varsayılan
        return new Vector3(playerIndex * 2f, 2f, 0f);
    }

    private Transform[] FindRaceSpawnPoints()
    {
        // "RaceSpawnPoints" parent'ını ara
        GameObject spawnPointsParent = GameObject.Find("RaceSpawnPoints");
        if (spawnPointsParent == null)
        {
            spawnPointsParent = GameObject.Find("SpawnPoints"); // Alternatif
        }

        if (spawnPointsParent != null)
        {
            Transform[] points = new Transform[spawnPointsParent.transform.childCount];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = spawnPointsParent.transform.GetChild(i);
            }
            return points;
        }

        return null;
    }

    private void TeleportPlayer(GameObject player, Vector3 position)
    {
        // CharacterController varsa disable et (teleport için)
        CharacterController charController = player.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
        }

        // Pozisyonu değiştir
        player.transform.position = position;

        // CharacterController'ı tekrar aktif et
        if (charController != null)
        {
            charController.enabled = true;
        }
    }

    private void PlayTeleportEffects(Vector3 position)
    {
        // Teleport effect spawn et
        if (teleportEffectPrefab != null)
        {
            GameObject effect = Instantiate(teleportEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f); // 2 saniye sonra sil
        }

        // Teleport sesini çal
        if (teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(teleportSound, position);
        }
    }
}