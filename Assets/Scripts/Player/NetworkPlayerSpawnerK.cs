using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NetworkPlayerSpawnerK : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnablePrefabs;

    void Start()
    {
        Debug.Log("Spawner çalıştı. IsServer: " + NetworkManager.Singleton.IsServer);

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Client olduğum için spawn işlemi yapmıyorum.");
            return;
        }

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            string prefabId = GameState.LocalPlayerData?.PrefabId ?? "Kartal";

            // Önce listeden ara
            GameObject selectedPrefab = spawnablePrefabs.Find(p => p.name == prefabId);

            // Listede yoksa Resources klasöründen yükle
            if (selectedPrefab == null)
            {
                selectedPrefab = Resources.Load<GameObject>($"Characters/{prefabId}");
                if (selectedPrefab == null)
                {
                    Debug.LogError("Prefab bulunamadı: " + prefabId);
                    continue;
                }
                else
                {
                    Debug.Log("Resources'tan prefab yüklendi: " + prefabId);
                }
            }

            Vector3 spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
            GameObject obj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError("NetworkObject eksik: " + selectedPrefab.name);
                continue;
            }

            netObj.SpawnWithOwnership(clientId);

            // Burada kayıtlı özelleştirmeyi uygula
            var builder = obj.GetComponent<CharacterBuilder>();
            if (builder != null && GameState.LocalPlayerData != null)
            {
                builder.ApplyCustomization(GameState.LocalPlayerData);
                Debug.Log("🎨 Spawn edilen karaktere kayıtlı özelleştirme uygulandı.");
            }
            else
            {
                Debug.LogWarning("⚠️ CharacterBuilder bulunamadı veya LocalPlayerData null.");
            }

            Debug.Log("Spawn edildi: " + selectedPrefab.name + " → ClientId: " + clientId);
        }
    }
}
