using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using Controller;

/// <summary>
/// 1.map yarış oyunu yöneticisi - Kazananı belirler ve oyunu bitirir
/// </summary>
public class RaceGameManager : NetworkBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private bool gameStarted = false;
    [SerializeField] private bool gameEnded = false;
    [SerializeField] private float waitingTime = 30f; // Oyuncu bekleme süresi
    [SerializeField] private float countdownTime = 8f; // Geri sayım süresi
    
    [Header("UI References")]
    [SerializeField] private RaceUI raceUI; // Kullanıcı arayüzü

    // Network variables
    private NetworkVariable<bool> isGameActive = new NetworkVariable<bool>(false); // Başlangıçta false
    private NetworkVariable<ulong> winnerClientId = new NetworkVariable<ulong>(999); // 999 = henüz kazanan yok
    private NetworkVariable<int> gamePhase = new NetworkVariable<int>(0); // 0=waiting, 1=countdown, 2=active, 3=ended
    private NetworkVariable<float> phaseTimer = new NetworkVariable<float>(30f);

    public static RaceGameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        // UI'yi bul
        if (raceUI == null)
        {
            raceUI = FindObjectOfType<RaceUI>();
            Debug.Log($"🎯 RaceUI bulundu: {(raceUI != null ? "✅" : "❌")}");
        }

        // Network variable değişikliklerini dinle
        winnerClientId.OnValueChanged += OnWinnerChanged;
        isGameActive.OnValueChanged += OnGameStateChanged;
        gamePhase.OnValueChanged += OnPhaseChanged;
        phaseTimer.OnValueChanged += OnTimerChanged;

        // Bekleme fazını başlat
        if (IsServer)
        {
            Debug.Log("🎮 Server olarak bekleme fazı başlatılıyor...");
            // Player spawn'ını bekle
            StartCoroutine(WaitForPlayersAndStart());
        }
        else
        {
            Debug.Log("👤 Client olarak bekleme dinleniyor");
        }

        Debug.Log("🏁 RaceGameManager başlatıldı");
    }

    public override void OnNetworkDespawn()
    {
        winnerClientId.OnValueChanged -= OnWinnerChanged;
        isGameActive.OnValueChanged -= OnGameStateChanged;
        gamePhase.OnValueChanged -= OnPhaseChanged;
        phaseTimer.OnValueChanged -= OnTimerChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartRaceServerRpc()
    {
        StartRace();
    }

    private void StartWaitingPhase()
    {
        if (!IsServer) return;

        gamePhase.Value = 0; // Waiting phase
        phaseTimer.Value = waitingTime;
        isGameActive.Value = false;
        winnerClientId.Value = 999;

        Debug.Log("⏰ Bekleme fazı başladı - Oyuncular donduruldu");
        
        // Oyuncuları dondur
        FreezeAllPlayersClientRpc(true);
        
        // UI'yi direkt çağır (client'larda da çalışsın)
        ShowWaitingUIClientRpc();
        
        // Timer başlat
        StartCoroutine(WaitingTimerCoroutine());
    }

    [ClientRpc]
    private void ShowWaitingUIClientRpc()
    {
        Debug.Log("📺 ShowWaitingUI çağrıldı");
        if (raceUI != null)
        {
            raceUI.ShowWaitingForPlayers(1, 4);
            Debug.Log("✅ Waiting UI gösterildi");
        }
        else
        {
            Debug.LogError("❌ RaceUI null!");
        }
    }

    // Yeni oyuncu için özel metod
    public void OnNewPlayerSpawned(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"🆕 Yeni oyuncu spawn oldu: {clientId}, Current phase: {gamePhase.Value}");
        
        // Eğer waiting fazındaysak yeni oyuncuyu da dondur ve UI göster
        if (gamePhase.Value == 0) // Waiting phase
        {
            Debug.Log($"🧊 Yeni oyuncuyu donduruyor: {clientId}");
            StartCoroutine(FreezeNewPlayerAfterDelay(clientId));
        }
    }

    private System.Collections.IEnumerator FreezeNewPlayerAfterDelay(ulong clientId)
    {
        // Oyuncunun tamamen spawn olmasını bekle
        yield return new WaitForSeconds(0.3f);
        
        // Sadece bu oyuncuyu dondur
        FreezeSpecificPlayerClientRpc(clientId, true);
        
        // UI'sını göster
        ShowWaitingUIToSpecificPlayerClientRpc(clientId);
    }

    [ClientRpc]
    private void FreezeSpecificPlayerClientRpc(ulong targetClientId, bool freeze)
    {
        // Sadece belirtilen client bu mesajı işlesin
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        Debug.Log($"🧊 Specific player freeze çağrıldı. Target: {targetClientId}, Local: {NetworkManager.Singleton.LocalClientId}, Freeze: {freeze}");
        
        var allObjects = FindObjectsOfType<GameObject>();
        int frozenPlayers = 0;
        int frozenComponents = 0;

        foreach (var obj in allObjects)
        {
            if (obj.CompareTag("Player"))
            {
                var netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null && netObj.OwnerClientId == targetClientId)
                {
                    StartCoroutine(FreezePlayerComponentsAfterDelay(obj, freeze, 0.1f));
                    frozenPlayers++;
                    break;
                }
            }
        }

        Debug.Log($"🧊 Sonuç: {frozenPlayers} player, {frozenComponents} component donduruldu");
    }

    [ClientRpc]
    private void ShowWaitingUIToSpecificPlayerClientRpc(ulong targetClientId)
    {
        // Sadece belirtilen client bu UI'yi görsün
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        Debug.Log($"📺 Specific ShowWaitingUI çağrıldı for: {targetClientId}");
        if (raceUI != null)
        {
            raceUI.ShowWaitingForPlayers(1, 4);
            Debug.Log("✅ Specific Waiting UI gösterildi");
        }
        else
        {
            Debug.LogError("❌ RaceUI null!");
        }
    }

    private System.Collections.IEnumerator WaitingTimerCoroutine()
    {
        while (phaseTimer.Value > 0 && gamePhase.Value == 0)
        {
            yield return new WaitForSeconds(1f);
            phaseTimer.Value = Mathf.Max(0, phaseTimer.Value - 1f);
        }
        
        // Waiting bitti, countdown başlat
        if (gamePhase.Value == 0) // Hala waiting phase'de
        {
            StartCountdownPhase();
        }
    }

    private void StartCountdownPhase()
    {
        if (!IsServer) return;

        gamePhase.Value = 1; // Countdown phase
        phaseTimer.Value = countdownTime;

        Debug.Log("🚀 Countdown fazı başladı");
        
        // UI'yi direkt çağır
        ShowCountdownUIClientRpc();
        
        StartCoroutine(CountdownTimerCoroutine());
    }

    [ClientRpc]
    private void ShowCountdownUIClientRpc()
    {
        Debug.Log("📺 ShowCountdownUI çağrıldı");
        if (raceUI != null)
        {
            raceUI.ShowCountdown((int)phaseTimer.Value);
            Debug.Log("✅ Countdown UI gösterildi");
        }
    }

    private System.Collections.IEnumerator CountdownTimerCoroutine()
    {
        while (phaseTimer.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            phaseTimer.Value = Mathf.Max(0, phaseTimer.Value - 1f);
        }
        
        // Countdown bitti, oyunu başlat
        StartRace();
    }

    private void StartRace()
    {
        if (!IsServer) return;

        gamePhase.Value = 2; // Active phase
        gameStarted = true;
        gameEnded = false;
        isGameActive.Value = true;

        Debug.Log("🏁 Yarış başladı - Oyuncular serbest!");
        
        // Oyuncuları serbest bırak
        FreezeAllPlayersClientRpc(false);
        
        // UI'yi gizle
        HideStartUIClientRpc();
    }

    [ClientRpc]
    private void HideStartUIClientRpc()
    {
        Debug.Log("📺 HideStartUI çağrıldı");
        if (raceUI != null)
        {
            raceUI.HideStartPanels();
            Debug.Log("✅ Start paneller gizlendi");
        }
    }

    public void PlayerFinished(ulong clientId)
    {
        if (!IsServer || gameEnded || gamePhase.Value != 2) return; // Sadece active phase'de

        // İlk gelen kazanır!
        gamePhase.Value = 3; // Ended phase
        winnerClientId.Value = clientId;
        isGameActive.Value = false;
        gameEnded = true;

        Debug.Log($"🏆 KAZANAN: Client {clientId}");

        // Oyuncuları tekrar dondur
        FreezeAllPlayersClientRpc(true);

        // Kazanan kişiden ismini iste
        RequestWinnerNameClientRpc(clientId);
        
        // Finish line efektlerini göster
        FinishLineTrigger finishLine = FindObjectOfType<FinishLineTrigger>();
        if (finishLine != null)
        {
            finishLine.ShowFinishEffectsClientRpc();
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

        Debug.Log($"🏆 Kazanan olarak ismimi gönderiyorum: {myName}");
        
        // İsmimi server'a gönder
        SendWinnerNameServerRpc(myName, winnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendWinnerNameServerRpc(string winnerName, ulong winnerClientId)
    {
        Debug.Log($"🏆 Server aldı kazanan ismi: {winnerName} (Client {winnerClientId})");
        
        // Tüm client'lara kazananın gerçek ismini gönder
        ShowWinnerWithNameClientRpc(winnerName, winnerClientId);
    }

    [ClientRpc]
    private void ShowWinnerWithNameClientRpc(string winnerName, ulong winnerClientId)
    {
        Debug.Log($"🏆 Kazanan ismiyle birlikte gösteriliyor: {winnerName} (Client {winnerClientId})");
        
        if (raceUI != null)
        {
            raceUI.ShowWinner(winnerName, winnerClientId);
        }

        Debug.Log($"🏆 Yarış bitti! Kazanan: {winnerName} (Client {winnerClientId})");
    }

    [ClientRpc]
    private void FreezeAllPlayersClientRpc(bool freeze)
    {
        Debug.Log($"🧊 FreezeAllPlayers çağrıldı: freeze={freeze}");
        
        // Biraz bekle ki player'lar tam spawn olsun
        StartCoroutine(FreezePlayersAfterDelay(freeze, 0.5f));
    }

    private System.Collections.IEnumerator FreezePlayersAfterDelay(bool freeze, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        var allObjects = FindObjectsOfType<GameObject>();
        int playerCount = 0;
        int componentCount = 0;
        
        foreach (var obj in allObjects)
        {
            if (obj.CompareTag("Player"))
            {
                playerCount++;
                Debug.Log($"🎯 Player bulundu: {obj.name}");
                
                // CharacterController'ı kontrol et
                CharacterController charController = obj.GetComponent<CharacterController>();
                if (charController != null)
                {
                    charController.enabled = !freeze;
                    componentCount++;
                    Debug.Log($"✅ CharacterController {(freeze ? "donduruldu" : "aktif edildi")}");
                }
                
                // Tüm MonoBehaviour bileşenlerini kontrol et
                var components = obj.GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    if (comp != null && (comp.GetType().Name.Contains("CharacterMover") || 
                        comp.GetType().Name.Contains("MovePlayerInput")))
                    {
                        comp.enabled = !freeze;
                        componentCount++;
                        Debug.Log($"✅ {comp.GetType().Name} {(freeze ? "donduruldu" : "aktif edildi")}");
                    }
                }
            }
        }
        
        Debug.Log($"🧊 FINAL Sonuç: {playerCount} player, {componentCount} component {(freeze ? "donduruldu" : "aktif edildi")}");
    }

    // Tek bir player için freeze metodu
    private System.Collections.IEnumerator FreezePlayerComponentsAfterDelay(GameObject playerObj, bool freeze, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log($"🧊 Single player freeze başlıyor: {playerObj.name}, freeze={freeze}");
        
        int componentCount = 0;
        
        // CharacterController'ı kontrol et
        CharacterController charController = playerObj.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = !freeze;
            componentCount++;
            Debug.Log($"✅ CharacterController {(freeze ? "donduruldu" : "aktif edildi")}");
        }
        
        // Tüm MonoBehaviour bileşenlerini kontrol et
        var components = playerObj.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp != null && (comp.GetType().Name.Contains("CharacterMover") || 
                comp.GetType().Name.Contains("MovePlayerInput")))
            {
                comp.enabled = !freeze;
                componentCount++;
                Debug.Log($"✅ {comp.GetType().Name} {(freeze ? "donduruldu" : "aktif edildi")}");
            }
        }
        
        Debug.Log($"🧊 Single player sonuç: {componentCount} component {(freeze ? "donduruldu" : "aktif edildi")}");
    }

    private System.Collections.IEnumerator WaitForPlayersAndStart()
    {
        Debug.Log("⏰ Oyuncuların spawn olmasını bekliyorum...");
        
        // 1 saniye bekle ki oyuncular spawn olsun
        yield return new WaitForSeconds(1f);
        
        // Tekrar kontrol et
        var players = FindObjectsOfType<NetworkObject>();
        int playerCount = 0;
        foreach (var netObj in players)
        {
            if (netObj.CompareTag("Player"))
            {
                playerCount++;
            }
        }
        
        Debug.Log($"🎯 {playerCount} oyuncu bulundu, oyun başlatılıyor");
        StartWaitingPhase();
    }





    private void OnPhaseChanged(int oldPhase, int newPhase)
    {
        Debug.Log($"🎮 Phase değişti: {oldPhase} → {newPhase}");
        
        if (raceUI == null) return;
        
        switch (newPhase)
        {
            case 0: // Waiting
                raceUI.ShowWaitingForPlayers(1, 4); // Basit versiyon
                break;
                
            case 1: // Countdown
                raceUI.ShowCountdown((int)phaseTimer.Value);
                break;
                
            case 2: // Active
                raceUI.HideStartPanels();
                break;
                
            case 3: // Ended
                // Winner UI PlayerFinished'de gösterilecek
                break;
        }
    }

    private void OnTimerChanged(float oldTimer, float newTimer)
    {
        if (raceUI == null) return;
        
        switch (gamePhase.Value)
        {
            case 0: // Waiting
                raceUI.UpdateWaitingTimer((int)newTimer, 1, 4);
                break;
                
            case 1: // Countdown
                raceUI.UpdateCountdown((int)newTimer);
                break;
        }
    }

    private void OnWinnerChanged(ulong oldValue, ulong newValue)
    {
        // Kazanan değiştiğinde - Artık ShowWinnerWithNameClientRpc ile hallediyoruz
        Debug.Log($"🏆 Winner changed: {oldValue} → {newValue}");
    }

    private void OnGameStateChanged(bool oldValue, bool newValue)
    {
        // Oyun durumu değiştiğinde - Artık ShowWinnerWithNameClientRpc ile hallediyoruz  
        Debug.Log($"🎮 Game state changed: {oldValue} → {newValue}");
    }





    // Public methods for UI or other systems
    public bool IsGameActive() => gamePhase.Value == 2;
    public bool IsWaiting() => gamePhase.Value == 0;
    public bool IsCountdown() => gamePhase.Value == 1;
    public bool IsEnded() => gamePhase.Value == 3;
    public ulong GetWinnerClientId() => winnerClientId.Value;
    public bool HasWinner() => winnerClientId.Value != 999;
}