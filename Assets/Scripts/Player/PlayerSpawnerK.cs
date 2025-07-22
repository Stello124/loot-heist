using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject kartalPrefab;

    void Start()
    {
        StartCoroutine(SpawnWhenReady());
    }

    IEnumerator SpawnWhenReady()
    {
        // NetworkManager hazýr olana kadar bekle
        while (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            yield return null;

        if (kartalPrefab == null)
        {
            Debug.LogError("Kartal prefabý atanmadý!");
            yield break;
        }

        Vector3 spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        GameObject playerObj = Instantiate(kartalPrefab, spawnPos, Quaternion.identity);
        playerObj.GetComponent<NetworkObject>().Spawn();
    }
}