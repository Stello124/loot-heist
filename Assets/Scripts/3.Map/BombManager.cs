using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class BombManager : NetworkBehaviour
{
    public static BombManager Instance;

    public GameObject bombPrefab;
    public float bombTimer = 15f; // Normal bomba süresi

    private NetworkVariable<ulong> currentBombHolderClientId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private GameObject bombVisual;
    private Coroutine countdownCoroutine;
    private bool gameStarted = false;

    // Simple Waiting System
    private NetworkVariable<int> gamePhase = new NetworkVariable<int>(0); // 0=waiting, 1=active
    private NetworkVariable<float> phaseTimer = new NetworkVariable<float>(40f);
    public float waitingTime = 40f; // Uzun bekleme: 40s (panel süresinden daha uzun)

    void Awake()
    {
        Instance = this;
        
        // Scene değişiminde bomba temizle
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("💣 BombManager Server aktif!");
            
            // Oyunu otomatik başlat
            StartCoroutine(AutoStartGameDelayed());
        }
        else
        {
            Debug.Log("💣 BombManager Client aktif!");
        }
        
        // Her client'ta UI oluştur
        StartCoroutine(CreateUIAfterDelay());
    }

    private IEnumerator CreateUIAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        // BombGameUI oluştur (her client'ta)
        BombGameUI bombUI = FindObjectOfType<BombGameUI>();
        if (bombUI == null)
        {
            Debug.Log("💻 Client'ta BombGameUI oluşturuluyor...");
            GameObject uiObj = new GameObject("BombGameUI");
            bombUI = uiObj.AddComponent<BombGameUI>();
            Debug.Log("✅ Client'ta BombGameUI oluştu");
        }
        else
        {
            Debug.Log("💻 BombGameUI zaten mevcut");
        }
        
        // UI artık baştan gizli oluşturuluyor, ekstra ResetUI gerekmiyor
        yield return new WaitForSeconds(0.1f);
        Debug.Log("🔒 BombGameUI otomatik olarak gizli oluşturuldu");
    }

    private IEnumerator AutoStartGameDelayed()
    {
        Debug.Log("⏰ Oyuncuların spawn olmasını bekliyorum...");
        
        // 1 saniye bekle ki oyuncular spawn olsun
        yield return new WaitForSeconds(1f);
        
        // Waiting fazını başlat
        StartWaitingPhase();
    }

    private void StartWaitingPhase()
    {
        if (!IsServer) return;

        gamePhase.Value = 0; // Waiting phase
        phaseTimer.Value = waitingTime;

        Debug.Log($"⏰ Basit bekleme fazı başladı - {waitingTime} saniye, oyuncular serbest, bomba yok");
        
        // Timer başlat
        StartCoroutine(SimpleWaitingTimer());
    }

    private System.Collections.IEnumerator SimpleWaitingTimer()
    {
        Debug.Log($"⏰ {waitingTime} saniye bekleniyor... (Manual Canvas'lar silindi, temiz ekran, bomba YOK)");
        
        while (phaseTimer.Value > 0 && gamePhase.Value == 0)
        {
            yield return new WaitForSeconds(1f);
            phaseTimer.Value = Mathf.Max(0, phaseTimer.Value - 1f);
            
            if (phaseTimer.Value % 5 == 0) // Her 5 saniyede log
            {
                Debug.Log($"⏰ Kalan bekleme süresi: {phaseTimer.Value} saniye (Bomba {waitingTime} saniyede gelecek)");
            }
        }
        
        // 40 saniye bitti, bomba oyununu başlat
        StartBombGame();
    }

    private void StartBombGame()
    {
        if (!IsServer) return;

        gamePhase.Value = 1; // Active phase (basit: 0=waiting, 1=active)
        gameStarted = true;

        Debug.Log($"💣 {waitingTime} saniye bitti! Bomba oyunu başladı! (Panel bitiminden {waitingTime-30} saniye sonra, Bomba süresi: {bombTimer}s)");
        
        // Bomba ata
        AssignBombToRandomPlayer();
    }

    // Freeze sistemi 3.map için kullanılmıyor - oyuncular hep serbest

    // UI metotları kaldırıldı - Sadece kazanan ekranı kullanılacak
    
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

    // Bu metot artık private StartBombGame() ile değiştirildi
    
    public void OnPlayerCountChanged()
    {
        if (!IsServer || !gameStarted) return;
        
        // Ek güvenlik: Sadece ACTIVE fazda kontrol et
        if (gamePhase.Value != 1)
        {
            Debug.Log($"❌ OnPlayerCountChanged: Oyun henüz aktif değil (faz: {gamePhase.Value})");
            return;
        }
        
        Debug.Log("🔍 OnPlayerCountChanged: Oyun aktif, CheckGameEnd çağrılıyor");
        CheckGameEnd();
    }

    public void AssignBombToRandomPlayer()
    {
        if (!IsServer) return;
        
        // SADECE ACTIVE PHASE'DE BOMBA ATA!
        if (gamePhase.Value != 1)
        {
            Debug.LogWarning($"💣 Bomba atama iptal - Yanlış phase: {gamePhase.Value} (1 olmalı, gameStarted: {gameStarted})");
            return;
        }
        
        // Sadece aktif player'ları al
        var activePlayers = FindObjectsOfType<NetworkObject>()
            .Where(obj => obj.CompareTag("Player") && obj.IsSpawned)
            .ToList();
            
        Debug.Log($"💣 Bomba atanacak aktif oyuncu sayısı: {activePlayers.Count}");
        
        if (activePlayers.Count == 0)
        {
            Debug.LogWarning("💣 Bomba atanacak aktif oyuncu yok!");
            return;
        }
        
        int randomIndex = Random.Range(0, activePlayers.Count);
        NetworkObject selectedPlayer = activePlayers[randomIndex];
        
        Debug.Log($"💣 Bomba {selectedPlayer.name} (Client {selectedPlayer.OwnerClientId})'e atanıyor - ACTIVE PHASE");
        SetBombHolderServerRpc(selectedPlayer.OwnerClientId);
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
        
        // Aktif player'ları say (Despawn olan'ları sayma)
        var allPlayers = FindObjectsOfType<NetworkObject>()
            .Where(obj => obj.CompareTag("Player") && obj.IsSpawned)
            .ToList();
        
        Debug.Log($"🔍 Aktif oyuncu sayısı: {allPlayers.Count}");
        
        if (allPlayers.Count == 1)
        {
            // KAZANAN!
            NetworkObject winnerNetObj = allPlayers[0];
            Debug.Log($"🏆 KAZANAN: {winnerNetObj.name} (Client: {winnerNetObj.OwnerClientId})");
            
            // Oyunu durdur
            gameStarted = false;
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
            
            // Kazanan kişiden ismini iste
            RequestWinnerNameClientRpc(winnerNetObj.OwnerClientId);
        }
        else if (allPlayers.Count > 1)
        {
            Debug.Log($"🔄 {allPlayers.Count} oyuncu kaldı, yeni bomba atanıyor...");
            // Sadece ACTIVE fazda bomba ata
            if (gamePhase.Value == 1)
            {
                AssignBombToRandomPlayer();
            }
            else
            {
                Debug.Log("❌ Oyun henüz aktif değil, bomba atanmıyor");
            }
        }
        else
        {
            Debug.Log("🤷 Hiç oyuncu kalmadı!");
            gameStarted = false;
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
        
        StartCoroutine(ShowWinnerUICoroutine(winnerName, winnerClientId));
    }

    private IEnumerator ShowWinnerUICoroutine(string winnerName, ulong winnerClientId)
    {
        Debug.Log($"🎯 ShowWinnerUICoroutine başladı: {winnerName}");
        
        // BombGameUI'yi bul (her client'ta oluşturulmuş olmalı)
        BombGameUI bombUI = FindObjectOfType<BombGameUI>();
        
        if (bombUI == null)
        {
            Debug.LogError("❌ BombGameUI bulunamadı! Client'ta UI oluşturulmamış.");
            yield break;
        }
        
        Debug.Log($"✅ BombGameUI bulundu, winner gösteriliyor: {winnerName}");
        bombUI.ShowWinner(winnerName, winnerClientId);
        
        Debug.Log($"🏆 UI gösterildi: {winnerName} (Client {winnerClientId})");
    }

    // Erişim için public getter
    public GameObject GetCurrentBombHolder()
    {
        return GetPlayerByClientId(currentBombHolderClientId.Value);
    }
    
    private GameObject GetPlayerByClientId(ulong clientId)
    {
        Debug.Log($"🔍 GetPlayerByClientId aranıyor: Client {clientId}");
        
        // Aktif NetworkObject'leri direkt kontrol et
        var allNetworkObjects = FindObjectsOfType<NetworkObject>();
        
        foreach (var netObj in allNetworkObjects)
        {
            if (netObj.CompareTag("Player") && netObj.IsSpawned && netObj.OwnerClientId == clientId)
            {
                Debug.Log($"✅ Client {clientId} için oyuncu bulundu: {netObj.name}");
                return netObj.gameObject;
            }
        }
        
        Debug.LogWarning($"❌ Client {clientId} için oyuncu bulunamadı!");
        return null;
    }
    
    // Public method for PlayerBombToucher and PlayerAttack to transfer bomb
    public void TransferBombToClient(ulong targetClientId)
    {
        if (IsServer)
        {
            // Server'daysa direkt transfer et
            GameObject currentHolder = GetPlayerByClientId(currentBombHolderClientId.Value);
            GameObject newHolder = GetPlayerByClientId(targetClientId);
            
            Debug.Log($"🔄 SERVER BOMBA TRANSFERİ! {currentHolder?.name} → {newHolder?.name}");
            SetBombHolderServerRpc(targetClientId);
        }
        else
        {
            // Client'taysa Server'a istek gönder
            Debug.Log($"📡 CLIENT'TAN TRANSFER İSTEĞİ BAŞLATIYOR: Target {targetClientId}");
            RequestBombTransferServerRpc(targetClientId);
            Debug.Log($"📡 CLIENT: RequestBombTransferServerRpc ÇAĞRILDI - Target {targetClientId}");
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestBombTransferServerRpc(ulong targetClientId)
    {
        Debug.Log($"📨 SERVER: RequestBombTransferServerRpc ÇAĞRILDI! Target: {targetClientId}");
        Debug.Log($"📨 SERVER: İstek başarıyla alındı, transfer başlıyor...");
        
        GameObject currentHolder = GetPlayerByClientId(currentBombHolderClientId.Value);
        GameObject newHolder = GetPlayerByClientId(targetClientId);
        
        Debug.Log($"📨 SERVER: CurrentHolder: {currentHolder?.name}, NewHolder: {newHolder?.name}");
        Debug.Log($"🔄 SERVER BOMBA TRANSFERİ! {currentHolder?.name} → {newHolder?.name}");
        
        SetBombHolderServerRpc(targetClientId);
        Debug.Log($"📨 SERVER: SetBombHolderServerRpc ÇAĞRILDI - Target {targetClientId}");
    }
}







