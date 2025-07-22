using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestServerRPC : NetworkBehaviour
{
    // Oyuncu spawn olduğunda tetiklenir
    public override void OnNetworkSpawn()
    {
        // Burada otomatik test yapmak istemiyorsan kaldırabilirsin
        // if (!IsServer) { TestServerRpc(0); }
    }

    [ClientRpc]
    void StartGameClientRpc()
    {
        // Tüm client'lar sahneye geçer
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        // Host çağırır → client'lara iletir → sahne değişir
        StartGameClientRpc();
        // Host da aynı ClientRpc ile sahneyi değiştirir (ayrıca yazmaya gerek yok)
    }
}