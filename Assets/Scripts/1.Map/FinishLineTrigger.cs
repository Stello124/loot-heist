using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 1.map yarış bitiş çizgisi - İlk gelen kazanır!
/// </summary>
public class FinishLineTrigger : NetworkBehaviour
{
    [Header("Finish Line Settings")]
    [SerializeField] private GameObject finishLineEffect; // Bitiş çizgisi efekti
    [SerializeField] private AudioClip finishSound; // Bitiş sesi

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🎯 Finish trigger: {other.name}");
        
        // Sadece server'da çalışsın
        if (!IsServer) 
        {
            Debug.Log("❌ Client'ta finish trigger çalışmaz");
            return;
        }

        // Sadece player'ları kontrol et
        if (!other.CompareTag("Player")) 
        {
            Debug.Log($"❌ Tag player değil: {other.tag}");
            return;
        }

        // Player'ın NetworkObject'ini al
        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj == null) 
        {
            Debug.Log("❌ NetworkObject bulunamadı");
            return;
        }

        Debug.Log($"🏁 Player finish line'a geldi: Client {playerNetObj.OwnerClientId}");

        // RaceGameManager'a kazananı bildir
        RaceGameManager raceManager = FindObjectOfType<RaceGameManager>();
        if (raceManager != null)
        {
            // Sadece oyun aktifse kazananı kabul et
            if (raceManager.IsGameActive())
            {
                Debug.Log("✅ Oyun aktif - Kazanan işleniyor");
                raceManager.PlayerFinished(playerNetObj.OwnerClientId);
            }
            else
            {
                Debug.Log("⚠️ Oyun henüz aktif değil - Bitiş çizgisi göz ardı edildi");
            }
        }
        else
        {
            Debug.LogError("❌ RaceGameManager bulunamadı!");
        }
    }

    // Bitiş efektlerini göster (tüm client'larda)
    [ClientRpc]
    public void ShowFinishEffectsClientRpc()
    {
        if (finishLineEffect != null)
        {
            finishLineEffect.SetActive(true);
        }

        if (finishSound != null)
        {
            AudioSource.PlayClipAtPoint(finishSound, transform.position);
        }
    }
}