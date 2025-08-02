using UnityEngine;
using System.Collections;

public class PlayerVisualSetup : MonoBehaviour
{
    [SerializeField] private Transform visualContainer;

    IEnumerator Start()
    {
        if (visualContainer == null)
        {
            Debug.LogError("VisualContainer atanmadı! Inspector’da bağlanması gerekiyor.");
            yield break;
        }

        yield return new WaitUntil(() =>
            GameState.LocalPlayerData != null &&
            !string.IsNullOrEmpty(GameState.LocalPlayerData.PrefabId)
        );

        LoadVisual();
    }

    public void LoadVisual()
    {
        // Eğer visualContainer’da önceden yüklenmiş karakter varsa sil
        foreach (Transform child in visualContainer)
        {
            Destroy(child.gameObject);
        }

        string prefabId = GameState.LocalPlayerData?.PrefabId ?? "palyaco";

        GameObject visualPrefab = Resources.Load<GameObject>($"Characters/{prefabId}");

        if (visualPrefab == null)
        {
            Debug.LogWarning($"Visual prefab bulunamadı: Resources/Characters/{prefabId}");
            return;
        }

        GameObject instance = Instantiate(visualPrefab, visualContainer);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        Debug.Log($"Visual prefab yüklendi: {prefabId}");
    }
}
