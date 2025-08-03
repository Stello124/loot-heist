using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class BreakablePlatform : NetworkBehaviour
{
    public GameObject platformToDisable;  // Mesh'li görsel platform objesi
    public float warningDuration = 1.5f;  // Yanıp sönme süresi
    public float blinkInterval = 0.1f;    // Yanıp sönme aralığı
    public float reappearDelay = 3f;      // Kırıldıktan sonra tekrar görünme süresi

    private Renderer platformRenderer;
    private Collider platformCollider;
    private NetworkVariable<bool> isBreaking = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isBroken = new NetworkVariable<bool>(false);

    void Start()
    {
        platformRenderer = platformToDisable.GetComponent<Renderer>();
        platformCollider = platformToDisable.GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // Sadece server'da çalışsın
        
        if (!isBreaking.Value && !isBroken.Value && other.CompareTag("Player"))
        {
            Debug.Log($"💥 BreakablePlatform tetiklendi: {gameObject.name} - Player: {other.name}");
            StartBreakSequenceServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartBreakSequenceServerRpc()
    {
        if (isBreaking.Value || isBroken.Value) return; // Zaten işlemdeyse işlem yapma
        
        isBreaking.Value = true;
        StartBreakSequenceClientRpc();
    }

    [ClientRpc]
    private void StartBreakSequenceClientRpc()
    {
        StartCoroutine(BreakSequence());
    }

    IEnumerator BreakSequence()
    {
        Debug.Log($"💥 BreakablePlatform yanıp sönmeye başladı: {gameObject.name}");

        float timer = 0f;
        while (timer < warningDuration)
        {
            platformRenderer.enabled = !platformRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // Platform'u kır
        if (IsServer)
        {
            isBroken.Value = true;
            isBreaking.Value = false;
            BreakPlatformClientRpc();
        }
    }
    
    [ClientRpc]
    private void BreakPlatformClientRpc()
    {
        Debug.Log($"💥 BreakablePlatform kırıldı: {gameObject.name}");
        platformRenderer.enabled = false;
        platformCollider.enabled = false;
        
        // Respawn için coroutine başlat
        StartCoroutine(RespawnPlatform());
    }
    
    private IEnumerator RespawnPlatform()
    {
        yield return new WaitForSeconds(reappearDelay);
        
        if (IsServer)
        {
            RespawnPlatformClientRpc();
        }
    }
    
    [ClientRpc]
    private void RespawnPlatformClientRpc()
    {
        Debug.Log($"🔄 BreakablePlatform yeniden oluştu: {gameObject.name}");
        platformRenderer.enabled = true;
        platformCollider.enabled = true;
        
        if (IsServer)
        {
            isBroken.Value = false;
        }
    }
}
