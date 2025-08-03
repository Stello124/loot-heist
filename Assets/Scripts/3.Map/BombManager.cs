using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class BombManager : NetworkBehaviour
{
    public static BombManager Instance;

    public GameObject bombPrefab;
    public float bombTimer = 555f;

    private NetworkVariable<ulong> currentBombHolderClientId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private GameObject bombVisual;
    private Coroutine countdownCoroutine;
    private bool gameStarted = false;

    void Awake()
    {
        Instance = this;
        
        // Scene değişiminde bomba temizle
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("💣 BombManager Server aktif!");
        
        // Oyunu otomatik başlat
        StartCoroutine(AutoStartGameDelayed());
    }

    private IEnumerator AutoStartGameDelayed()
    {
        Debug.Log("⏰ Oyuncuların spawn olmasını bekliyorum...");
        
        // 2 saniye bekle ki oyuncular spawn olsun
        yield return new WaitForSeconds(2f);
        
        // En az 2 oyuncu varsa oyunu başlat
        var players = GlobalPlayerSpawner.GetAllPlayers();
        if (players.Count >= 2)
        {
            Debug.Log($"🎮 {players.Count} oyuncu ile bomba oyunu başlatılıyor!");
            StartBombGame();
        }
        else
        {
            Debug.Log("⚠️ Oyun başlatılamadı: En az 2 oyuncu gerekli");
        }
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        // 3.map dışında bomba görselini temizle
        if (scene.name != "3.map" && bombVisual != null)
        {
            Debug.Log($"💣 Scene değişti ({scene.name}) - Bomba görseli temizlendi");
            Destroy(bombVisual);
            bombVisual = null;
        }
    }

    public void StartBombGame()
    {
        if (!IsServer) return;
        
        gameStarted = true;
        AssignBombToRandomPlayer();
    }
    
    public void OnPlayerCountChanged()
    {
        if (!IsServer || !gameStarted) return;
        
        CheckGameEnd();
    }

    public void AssignBombToRandomPlayer()
    {
        if (!IsServer) return;
        
        var players = GlobalPlayerSpawner.GetAllPlayers();
        if (players.Count == 0)
        {
            Debug.LogWarning("💣 Bomba atanacak oyuncu yok!");
            return;
        }
        
        int randomIndex = Random.Range(0, players.Count);
        GameObject selectedPlayer = players[randomIndex];
        
        NetworkObject netObj = selectedPlayer.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            SetBombHolderServerRpc(netObj.OwnerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBombHolderServerRpc(ulong clientId)
    {
        currentBombHolderClientId.Value = clientId;
        UpdateBombVisualClientRpc(clientId);
        
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(BombCountdown());
        
        Debug.Log($"💣 Bomba client {clientId}'ye atandı");
    }
    
    [ClientRpc]
    private void UpdateBombVisualClientRpc(ulong clientId)
    {
        // 🚨 SADECE 3.MAP'TE BOMBA GÖSTERSİN!
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != "3.map")
        {
            Debug.Log($"💣 Bomba görseli iptal edildi - Sahne: {currentScene} (3.map değil)");
            return;
        }
        
        // Önceki bomba görselini temizle
        if (bombVisual != null)
            Destroy(bombVisual);
            
        // Yeni bomba sahibini bul
        GameObject newHolder = GetPlayerByClientId(clientId);
        if (newHolder == null)
        {
            Debug.LogError($"💣 Client {clientId} için oyuncu bulunamadı!");
            return;
        }

        // Karakterin içindeki "RightHand" objesini bul
        Transform hand = newHolder.GetComponentsInChildren<Transform>()
            .FirstOrDefault(t => t.name == "RightHand");

        if (hand == null)
        {
            Debug.LogError("💣 RightHand bulunamadı! Prefabda doğru isimli nesne olduğundan emin ol.");
            return;
        }

        // Bombayı spawn et ve ele yapıştır
        bombVisual = Instantiate(bombPrefab, hand.position, hand.rotation, hand);
        bombVisual.transform.localPosition = Vector3.zero;
        
        Debug.Log($"💣 Bomba görseli {newHolder.name} için oluşturuldu (3.map'te)");
    }

    IEnumerator BombCountdown()
    {
        if (!IsServer) yield break;
        
        float time = bombTimer;
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }

        // Bomba patladı - Server'da işle
        ulong explodedClientId = currentBombHolderClientId.Value;
        GameObject explodedPlayer = GetPlayerByClientId(explodedClientId);
        
        if (explodedPlayer != null)
        {
            Debug.Log($"💥 {explodedPlayer.name} patladı! (Client: {explodedClientId})");
            
            // Oyuncuyu yok et
            NetworkObject netObj = explodedPlayer.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Despawn();
            }
        }
        
        // Bomba görselini temizle
        DestroyBombVisualClientRpc();
        currentBombHolderClientId.Value = 0; // Reset

        yield return new WaitForSeconds(1f);

        CheckGameEnd();
    }
    
    [ClientRpc]
    private void DestroyBombVisualClientRpc()
    {
        if (bombVisual != null)
        {
            Destroy(bombVisual);
            bombVisual = null;
        }
    }
    
    private void CheckGameEnd()
    {
        if (!IsServer) return;
        
        var remainingPlayers = GlobalPlayerSpawner.GetAllPlayers();
        
        if (remainingPlayers.Count == 1)
        {
            GameObject winner = remainingPlayers[0];
            NetworkObject winnerNetObj = winner.GetComponent<NetworkObject>();
            
            if (winnerNetObj != null)
            {
                Debug.Log($"🏆 {winner.name} kazandı! (Client: {winnerNetObj.OwnerClientId})");
                
                // Kazanan kişiden ismini iste
                RequestWinnerNameClientRpc(winnerNetObj.OwnerClientId);
            }
        }
        else if (remainingPlayers.Count > 1)
        {
            AssignBombToRandomPlayer();
        }
        else
        {
            Debug.Log("🤷 Hiç oyuncu kalmadı!");
        }
    }
    
    [ClientRpc]
    private void RequestWinnerNameClientRpc(ulong winnerClientId)
    {
        // Sadece kazanan kişi cevap versin
        if (NetworkManager.Singleton.LocalClientId != winnerClientId) return;

        string myName = "Host"; // Default
        
        // StartupManager'dan gerçek ismi al
        if (!string.IsNullOrEmpty(StartupManager.PlayerName))
        {
            myName = StartupManager.PlayerName;
        }
        else if (winnerClientId == 0)
        {
            myName = "Host";
        }
        else
        {
            myName = $"Player {winnerClientId}";
        }

        Debug.Log($"🏆 Bomba oyunu kazanan olarak ismimi gönderiyorum: {myName}");
        
        // İsmimi server'a gönder
        SendWinnerNameServerRpc(myName, winnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendWinnerNameServerRpc(string winnerName, ulong winnerClientId)
    {
        Debug.Log($"🏆 Server aldı bomba kazanan ismi: {winnerName} (Client {winnerClientId})");
        
        // Tüm client'lara kazananın gerçek ismini gönder
        ShowWinnerWithNameClientRpc(winnerName, winnerClientId);
    }

    [ClientRpc]
    private void ShowWinnerWithNameClientRpc(string winnerName, ulong winnerClientId)
    {
        Debug.Log($"🏆 Bomba kazananı ismiyle gösteriliyor: {winnerName} (Client {winnerClientId})");
        
        BombGameUI bombUI = FindObjectOfType<BombGameUI>();
        if (bombUI != null)
        {
            bombUI.ShowWinner(winnerName, winnerClientId);
        }
        else
        {
            Debug.LogError("❌ BombGameUI bulunamadı!");
        }

        Debug.Log($"🏆 Bomba oyunu bitti! Kazanan: {winnerName} (Client {winnerClientId})");
    }

    // Erişim için public getter
    public GameObject GetCurrentBombHolder()
    {
        return GetPlayerByClientId(currentBombHolderClientId.Value);
    }
    
    private GameObject GetPlayerByClientId(ulong clientId)
    {
        if (clientId == 0) return null;
        
        var players = GlobalPlayerSpawner.GetAllPlayers();
        foreach (var player in players)
        {
            NetworkObject netObj = player.GetComponent<NetworkObject>();
            if (netObj != null && netObj.OwnerClientId == clientId)
            {
                return player;
            }
        }
        
        return null;
    }
    
    // Public method for PlayerBombToucher and PlayerAttack to transfer bomb
    public void TransferBombToClient(ulong targetClientId)
    {
        if (!IsServer) return;
        SetBombHolderServerRpc(targetClientId);
    }
}







