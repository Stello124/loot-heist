using UnityEngine;
using Unity.Netcode;
using System.Collections;

/// <summary>
/// 4.map oyuncuların düştüğünde respawn olması için trigger sistemi
/// Düşme bölgesi - oyuncu buraya düşerse respawn olur veya eliminate olur
/// </summary>
public class PlatformRespawnTrigger : NetworkBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private Transform[] respawnPoints; // Respawn noktaları
    [SerializeField] private bool eliminateOnFall = false; // false = Respawn olur, true = Eliminate olur
    [SerializeField] private float respawnDelay = 1f; // Respawn gecikmesi (race için hızlı)
    [SerializeField] private AudioClip fallSound; // Düşme sesi

    [Header("Effects")]
    [SerializeField] private GameObject fallEffect; // Düşme efekti

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"💀 Fall trigger: {other.name} (4.map)");
        
        // Sadece server'da çalışsın
        if (!IsServer) 
        {
            Debug.Log("❌ Client'ta fall trigger çalışmaz (4.map)");
            return;
        }

        // Sadece player'ları kontrol et
        if (!other.CompareTag("Player")) 
        {
            Debug.Log($"❌ Tag player değil: {other.tag} (4.map)");
            return;
        }

        // Player'ın NetworkObject'ini al
        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj == null) 
        {
            Debug.Log("❌ NetworkObject bulunamadı (4.map)");
            return;
        }

        ulong playerId = playerNetObj.OwnerClientId;
        Debug.Log($"🟡 Player düştü - başa dönüyor: Client {playerId} (4.map)");

        // PlatformGameManager kontrolü
        PlatformGameManager platformManager = FindObjectOfType<PlatformGameManager>();
        if (platformManager != null && platformManager.IsGameActive())
        {
            if (eliminateOnFall)
            {
                Debug.Log($"💀 Player eliminate ediliyor: {playerId} (4.map)");
                
                // Efektleri göster
                ShowFallEffectsClientRpc(other.transform.position);
                
                // Eliminate et
                platformManager.PlayerEliminated(playerId);
            }
            else
            {
                Debug.Log($"🔄 Player respawn ediliyor: {playerId} (4.map)");
                
                // Efektleri göster
                ShowFallEffectsClientRpc(other.transform.position);
                
                // Respawn et
                StartCoroutine(RespawnPlayer(other.gameObject, playerId));
            }
        }
        else
        {
            Debug.Log("⚠️ Oyun aktif değil - Fall trigger göz ardı edildi (4.map)");
        }
    }

    private IEnumerator RespawnPlayer(GameObject player, ulong playerId)
    {
        Debug.Log($"🔄 Respawn başlatılıyor: {playerId} (4.map)");
        
        // Respawn noktası seç
        Transform respawnPoint = GetRandomRespawnPoint();
        if (respawnPoint == null)
        {
            Debug.LogWarning("⚠️ Respawn point bulunamadı! (4.map)");
            yield break;
        }

        // Hemen teleport et (gecikme olmadan)
        TeleportPlayerClientRpc(playerId, respawnPoint.position);
        Debug.Log($"🔄 Player hemen respawn edildi: {playerId} at {respawnPoint.position} (4.map)");
        
        // Küçük bekleme sonrası pozisyonu tekrar kontrol et
        yield return new WaitForSeconds(0.2f);
        ConfirmTeleportClientRpc(playerId, respawnPoint.position);
    }

    private Transform GetRandomRespawnPoint()
    {
        if (respawnPoints == null || respawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ Respawn points tanımlanmamış! (4.map)");
            return null;
        }
        
        int randomIndex = Random.Range(0, respawnPoints.Length);
        return respawnPoints[randomIndex];
    }

    [ClientRpc]
    private void ShowFallEffectsClientRpc(Vector3 fallPosition)
    {
        // Fall effect göster
        if (fallEffect != null)
        {
            GameObject effect = Instantiate(fallEffect, fallPosition, Quaternion.identity);
            Destroy(effect, 3f); // 3 saniye sonra sil
        }

        // Fall sound çal
        if (fallSound != null)
        {
            AudioSource.PlayClipAtPoint(fallSound, fallPosition);
        }
    }

    [ClientRpc]
    private void ConfirmTeleportClientRpc(ulong targetPlayerId, Vector3 targetPosition)
    {
        // Sadece ilgili client bu mesajı işlesin
        if (NetworkManager.Singleton.LocalClientId != targetPlayerId) return;

        var allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.CompareTag("Player"))
            {
                var netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null && netObj.OwnerClientId == targetPlayerId)
                {
                    // Pozisyonu tekrar kontrol et ve düzelt
                    obj.transform.position = targetPosition;
                    
                    // CharacterController varsa resetle
                    var charController = obj.GetComponent<CharacterController>();
                    if (charController != null)
                    {
                        charController.enabled = false;
                        charController.enabled = true;
                    }
                    
                    Debug.Log($"🔄 Teleport onaylandı: {targetPosition} (4.map)");
                    break;
                }
            }
        }
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong targetPlayerId, Vector3 newPosition)
    {
        // Sadece ilgili client bu mesajı işlesin
        if (NetworkManager.Singleton.LocalClientId != targetPlayerId) return;

        Debug.Log($"🔄 TeleportPlayerClientRpc çağrıldı: {targetPlayerId} → {newPosition} (4.map)");

        var allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.CompareTag("Player"))
            {
                var netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null && netObj.OwnerClientId == targetPlayerId)
                {
                    Debug.Log($"🔄 Player bulundu, teleport ediliyor: {obj.name} (4.map)");
                    
                    // CharacterController varsa geçici olarak kapat
                    var charController = obj.GetComponent<CharacterController>();
                    if (charController != null)
                    {
                        charController.enabled = false;
                    }
                    
                    // Pozisyonu değiştir
                    obj.transform.position = newPosition;
                    
                    // CharacterController'ı tekrar aç
                    if (charController != null)
                    {
                        charController.enabled = true;
                    }
                    
                    Debug.Log($"🔄 Player teleported to: {newPosition} (4.map)");
                    break;
                }
            }
        }
    }

    // Inspector debug için
    [ContextMenu("🎯 Set Respawn Mode")]
    public void SetRespawnMode()
    {
        eliminateOnFall = false;
        gameObject.name = "RespawnTrigger";
        Debug.Log("✅ Bu trigger respawn mode'a ayarlandı (4.map)");
    }

    [ContextMenu("💀 Set Elimination Mode")]
    public void SetEliminationMode()
    {
        eliminateOnFall = true;
        gameObject.name = "EliminationTrigger";
        Debug.Log("✅ Bu trigger elimination mode'a ayarlandı (4.map)");
    }

    // Respawn points'leri göster
    void OnDrawGizmosSelected()
    {
        if (respawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in respawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 1f);
                    Gizmos.DrawLine(point.position, point.position + Vector3.up * 2f);
                }
            }
        }
    }
}