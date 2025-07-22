using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NetworkPlayerSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnablePrefabs;

    void Start()
    {
        Debug.Log("Spawner çalýþtý. PrefabId: " + GameState.LocalPlayerData?.PrefabId);


        //if (!NetworkManager.Singleton.IsServer) return;

        string prefabId = GameState.LocalPlayerData?.PrefabId;
        GameObject selectedPrefab = spawnablePrefabs.Find(p => p.name == prefabId);

        if (selectedPrefab == null)
        {
            Debug.LogError("Prefab bulunamadý: " + prefabId);
            return;
        }

        Vector3 spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        GameObject obj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn();
    }
}