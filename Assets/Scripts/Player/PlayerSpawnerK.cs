using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Vector3 spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        // Karakteri bu client’e ait olarak spawn et
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
