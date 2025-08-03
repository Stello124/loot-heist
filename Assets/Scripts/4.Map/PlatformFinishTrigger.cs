using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 4.map platform oyunu bitiş sistemi - İlk gelen veya son kalan kazanır!
/// </summary>
public class PlatformFinishTrigger : NetworkBehaviour
{
    [Header("Finish Area Settings")]
    [SerializeField] private GameObject finishAreaEffect; // Bitiş alanı efekti
    [SerializeField] private AudioClip finishSound; // Bitiş sesi
    [SerializeField] private bool isFinishLine = true; // true: finish line, false: elimination area

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🎯 Platform finish trigger: {other.name} (4.map)");
        
        // Sadece server'da çalışsın
        if (!IsServer) 
        {
            Debug.Log("❌ Client'ta finish trigger çalışmaz (4.map)");
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

        Debug.Log($"🟡 Player platform trigger'a geldi: Client {playerNetObj.OwnerClientId} (4.map)");

        // PlatformGameManager'a bildir
        PlatformGameManager platformManager = FindObjectOfType<PlatformGameManager>();
        if (platformManager != null)
        {
            // Sadece oyun aktifse işlem yap
            if (platformManager.IsGameActive())
            {
                if (isFinishLine)
                {
                    Debug.Log("✅ Oyun aktif - Finish işleniyor (4.map)");
                    
                    // Finish effect'lerini tüm client'larda göster
                    ShowFinishEffectsClientRpc(other.transform.position, playerNetObj.OwnerClientId);
                    
                    // Player finished bildir
                    platformManager.PlayerFinished(playerNetObj.OwnerClientId);
                }
                else
                {
                    Debug.Log("💀 Oyun aktif - Elimination işleniyor (4.map)");
                    
                    // Elimination effect'lerini göster
                    ShowEliminationEffectsClientRpc(other.transform.position, playerNetObj.OwnerClientId);
                    
                    // Player eliminated bildir
                    platformManager.PlayerEliminated(playerNetObj.OwnerClientId);
                }
            }
            else
            {
                Debug.Log("⚠️ Oyun henüz aktif değil - Platform trigger göz ardı edildi (4.map)");
            }
        }
        else
        {
            Debug.LogError("❌ PlatformGameManager bulunamadı! (4.map)");
        }
    }

    // Finish efektlerini göster (tüm client'larda)
    [ClientRpc]
    public void ShowFinishEffectsClientRpc(Vector3 playerPosition, ulong playerId)
    {
        Debug.Log($"🎊 ShowFinishEffects çağrıldı: Player {playerId} (4.map)");
        
        if (finishAreaEffect != null)
        {
            GameObject effect = Instantiate(finishAreaEffect, playerPosition, Quaternion.identity);
            Destroy(effect, 5f); // 5 saniye sonra efekti sil
        }

        if (finishSound != null)
        {
            AudioSource.PlayClipAtPoint(finishSound, playerPosition);
        }
    }

    // Elimination efektlerini göster (tüm client'larda)
    [ClientRpc]
    public void ShowEliminationEffectsClientRpc(Vector3 playerPosition, ulong playerId)
    {
        Debug.Log($"💀 ShowEliminationEffects çağrıldı: Player {playerId} (4.map)");
        
        // Farklı efekt göstermek için finishAreaEffect'i farklı renkte kullanabiliriz
        if (finishAreaEffect != null)
        {
            GameObject effect = Instantiate(finishAreaEffect, playerPosition, Quaternion.identity);
            // Elimination için kırmızı renk uygula
            var renderer = effect.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = Color.red;
            
            Destroy(effect, 3f); // 3 saniye sonra efekti sil
        }

        // Elimination için farklı ses çal
        if (finishSound != null)
        {
            AudioSource.PlayClipAtPoint(finishSound, playerPosition, 0.7f); // Daha düşük ses
        }
    }

    // Inspector'da ayarlamak için
    [ContextMenu("🎯 Set as Finish Line")]
    public void SetAsFinishLine()
    {
        isFinishLine = true;
        gameObject.name = "FinishLineTrigger";
        Debug.Log("✅ Bu trigger finish line olarak ayarlandı (4.map)");
    }

    [ContextMenu("💀 Set as Elimination Area")]
    public void SetAsEliminationArea()
    {
        isFinishLine = false;
        gameObject.name = "EliminationTrigger";
        Debug.Log("✅ Bu trigger elimination area olarak ayarlandı (4.map)");
    }
}