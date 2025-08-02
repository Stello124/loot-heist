using UnityEngine;
using Unity.Netcode;

public class PlayerBombToucher : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Sadece owner için çalışsın
        if (!IsOwner) return;
        
        Debug.Log($"{gameObject.name} triggered with {other.gameObject.name}");

        if (!other.CompareTag("Player")) return;

        GameObject me = this.gameObject;
        GameObject otherPlayer = other.gameObject;

        // Current bomb holder kontrolü
        if (BombManager.Instance.GetCurrentBombHolder() == me)
        {
            NetworkObject otherNetObj = otherPlayer.GetComponent<NetworkObject>();
            if (otherNetObj != null)
            {
                Debug.Log($"{me.name} passed bomb to {otherPlayer.name}");
                // Server'a bomba transferi isteği gönder
                TransferBombServerRpc(otherNetObj.OwnerClientId);
            }
        }
    }
    
    [ServerRpc]
    private void TransferBombServerRpc(ulong targetClientId)
    {
        if (BombManager.Instance != null)
        {
            // BombManager'da private olan SetBombHolderServerRpc'yi çağırabilmek için
            // public bir method ekleyeceğiz
            BombManager.Instance.TransferBombToClient(targetClientId);
        }
    }
}


