using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

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

        if (backToLobbyButton == null)
        {
            Debug.LogError("❌ Back To Lobby Button atanmamış! Inspector'da bağlayın. (4.map)");
            hasErrors = true;
        }
        
        // Not: playAgainButton artık kullanılmıyor - sadece backToLobbyButton gerekli

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
        if (backToLobbyButton != null)
        {
            Debug.Log($"🔧 Button setup başlıyor: {backToLobbyButton.name} (4.map)");
            
            backToLobbyButton.onClick.RemoveAllListeners();
            backToLobbyButton.onClick.AddListener(OnBackToLobby);
            
            // Button'un interactable olduğunu kontrol et
            if (!backToLobbyButton.interactable)
            {
                backToLobbyButton.interactable = true;
                Debug.Log("🔧 Button interactable yapıldı!");
            }
            
            Debug.Log($"✅ BackToLobby button listener eklendi: Interactable={backToLobbyButton.interactable} (4.map)");
        }
        else
        {
            Debug.LogError("❌ BackToLobby button NULL! Inspector'da bağlayın! (4.map)");
        }
        
        // EventSystem kontrolü
        var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("❌ EventSystem bulunamadı! UI etkileşimi çalışmayabilir!");
        }
        else
        {
            Debug.Log($"✅ EventSystem mevcut: {eventSystem.name}");
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
            
            // MOUSE KONTROLÜNÜ AKTİF ET!
            EnableMouseControl();
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

    /// <summary>
    /// Mouse kontrolünü aktif et - Winner ekranında mouse'u serbest bırak
    /// </summary>
    private void EnableMouseControl()
    {
        Debug.Log("🖱️ Mouse kontrolü aktif ediliyor... (4.map)");
        
        // Cursor'u görünür yap ve unlock et
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Mouse'un hareket etmesini sağlamak için Time.timeScale'i kontrol et
        if (Time.timeScale == 0f)
        {
            Debug.LogWarning("⚠️ Time.timeScale = 0! Mouse problemi olabilir.");
        }
        
        Debug.Log($"✅ Cursor ayarları: Visible={Cursor.visible}, LockState={Cursor.lockState}, TimeScale={Time.timeScale}");
        
        // Mouse'u sürekli korumak için coroutine başlat
        StartCoroutine(KeepMouseActive());
    }

    /// <summary>
    /// Mouse'u sürekli aktif tut - Başka scriptler kapatmasın diye
    /// </summary>
    private System.Collections.IEnumerator KeepMouseActive()
    {
        Debug.Log("🛡️ Mouse koruma sistemi başlatıldı...");
        
        while (winnerPanel != null && winnerPanel.activeInHierarchy)
        {
            // Her 0.1 saniyede mouse'u kontrol et ve aktif tut
            if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
            {
                Debug.Log("⚠️ Mouse tekrar kapatılmış! Yeniden açılıyor...");
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("🛡️ Mouse koruma sistemi sonlandırıldı.");
    }

    private void OnBackToLobby()
    {
        Debug.Log("🏠 ==> BUTTON TIKLANDI! Back to Lobby button çalışıyor! (4.map)");
        
        // MOUSE'U KORUMALI ŞEKİLDE AKTİF TUT!
        StartCoroutine(ProtectMouseAndLoadMenu());
    }

    private System.Collections.IEnumerator ProtectMouseAndLoadMenu()
    {
        Debug.Log("🛡️ Mouse korumalı sahne değiştirme başlıyor...");
        
        // Mouse'u sürekli aktif tut (başka scriptler kapatmasın diye)
        for (int i = 0; i < 10; i++)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log($"🖱️ Mouse koruma döngüsü #{i+1}: Visible={Cursor.visible}, LockState={Cursor.lockState}");
            yield return new WaitForFixedUpdate(); // Her frame mouse'u koru
        }
        
        Debug.Log("✅ Mouse koruması tamamlandı, sahne değiştiriliyor...");
        
        // NetworkManager durumunu kontrol et
        if (NetworkManager.Singleton != null)
        {
            Debug.Log($"🔗 NetworkManager durumu: IsHost={NetworkManager.Singleton.IsHost}, IsClient={NetworkManager.Singleton.IsClient}, IsConnected={NetworkManager.Singleton.IsConnectedClient}");
            
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("🏠 HOST olarak ana menüye dönülüyor...");
                LoadMainMenuDirectly();
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("👤 CLIENT olarak ana menüye dönülüyor...");
                LoadMainMenuDirectly();
            }
            else
            {
                Debug.Log("❓ Network durumu belirsiz, direkt sahne değiştiriliyor...");
                LoadMainMenuDirectly();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ NetworkManager NULL! Direkt sahne değiştiriliyor...");
            LoadMainMenuDirectly();
        }
    }

    private void LoadMainMenuDirectly()
    {
        Debug.Log("🏠 Ana menüye dönülüyor...");
        
        // Mouse'u aktif tut
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Network'ü kapat
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        // Ana menü sahnesine dön
        SceneManager.LoadScene("LobbyBrowserScene");
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

    [ContextMenu("🔧 Test Button Click")]
    public void TestButtonClick()
    {
        Debug.Log("🔧 Manuel button test...");
        OnBackToLobby();
    }

    [ContextMenu("🛡️ Start Mouse Protection")]
    public void StartMouseProtection()
    {
        Debug.Log("🛡️ Manuel mouse koruma başlatılıyor...");
        StartCoroutine(KeepMouseActive());
    }



    [ContextMenu("🖱️ Force Enable Mouse")]
    public void ForceEnableMouse()
    {
        Debug.Log("🖱️ Force enable mouse...");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log($"✅ Mouse zorla aktif edildi: Visible={Cursor.visible}, LockState={Cursor.lockState}");
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