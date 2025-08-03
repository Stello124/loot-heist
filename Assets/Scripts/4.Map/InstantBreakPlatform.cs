using UnityEngine;
using Unity.Netcode;

public class OneTimeBreakPlatform : NetworkBehaviour
{
    public GameObject platformToDestroy; // Kırılacak platform (görsel + collider içeren)
    
    private NetworkVariable<bool> isBroken = new NetworkVariable<bool>(false);

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // Sadece server'da çalışsın
        
        if (other.CompareTag("Player") && !isBroken.Value)
        {
            Debug.Log($"💥 Platform kırılıyor: {gameObject.name} - Player: {other.name}");
            BreakPlatformServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void BreakPlatformServerRpc()
    {
        if (isBroken.Value) return; // Zaten kırılmışsa işlem yapma
        
        isBroken.Value = true;
        BreakPlatformClientRpc();
    }
    
    [ClientRpc]
    private void BreakPlatformClientRpc()
    {
        if (platformToDestroy != null)
        {
            Debug.Log($"💥 Platform kırıldı (Client): {gameObject.name}");
            Destroy(platformToDestroy);
        }
        else
        {
            Debug.LogWarning($"⚠️ platformToDestroy null: {gameObject.name}");
        }
    }
}
