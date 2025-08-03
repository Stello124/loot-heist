using UnityEngine;
using Unity.Netcode;

public class PlatformSpawner : NetworkBehaviour
{
    public GameObject normalPlatformPrefab;
    public GameObject instantBreakPlatformPrefab;
    public GameObject timedRespawnPlatformPrefab;

    public int stepCount = 20;
    public float stepSpacing = 2.5f;
    public float laneSpacing = 2f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Sadece server platformları spawn etsin
        
        Debug.Log("PlatformSpawner başladı - Server'da platformlar spawn ediliyor.");
        SpawnPlatforms();
    }

    void SpawnPlatforms()
    {
        if (normalPlatformPrefab == null || instantBreakPlatformPrefab == null || timedRespawnPlatformPrefab == null)
        {
            Debug.LogError("Bir veya daha fazla prefab atanmamış!");
            return;
        }

        Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

        for (int i = 0; i < stepCount; i++)
        {
            float zPos = i * stepSpacing;
            Vector3 basePos = transform.position;

            Vector3[] positions = new Vector3[]
            {
                basePos + new Vector3(-laneSpacing, 0f, zPos),
                basePos + new Vector3(0f, 0f, zPos),
                basePos + new Vector3(laneSpacing, 0f, zPos)
            };

            GameObject[] prefabs = new GameObject[]
            {
                normalPlatformPrefab,
                instantBreakPlatformPrefab,
                timedRespawnPlatformPrefab
            };

            Shuffle(prefabs);

            for (int j = 0; j < 3; j++)
            {
                // NetworkObject olarak spawn et
                GameObject spawnedPlatform = Instantiate(prefabs[j], positions[j], rotation);
                
                // NetworkObject component'i varsa spawn et
                NetworkObject netObj = spawnedPlatform.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                    Debug.Log($"Network Platform spawn edildi: {spawnedPlatform.name} at {positions[j]}");
                }
                else
                {
                    Debug.LogWarning($"Platform'da NetworkObject yok: {spawnedPlatform.name}");
                }
            }

            Debug.Log($"Adım {i + 1}: Platformlar karıştırılarak spawn edildi.");
        }

        Debug.Log("Tüm platformlar spawn edildi.");
    }

    // Basit Shuffle algoritması
    void Shuffle(GameObject[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            GameObject temp = array[i];
            array[i] = array[rnd];
            array[rnd] = temp;
        }
    }
}
