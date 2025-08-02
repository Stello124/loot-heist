using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneDebugLogger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🎬 Aktif Sahne: " + SceneManager.GetActiveScene().name);

        if (NetworkManager.Singleton != null)
        {
            Debug.Log("🌐 ClientID: " + NetworkManager.Singleton.LocalClientId);
            Debug.Log("🔌 IsServer: " + NetworkManager.Singleton.IsServer);
            Debug.Log("🧍 IsClient: " + NetworkManager.Singleton.IsClient);
            Debug.Log("👑 IsHost: " + NetworkManager.Singleton.IsHost);
            Debug.Log("🛜 Relay bağlı mı: " + NetworkManager.Singleton.IsConnectedClient);
            Debug.Log("🎯 Connected Clients: " + NetworkManager.Singleton.ConnectedClientsIds.Count);

            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Debug.Log("🔗 Bağlı Client: " + clientId);
            }
        }
        else
        {
            Debug.LogError("❌ NetworkManager.Singleton null!");
        }
    }
}