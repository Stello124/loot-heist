using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

/// <summary>
/// 4.map platform oyunu UI sistemi - 1.map'teki RaceUI'dan uyarlandı
/// </summary>
public class PlatformUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject winnerPanel;

    [Header("UI Texts")]
    public TextMeshProUGUI startText;
    public TextMeshProUGUI winnerText;

    [Header("UI Buttons")]
    public Button playAgainButton;
    public Button backToLobbyButton;

    private void Start()
    {
        CreateUIIfMissing();
        SetupButtons();
        ResetUI();
    }

    private void CreateUIIfMissing()
    {
        // Canvas bulup yoksa oluştur
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("PlatformCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log("✅ Canvas oluşturuldu (4.map)");
        }

        // StartPanel oluştur
        if (startPanel == null)
        {
            startPanel = new GameObject("StartPanel");
            startPanel.transform.SetParent(canvas.transform, false);
            
            // Panel background
            UnityEngine.UI.Image panelImage = startPanel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f); // Yarı saydam siyah
            
            // RectTransform ayarla
            RectTransform panelRect = startPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Debug.Log("✅ StartPanel oluşturuldu (4.map)");
        }

        // StartText oluştur
        if (startText == null)
        {
            GameObject textObj = new GameObject("StartText");
            textObj.transform.SetParent(startPanel.transform, false);
            
            startText = textObj.AddComponent<TextMeshProUGUI>();
            startText.text = "Platform Oyunu Hazırlanıyor...";
            startText.fontSize = 48;
            startText.color = Color.white;
            startText.alignment = TextAlignmentOptions.Center;
            
            // RectTransform ayarla
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Debug.Log("✅ StartText oluşturuldu (4.map)");
        }

        // WinnerPanel oluştur
        if (winnerPanel == null)
        {
            winnerPanel = new GameObject("WinnerPanel");
            winnerPanel.transform.SetParent(canvas.transform, false);
            
            // Panel background
            UnityEngine.UI.Image winnerPanelImage = winnerPanel.AddComponent<UnityEngine.UI.Image>();
            winnerPanelImage.color = new Color(0, 0, 0, 0.9f);
            
            // RectTransform ayarla
            RectTransform winnerRect = winnerPanel.GetComponent<RectTransform>();
            winnerRect.anchorMin = Vector2.zero;
            winnerRect.anchorMax = Vector2.one;
            winnerRect.offsetMin = Vector2.zero;
            winnerRect.offsetMax = Vector2.zero;
            
            Debug.Log("✅ WinnerPanel oluşturuldu (4.map)");
        }

        // WinnerText oluştur
        if (winnerText == null)
        {
            GameObject winnerTextObj = new GameObject("WinnerText");
            winnerTextObj.transform.SetParent(winnerPanel.transform, false);
            
            winnerText = winnerTextObj.AddComponent<TextMeshProUGUI>();
            winnerText.text = "Kazanan Belirleniyor...";
            winnerText.fontSize = 56;
            winnerText.color = Color.yellow;
            winnerText.alignment = TextAlignmentOptions.Center;
            
            // RectTransform ayarla
            RectTransform winnerTextRect = winnerTextObj.GetComponent<RectTransform>();
            winnerTextRect.anchorMin = new Vector2(0, 0.6f);
            winnerTextRect.anchorMax = new Vector2(1, 0.9f);
            winnerTextRect.offsetMin = Vector2.zero;
            winnerTextRect.offsetMax = Vector2.zero;
            
            Debug.Log("✅ WinnerText oluşturuldu (4.map)");
        }

        // PlayAgain Button oluştur
        if (playAgainButton == null)
        {
            GameObject buttonObj = new GameObject("PlayAgainButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            playAgainButton = buttonObj.AddComponent<Button>();
            UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0, 0.7f, 0, 1f); // Yeşil
            
            // Button text
            GameObject buttonTextObj = new GameObject("ButtonText");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Tekrar Oyna";
            buttonText.fontSize = 24;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            // RectTransform ayarları
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.2f, 0.3f);
            buttonRect.anchorMax = new Vector2(0.4f, 0.5f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            Debug.Log("✅ PlayAgainButton oluşturuldu (4.map)");
        }

        // BackToLobby Button oluştur
        if (backToLobbyButton == null)
        {
            GameObject buttonObj = new GameObject("BackToLobbyButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            backToLobbyButton = buttonObj.AddComponent<Button>();
            UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.7f, 0, 0, 1f); // Kırmızı
            
            // Button text
            GameObject buttonTextObj = new GameObject("ButtonText");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Lobby'e Dön";
            buttonText.fontSize = 24;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            // RectTransform ayarları
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.6f, 0.3f);
            buttonRect.anchorMax = new Vector2(0.8f, 0.5f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            Debug.Log("✅ BackToLobbyButton oluşturuldu (4.map)");
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
            startText.text = $"Oyuncular Bekleniyor...\n{joinedCount}/{expectedCount}\n\nPlatform Oyunu";
            startText.fontSize = 48;
            startText.color = Color.white;
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
            startText.text = $"Oyuncular Bekleniyor...\n{joinedCount}/{expectedCount}\n{timeLeft}s\n\nPlatform Oyunu";
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
        }

        if (startText != null)
        {
            startText.text = $"PLATFORM OYUNU BAŞLIYOR!\n{countdownValue}";
            startText.fontSize = 52;
            startText.color = Color.yellow;
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
            startText.text = $"PLATFORM OYUNU BAŞLIYOR!\n{countdownValue}";
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
        }

        if (winnerText != null)
        {
            string winnerName = $"Oyuncu {winnerClientId}";
            
            // Eğer kazanan ben isem
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == winnerClientId)
            {
                winnerText.text = $"🏆 KAZANDIN! 🏆\n\nPlatform Ustası!";
                winnerText.color = Color.green;
            }
            else
            {
                winnerText.text = $"🏆 {winnerName} KAZANDI! 🏆\n\nPlatform Ustası!";
                winnerText.color = Color.yellow;
            }
            
            winnerText.fontSize = 56;
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

    // Debug için
    [ContextMenu("🔍 Debug UI State")]
    public void DebugUIState()
    {
        Debug.Log($"🔍 UI State (4.map):");
        Debug.Log($"  StartPanel: {(startPanel ? (startPanel.activeInHierarchy ? "Active" : "Inactive") : "NULL")}");
        Debug.Log($"  WinnerPanel: {(winnerPanel ? (winnerPanel.activeInHierarchy ? "Active" : "Inactive") : "NULL")}");
        Debug.Log($"  StartText: {(startText ? startText.text : "NULL")}");
        Debug.Log($"  WinnerText: {(winnerText ? winnerText.text : "NULL")}");
    }
}