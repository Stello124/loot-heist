using UnityEngine;
using Unity.Netcode;
using System.Collections;

/// <summary>
/// 4.map kırılabilir platform sistemi - Network sync ile tüm client'larda çalışır
/// </summary>
public class OneTimeBreakPlatform : NetworkBehaviour
{
    [Header("Break Platform Settings")]
    public GameObject platformToDestroy; // Kırılacak platform (görsel + collider içeren)
    public float breakDelay = 0.1f; // Kırılma gecikmesi
    public GameObject breakEffect; // Kırılma efekti (opsiyonel)
    public AudioClip breakSound; // Kırılma sesi (opsiyonel)
    
    private bool isAlreadyBroken = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🎯 Platform trigger: {other.name} (4.map)");
        
        // Zaten kırıldıysa işlem yapma
        if (isAlreadyBroken) 
        {
            Debug.Log("⚠️ Platform zaten kırılmış (4.map)");
            return;
        }
        
        // Sadece server'da çalışsın
        if (!IsServer) 
        {
            Debug.Log("❌ Client'ta platform break çalışmaz (4.map)");
            return;
        }

        // Sadece player'ları kontrol et
        if (!other.CompareTag("Player")) 
        {
            Debug.Log($"❌ Tag player değil: {other.tag} (4.map)");
            return;
        }

        Debug.Log($"🟡 Platform kırılacak: {gameObject.name} (4.map)");
        
        isAlreadyBroken = true;
        StartCoroutine(BreakPlatformWithDelay());
    }

    private IEnumerator BreakPlatformWithDelay()
    {
        // Küçük gecikme
        yield return new WaitForSeconds(breakDelay);
        
        // Efekt pozisyonunu al
        Vector3 effectPosition = platformToDestroy != null ? platformToDestroy.transform.position : transform.position;
        
        // Tüm client'larda platform'u kır
        BreakPlatformClientRpc(effectPosition);
        
        Debug.Log($"✅ Platform kırıldı: {gameObject.name} (4.map)");
    }

    [ClientRpc]
    private void BreakPlatformClientRpc(Vector3 effectPosition)
    {
        Debug.Log($"🔨 BreakPlatformClientRpc çağrıldı: {gameObject.name} (4.map)");
        
        // Platform'u kır
        if (platformToDestroy != null)
        {
            // Renderer ve Collider'ı kapat (görsel olarak kırılmış gözüksün)
            var renderer = platformToDestroy.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
            
            var collider = platformToDestroy.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            
            Debug.Log($"🔨 Platform görsel olarak kırıldı: {platformToDestroy.name} (4.map)");
        }
        
        // Efekt göster
        if (breakEffect != null)
        {
            GameObject effect = Instantiate(breakEffect, effectPosition, Quaternion.identity);
            Destroy(effect, 3f); // 3 saniye sonra efekti sil
        }
        
        // Ses çal
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, effectPosition);
        }
    }

    // Inspector debug için
    [ContextMenu("🔨 Test Break Platform")]
    public void TestBreakPlatform()
    {
        if (IsServer)
        {
            StartCoroutine(BreakPlatformWithDelay());
        }
        else
        {
            Debug.Log("⚠️ Test sadece Server'da çalışır (4.map)");
        }
    }
}
