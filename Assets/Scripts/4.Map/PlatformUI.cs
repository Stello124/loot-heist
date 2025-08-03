using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

/// <summary>
/// 4.map platform oyunu UI sistemi - Manuel düzenlenebilir versiyon
/// Inspector'dan UI elemanlarını bağla ve özelleştir
/// </summary>
public class PlatformUI : MonoBehaviour
{
    [Header("🎨 UI Panel Bağlantıları")]
    [Tooltip("Bekleme ve countdown için ana panel")]
    public GameObject startPanel;
    [Tooltip("Kazanan gösterimi için panel")]
    public GameObject winnerPanel;

    [Header("📝 UI Text Bağlantıları")]
    [Tooltip("Bekleme/countdown text'i")]
    public TextMeshProUGUI startText;
    [Tooltip("Kazanan text'i")]
    public TextMeshProUGUI winnerText;

    [Header("🔲 UI Button Bağlantıları")]
    [Tooltip("Tekrar oyna butonu")]
    public Button playAgainButton;
    [Tooltip("Lobby'e dön butonu")]
    public Button backToLobbyButton;

    [Header("⚙️ Text Özelleştirmeleri")]
    [Space(10)]
    [Header("Waiting Phase Ayarları")]
    public string waitingTitleText = "Platform Race";
    public string waitingSubText = "İlk bitiren kazanır!";
    public float waitingFontSize = 42f;
    public Color waitingTextColor = Color.white;

    [Header("Countdown Phase Ayarları")]
    public string countdownTitleText = "PLATFORM RACE BAŞLIYOR!";
    public string countdownSubText = "İlk bitiren kazanır!";
    public float countdownFontSize = 48f;
    public Color countdownTextColor = Color.yellow;

    [Header("Winner Phase Ayarları")]
    public string winnerTitleWin = "🏆 KAZANDIN! 🏆";
    public string winnerTitleLose = "🏆 {WINNER} KAZANDI! 🏆";
    public string winnerSubText = "Platform Race Şampiyonu!";
    public float winnerFontSize = 56f;
    public Color winnerColorWin = Color.green;
    public Color winnerColorLose = Color.yellow;

    private void Start()
    {
        ValidateUIComponents();
        SetupButtons();
        ResetUI();
    }

    private void ValidateUIComponents()
    {
        bool hasErrors = false;

        if (startPanel == null)
        {
            Debug.LogError("❌ Start Panel atanmamış! Inspector'da bağlayın. (4.map)");
            hasErrors = true;
        }

        if (winnerPanel == null)
        {
            Debug.LogError("❌ Winner Panel atanmamış! Inspector'da bağlayın. (4.map)");
            hasErrors = true;
        }

        if (startText == null)
        {
            Debug.LogError("❌ Start Text atanmamış! Inspector'da bağlayın. (4.map)");
            hasErrors = true;
        }

        if (winnerText == null)
        {
            Debug.LogError("❌ Winner Text atanmamış! Inspector'da bağlayın. (4.map)");
            hasErrors = true;
        }

        if (hasErrors)
        {
            Debug.LogError("🚨 UI SETUP HATASI: Manuel olarak UI elemanlarını oluşturup Inspector'da bağlayın! (4.map)");
            Debug.LogError("📖 Detaylı rehber: 4MAP_MANUAL_UI_SETUP_REHBERI.md");
        }
        else
        {
            Debug.Log("✅ Tüm UI elemanları başarıyla bağlandı! (4.map)");
        }
    }

    private void SetupButtons()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(OnPlayAgain);
        }

        if (backToLobbyButton != null)
        {
            backToLobbyButton.onClick.RemoveAllListeners();
            backToLobbyButton.onClick.AddListener(OnBackToLobby);
        }
    }

    public void ShowWaitingForPlayers(int joinedCount, int expectedCount)
    {
        Debug.Log($"📺 ShowWaitingForPlayers çağrıldı: {joinedCount}/{expectedCount} (4.map)");
        
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            Debug.Log($"✅ StartPanel aktif edildi - Active: {startPanel.activeInHierarchy} (4.map)");
        }
        else
        {
            Debug.LogError("❌ StartPanel null! (4.map)");
        }

        if (startText != null)
        {
            // Özelleştirilebilir text formatı
            string formattedText = $"Oyuncular Bekleniyor...\n{joinedCount}/{expectedCount}";
            
            if (!string.IsNullOrEmpty(waitingTitleText))
                formattedText += $"\n\n{waitingTitleText}";
                
            if (!string.IsNullOrEmpty(waitingSubText))
                formattedText += $"\n{waitingSubText}";

            startText.text = formattedText;
            startText.fontSize = waitingFontSize;
            startText.color = waitingTextColor;
            Debug.Log($"✅ StartText güncellendi: {startText.text} (4.map)");
        }
        else
        {
            Debug.LogError("❌ StartText null! (4.map)");
        }
        
        // Diğer panelleri gizle
        if (winnerPanel != null) winnerPanel.SetActive(false);
    }

    public void UpdateWaitingTimer(int timeLeft, int joinedCount, int expectedCount)
    {
        if (startText != null)
        {
            // Özelleştirilebilir timer formatı
            string formattedText = $"Oyuncular Bekleniyor...\n{joinedCount}/{expectedCount}\n{timeLeft}s";
            
            if (!string.IsNullOrEmpty(waitingTitleText))
                formattedText += $"\n\n{waitingTitleText}";
                
            if (!string.IsNullOrEmpty(waitingSubText))
                formattedText += $"\n{waitingSubText}";

            startText.text = formattedText;
            Debug.Log($"⏰ Waiting timer güncellendi: {timeLeft}s (4.map)");
        }
    }

    public void ShowCountdown(int countdownValue)
    {
        Debug.Log($"📺 ShowCountdown çağrıldı: {countdownValue} (4.map)");
        
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            Debug.Log($"✅ StartPanel countdown için aktif - Active: {startPanel.activeInHierarchy} (4.map)");
        }
        else
        {
            Debug.LogError("❌ StartPanel null! (4.map)");
            return;
        }

        if (startText != null)
        {
            // Özelleştirilebilir countdown formatı
            string formattedText = "";
            
            if (!string.IsNullOrEmpty(countdownTitleText))
                formattedText += $"{countdownTitleText}\n";
                
            formattedText += $"{countdownValue}";
            
            if (!string.IsNullOrEmpty(countdownSubText))
                formattedText += $"\n\n{countdownSubText}";

            startText.text = formattedText;
            startText.fontSize = countdownFontSize;
            startText.color = countdownTextColor;
            Debug.Log($"✅ Countdown text güncellendi: {startText.text} (4.map)");
        }
        else
        {
            Debug.LogError("❌ StartText null! (4.map)");
        }
    }

    public void UpdateCountdown(int countdownValue)
    {
        if (startText != null)
        {
            // Özelleştirilebilir countdown update formatı
            string formattedText = "";
            
            if (!string.IsNullOrEmpty(countdownTitleText))
                formattedText += $"{countdownTitleText}\n";
                
            formattedText += $"{countdownValue}";
            
            if (!string.IsNullOrEmpty(countdownSubText))
                formattedText += $"\n\n{countdownSubText}";

            startText.text = formattedText;
            Debug.Log($"⏰ Countdown güncellendi: {countdownValue} (4.map)");
        }
    }

    public void HideStartPanel()
    {
        Debug.Log("📺 HideStartPanel çağrıldı (4.map)");
        
        if (startPanel != null)
        {
            startPanel.SetActive(false);
            Debug.Log("✅ StartPanel gizlendi (4.map)");
        }
        else
        {
            Debug.LogError("❌ StartPanel null! (4.map)");
        }
    }

    public void ShowWinner(ulong winnerClientId)
    {
        Debug.Log($"📺 ShowWinner çağrıldı: {winnerClientId} (4.map)");
        
        if (startPanel != null) startPanel.SetActive(false);
        
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);
            Debug.Log("✅ WinnerPanel gösterildi (4.map)");
        }
        else
        {
            Debug.LogError("❌ WinnerPanel null! (4.map)");
            return;
        }

        if (winnerText != null)
        {
            string winnerName = $"Oyuncu {winnerClientId}";
            bool isLocalPlayerWinner = NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == winnerClientId;
            
            // Özelleştirilebilir winner formatı
            string formattedText = "";
            Color textColor;
            
            if (isLocalPlayerWinner)
            {
                // Ben kazandım
                formattedText = winnerTitleWin;
                textColor = winnerColorWin;
            }
            else
            {
                // Başkası kazandı
                formattedText = winnerTitleLose.Replace("{WINNER}", winnerName);
                textColor = winnerColorLose;
            }
            
            if (!string.IsNullOrEmpty(winnerSubText))
                formattedText += $"\n\n{winnerSubText}";

            winnerText.text = formattedText;
            winnerText.fontSize = winnerFontSize;
            winnerText.color = textColor;
            Debug.Log($"✅ WinnerText güncellendi: {winnerText.text} (4.map)");
        }
        else
        {
            Debug.LogError("❌ WinnerText null! (4.map)");
        }
    }

    private void ResetUI()
    {
        Debug.Log("🔄 UI sıfırlanıyor (4.map)");
        
        if (startPanel != null) startPanel.SetActive(false);
        if (winnerPanel != null) winnerPanel.SetActive(false);
        
        Debug.Log("✅ UI sıfırlandı (4.map)");
    }

    private void OnPlayAgain()
    {
        Debug.Log("🎮 Play Again butonu tıklandı (4.map)");
        
        // Sahneyi yeniden yükle
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("4.map", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("⚠️ Sadece Host sahne değiştirebilir!");
        }
    }

    private void OnBackToLobby()
    {
        Debug.Log("🏠 Back to Lobby butonu tıklandı (4.map)");
        
        // Lobby'e dön
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("LobbyRoom", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("⚠️ Sadece Host sahne değiştirebilir!");
        }
    }

    // Debug ve test için context menu'ler
    [ContextMenu("🔍 Debug UI State")]
    public void DebugUIState()
    {
        Debug.Log($"🔍 UI State (4.map):");
        Debug.Log($"  StartPanel: {(startPanel ? (startPanel.activeInHierarchy ? "Active" : "Inactive") : "NULL")}");
        Debug.Log($"  WinnerPanel: {(winnerPanel ? (winnerPanel.activeInHierarchy ? "Active" : "Inactive") : "NULL")}");
        Debug.Log($"  StartText: {(startText ? startText.text : "NULL")}");
        Debug.Log($"  WinnerText: {(winnerText ? winnerText.text : "NULL")}");
    }

    [ContextMenu("🧪 Test Waiting UI")]
    public void TestWaitingUI()
    {
        Debug.Log("🧪 Testing Waiting UI...");
        ShowWaitingForPlayers(2, 4);
    }

    [ContextMenu("🧪 Test Countdown UI")]
    public void TestCountdownUI()
    {
        Debug.Log("🧪 Testing Countdown UI...");
        ShowCountdown(5);
    }

    [ContextMenu("🧪 Test Winner UI (Win)")]
    public void TestWinnerUIWin()
    {
        Debug.Log("🧪 Testing Winner UI (Win)...");
        // Simulate local player win
        if (NetworkManager.Singleton != null)
        {
            ShowWinner(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            ShowWinner(0); // Fallback
        }
    }

    [ContextMenu("🧪 Test Winner UI (Lose)")]
    public void TestWinnerUILose()
    {
        Debug.Log("🧪 Testing Winner UI (Lose)...");
        // Simulate other player win
        ShowWinner(999); // Different client ID
    }

    [ContextMenu("🔄 Reset UI")]
    public void ManualResetUI()
    {
        Debug.Log("🔄 Manuel UI Reset...");
        ResetUI();
    }

    [ContextMenu("⚙️ Apply Custom Settings")]
    public void ApplyCustomSettings()
    {
        Debug.Log("⚙️ Custom ayarlar uygulanıyor...");
        Debug.Log($"Waiting: {waitingTitleText} | {waitingSubText}");
        Debug.Log($"Countdown: {countdownTitleText} | {countdownSubText}");
        Debug.Log($"Winner Win: {winnerTitleWin}");
        Debug.Log($"Winner Lose: {winnerTitleLose}");
        Debug.Log("Inspector'da değişiklik yap ve tekrar test et!");
    }
}