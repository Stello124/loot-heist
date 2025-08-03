using UnityEngine;
using Unity.Netcode;

public class BombGameManager : NetworkBehaviour
{
    [Header("UI")]
    public BombGameUI bombGameUI;

    void Start()
    {
        // BombGameUI'yi otomatik bul veya oluştur
        if (bombGameUI == null)
        {
            bombGameUI = FindObjectOfType<BombGameUI>();
            
            if (bombGameUI == null)
            {
                // BombGameUI GameObject'i oluştur
                GameObject uiObj = new GameObject("BombGameUI");
                bombGameUI = uiObj.AddComponent<BombGameUI>();
                
                Debug.Log("💻 BombGameUI otomatik oluşturuldu");
            }
        }
    }
}