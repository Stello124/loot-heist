using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using Controller;

/// <summary>
/// 4.map platform oyunu yöneticisi - 1.map'teki RaceGameManager'dan uyarlandı
/// Oyuncuları bekler, countdown yapar, platform oyununu başlatır
/// </summary>
public class PlatformGameManager : NetworkBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private bool gameStarted = false;
    [SerializeField] private bool gameEnded = false;
    [SerializeField] private float waitingTime = 30f; // Oyuncu bekleme süresi
    [SerializeField] private float countdownTime = 8f; // Geri sayım süresi
    
    [Header("UI References")]
    [SerializeField] private PlatformUI platformUI; // Kullanıcı arayüzü

    [Header("Platform Game Settings")]
    [SerializeField] private GameObject finishArea; // Bitiş alanı (zorunlu)
    [SerializeField] private bool lastPlayerWins = false; // false = İlk bitiren kazanır, true = Son kalan kazanır

    // Network variables
    private NetworkVariable<bool> isGameActive = new NetworkVariable<bool>(false);
    private NetworkVariable<ulong> winnerClientId = new NetworkVariable<ulong>(999); // 999 = henüz kazanan yok
    private NetworkVariable<int> gamePhase = new NetworkVariable<int>(0); // 0=waiting, 1=countdown, 2=active, 3=ended
    private NetworkVariable<float> phaseTimer = new NetworkVariable<float>(30f);

    public static PlatformGameManager Instance;

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
        // UI'yi bul veya validate et
        if (platformUI == null)
        {
            platformUI = FindObjectOfType<PlatformUI>();
            if (platformUI != null)
            {
                Debug.Log("🎯 PlatformUI otomatik bulundu ✅");
            }
            else
            {
                Debug.LogError("❌ PlatformUI bulunamadı! Inspector'da PlatformUI'yi manuel bağlayın veya sahneye PlatformUI GameObject'i ekleyin.");
            }
        }
        else
        {
            Debug.Log("🎯 PlatformUI Inspector'dan bağlandı ✅");
        }

        // Network variable değişikliklerini dinle
        winnerClientId.OnValueChanged += OnWinnerChanged;
        isGameActive.OnValueChanged += OnGameStateChanged;
        gamePhase.OnValueChanged += OnPhaseChanged;
        phaseTimer.OnValueChanged += OnTimerChanged;

        // Bekleme fazını başlat
        if (IsServer)
        {
            Debug.Log("🎮 Server olarak bekleme fazı başlatılıyor... (4.map)");
            StartCoroutine(WaitForPlayersAndStart());
        }
        else
        {
            Debug.Log("👤 Client olarak bekleme dinleniyor (4.map)");
        }

        Debug.Log("🟡 PlatformGameManager başlatıldı (4.map)");
    }

    public override void OnNetworkDespawn()
    {
        winnerClientId.OnValueChanged -= OnWinnerChanged;
        isGameActive.OnValueChanged -= OnGameStateChanged;
        gamePhase.OnValueChanged -= OnPhaseChanged;
        phaseTimer.OnValueChanged -= OnTimerChanged;
    }

    private IEnumerator WaitForPlayersAndStart()
    {
        yield return new WaitForSeconds(1f); // Sahne yüklensin

        Debug.Log("🟡 4.map platform oyunu için oyuncular bekleniyor...");
        StartWaitingPhase();
    }

    private void StartWaitingPhase()
    {
        if (!IsServer) return;

        gamePhase.Value = 0; // Waiting phase
        phaseTimer.Value = waitingTime;
        isGameActive.Value = false;
        winnerClientId.Value = 999;

        Debug.Log("⏰ Bekleme fazı başladı - Oyuncular donduruldu (4.map)");
        
        // Oyuncuları dondur
        FreezeAllPlayersClientRpc(true);
        
        // UI'yi göster
        ShowWaitingUIClientRpc();
        
        // Timer başlat
        StartCoroutine(WaitingTimerCoroutine());
    }

    [ClientRpc]
    private void ShowWaitingUIClientRpc()
    {
        Debug.Log("📺 ShowWaitingUI çağrıldı (4.map)");
        if (platformUI != null)
        {
            platformUI.ShowWaitingForPlayers(1, 4);
            Debug.Log("✅ Waiting UI gösterildi (4.map)");
        }
        else
        {
            Debug.LogError("❌ PlatformUI null! (4.map)");
        }
    }

    private IEnumerator WaitingTimerCoroutine()
    {
        while (phaseTimer.Value > 0 && gamePhase.Value == 0)
        {
            yield return new WaitForSeconds(1f);
            phaseTimer.Value = Mathf.Max(0, phaseTimer.Value - 1f);

            // UI güncellemesi
            UpdateWaitingUIClientRpc((int)phaseTimer.Value);
        }

        // Waiting bitince countdown başlat
        if (gamePhase.Value == 0) // Hala waiting fazındaysak
        {
            Debug.Log("⏰ Waiting süresi doldu, countdown başlıyor (4.map)");
            StartCountdownPhase();
        }
    }

    [ClientRpc]
    private void UpdateWaitingUIClientRpc(int timeLeft)
    {
        if (platformUI != null)
        {
            platformUI.UpdateWaitingTimer(timeLeft, GetConnectedPlayerCount(), 4);
        }
    }

    private void StartCountdownPhase()
    {
        if (!IsServer) return;

        gamePhase.Value = 1; // Countdown phase
        phaseTimer.Value = countdownTime;

        Debug.Log("🚀 Countdown fazı başladı (4.map)");
        
        // Countdown UI göster
        ShowCountdownUIClientRpc();
        
        // Countdown timer başlat
        StartCoroutine(CountdownTimerCoroutine());
    }

    [ClientRpc]
    private void ShowCountdownUIClientRpc()
    {
        Debug.Log("📺 ShowCountdownUI çağrıldı (4.map)");
        if (platformUI != null)
        {
            platformUI.ShowCountdown((int)phaseTimer.Value);
        }
    }

    private IEnumerator CountdownTimerCoroutine()
    {
        while (phaseTimer.Value > 0 && gamePhase.Value == 1)
        {
            yield return new WaitForSeconds(1f);
            phaseTimer.Value = Mathf.Max(0, phaseTimer.Value - 1f);

            // UI güncellemesi
            UpdateCountdownUIClientRpc((int)phaseTimer.Value);
        }

        // Countdown bitince oyunu başlat
        if (gamePhase.Value == 1) // Hala countdown fazındaysak
        {
            Debug.Log("🟡 Countdown bitti, platform oyunu başlıyor! (4.map)");
            StartPlatformGame();
        }
    }

    [ClientRpc]
    private void UpdateCountdownUIClientRpc(int timeLeft)
    {
        if (platformUI != null)
        {
            platformUI.UpdateCountdown(timeLeft);
        }
    }

    private void StartPlatformGame()
    {
        if (!IsServer) return;

        gamePhase.Value = 2; // Active phase
        isGameActive.Value = true;
        gameStarted = true;

        Debug.Log("🟡 Platform race başladı! İlk bitiren kazanır! (4.map)");

        // Oyuncuları serbest bırak
        FreezeAllPlayersClientRpc(false);
        
        // UI'yi gizle
        HideStartUIClientRpc();
    }

    [ClientRpc]
    private void HideStartUIClientRpc()
    {
        Debug.Log("📺 HideStartUI çağrıldı (4.map)");
        if (platformUI != null)
        {
            platformUI.HideStartPanel();
        }
    }

    [ClientRpc]
    private void FreezeAllPlayersClientRpc(bool freeze)
    {
        Debug.Log($"🧊 FreezeAllPlayers çağrıldı: {freeze} (4.map)");
        
        // Her client sadece kendi player'ını freeze etsin (1.map pattern)
        var allObjects = FindObjectsOfType<GameObject>();
        int frozenPlayers = 0;

        foreach (var obj in allObjects)
        {
            if (obj.CompareTag("Player"))
            {
                var netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null && netObj.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                {
                    // Sadece kendi player'ını freeze et
                    StartCoroutine(FreezePlayerComponentsAfterDelay(obj, freeze, 0.1f));
                    frozenPlayers++;
                    Debug.Log($"🧊 Local player freeze edildi: {NetworkManager.Singleton.LocalClientId} (4.map)");
                    break; // Sadece bir player bulduğumuzda dur
                }
            }
        }

        Debug.Log($"🧊 {frozenPlayers} local oyuncu {(freeze ? "donduruldu" : "serbest bırakıldı")} (4.map)");
    }

    private IEnumerator FreezePlayerComponentsAfterDelay(GameObject player, bool freeze, float delay)
    {
        yield return new WaitForSeconds(delay);

        // CharacterMover'ı kontrol et
        var characterMover = player.GetComponent<CharacterMover>();
        if (characterMover != null)
        {
            characterMover.enabled = !freeze;
            Debug.Log($"🧊 CharacterMover.enabled = {!freeze} (4.map)");
        }

        // MovePlayerInput'u kontrol et
        var moveInput = player.GetComponent<MovePlayerInput>();
        if (moveInput != null)
        {
            moveInput.enabled = !freeze;
            Debug.Log($"🧊 MovePlayerInput.enabled = {!freeze} (4.map)");
        }
    }

    public void PlayerFinished(ulong clientId)
    {
        if (!IsServer || !isGameActive.Value || gameEnded) return;

        Debug.Log($"🟡 Player finished: {clientId} (4.map)");

        winnerClientId.Value = clientId;
        EndGame();
    }

    public void PlayerEliminated(ulong clientId)
    {
        if (!IsServer || !isGameActive.Value || gameEnded) return;

        Debug.Log($"💀 Player eliminated: {clientId} (4.map)");

        // LastPlayerWins modunda: Kalan oyuncu sayısını kontrol et
        if (lastPlayerWins)
        {
            int remainingPlayers = GetConnectedPlayerCount();
            if (remainingPlayers <= 1)
            {
                // Son oyuncu kaldı - o kazandı
                foreach (ulong playerId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (playerId != clientId) // Eliminate olan haricindeki
                    {
                        winnerClientId.Value = playerId;
                        break;
                    }
                }
                EndGame();
            }
        }
        else
        {
            Debug.Log("⚠️ Race modunda elimination göz ardı edildi - sadece respawn çalışır (4.map)");
        }
    }

    private void EndGame()
    {
        if (!IsServer) return;

        gamePhase.Value = 3; // Ended phase
        isGameActive.Value = false;
        gameEnded = true;

        Debug.Log($"🏆 Platform race bitti! Kazanan: {winnerClientId.Value} (4.map)");

        // Oyuncuları dondur
        FreezeAllPlayersClientRpc(true);
        
        // Kazanan UI göster
        ShowWinnerUIClientRpc(winnerClientId.Value);
    }

    [ClientRpc]
    private void ShowWinnerUIClientRpc(ulong winner)
    {
        Debug.Log($"📺 ShowWinnerUI çağrıldı: {winner} (4.map)");
        if (platformUI != null)
        {
            platformUI.ShowWinner(winner);
        }
    }

    // Event handlers
    private void OnWinnerChanged(ulong oldValue, ulong newValue)
    {
        Debug.Log($"🏆 Winner changed: {oldValue} → {newValue} (4.map)");
    }

    private void OnGameStateChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"⚙️ Game state changed: {oldValue} → {newValue} (4.map)");
    }

    private void OnPhaseChanged(int oldValue, int newValue)
    {
        Debug.Log($"🔄 Phase changed: {oldValue} → {newValue} (4.map)");
    }

    private void OnTimerChanged(float oldValue, float newValue)
    {
        Debug.Log($"⏰ Timer changed: {oldValue:F1} → {newValue:F1} (4.map)");
    }

    // Utility methods
    public bool IsGameActive()
    {
        return isGameActive.Value;
    }

    private int GetConnectedPlayerCount()
    {
        return NetworkManager.Singleton?.ConnectedClientsIds?.Count ?? 0;
    }

    // Yeni oyuncu katıldığında
    public void OnNewPlayerSpawned(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"🆕 Yeni oyuncu spawn oldu: {clientId}, Current phase: {gamePhase.Value} (4.map)");
        
        // Eğer waiting fazındaysak yeni oyuncuyu da dondur
        if (gamePhase.Value == 0) // Waiting phase
        {
            Debug.Log($"🧊 Yeni oyuncuyu donduruyor: {clientId} (4.map)");
            StartCoroutine(FreezeNewPlayerAfterDelay(clientId));
        }
    }

    private IEnumerator FreezeNewPlayerAfterDelay(ulong clientId)
    {
        yield return new WaitForSeconds(0.3f);
        FreezeSpecificPlayerClientRpc(clientId, true);
        ShowWaitingUIToSpecificPlayerClientRpc(clientId);
    }

    [ClientRpc]
    private void FreezeSpecificPlayerClientRpc(ulong targetClientId, bool freeze)
    {
        // Sadece belirtilen client bu mesajı işlesin (1.map pattern)
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        Debug.Log($"🧊 Specific player freeze çağrıldı. Target: {targetClientId}, Local: {NetworkManager.Singleton.LocalClientId}, Freeze: {freeze} (4.map)");
        
        var allObjects = FindObjectsOfType<GameObject>();
        int frozenPlayers = 0;
        foreach (var obj in allObjects)
        {
            if (obj.CompareTag("Player"))
            {
                var netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null && netObj.OwnerClientId == targetClientId)
                {
                    StartCoroutine(FreezePlayerComponentsAfterDelay(obj, freeze, 0.1f));
                    frozenPlayers++;
                    Debug.Log($"🧊 Target player freeze edildi: {targetClientId} (4.map)");
                    break;
                }
            }
        }

        Debug.Log($"🧊 Sonuç: {frozenPlayers} target player freeze edildi (4.map)");
    }

    [ClientRpc]
    private void ShowWaitingUIToSpecificPlayerClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        if (platformUI != null)
        {
            platformUI.ShowWaitingForPlayers(1, 4);
        }
    }
}