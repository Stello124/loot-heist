using UnityEngine;
using Unity.Netcode;

public class GameFlowController : NetworkBehaviour
{
    public void StartGame()
    {
        Debug.Log("🎮 Oyun başladı!");
        
        // Sadece server oyunu başlatır
        if (IsServer && BombManager.Instance != null)
        {
            // BombManager artık otomatik başlıyor
            Debug.Log("🎮 GameFlowController: BombManager otomatik başlatıldı");
        }
    }
}

