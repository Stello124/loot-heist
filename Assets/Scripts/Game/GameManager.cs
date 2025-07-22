using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject); // sahneler arasý yok olmasýn
    }

    void Start()
    {
        // Test için otomatik veri tanýmý — prefab adý "Kartal"
        SetLocalPlayerData(prefabId: "Kartal");
    }

    public void SetLocalPlayerData(string prefabId, string selectedSkin = "Basic", string playerName = "Player")
    {
        GameState.LocalPlayerData = new PlayerData
        {
            PlayerName = playerName,
            PrefabId = prefabId,
            SelectedSkin = selectedSkin
        };
    }
}