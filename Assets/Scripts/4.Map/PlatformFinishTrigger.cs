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
                    platformManager.PlayerFinished(playerNetObj.OwnerClientId);
                }
                else
                {
                    Debug.Log("💀 Oyun aktif - Elimination işleniyor (4.map)");
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

    // Bitiş efektlerini göster (tüm client'larda)
    [ClientRpc]
    public void ShowFinishEffectsClientRpc()
    {
        if (finishAreaEffect != null)
        {
            finishAreaEffect.SetActive(true);
        }

        if (finishSound != null)
        {
            AudioSource.PlayClipAtPoint(finishSound, transform.position);
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