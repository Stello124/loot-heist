using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class RaceUI : MonoBehaviour
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
            GameObject canvasObj = new GameObject("RaceCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log("✅ Canvas oluşturuldu");
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
            
            Debug.Log("✅ StartPanel oluşturuldu");
        }

        // StartText oluştur
        if (startText == null)
        {
            GameObject textObj = new GameObject("StartText");
            textObj.transform.SetParent(startPanel.transform, false);
            
            startText = textObj.AddComponent<TextMeshProUGUI>();
            startText.text = "Oyun Hazırlanıyor...";
            startText.fontSize = 48;
            startText.color = Color.white;
            startText.alignment = TextAlignmentOptions.Center;
            
            // RectTransform ayarla
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Debug.Log("✅ StartText oluşturuldu");
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
            
            winnerPanel.SetActive(false);
            Debug.Log("✅ WinnerPanel oluşturuldu");
        }

        // WinnerText oluştur
        if (winnerText == null)
        {
            GameObject winnerTextObj = new GameObject("WinnerText");
            winnerTextObj.transform.SetParent(winnerPanel.transform, false);
            
            winnerText = winnerTextObj.AddComponent<TextMeshProUGUI>();
            winnerText.text = "Kazanan!";
            winnerText.fontSize = 64;
            winnerText.color = Color.yellow;
            winnerText.alignment = TextAlignmentOptions.Center;
            
            // RectTransform ayarla
            RectTransform winnerTextRect = winnerTextObj.GetComponent<RectTransform>();
            winnerTextRect.anchorMin = new Vector2(0, 0.6f);
            winnerTextRect.anchorMax = new Vector2(1, 0.9f);
            winnerTextRect.offsetMin = Vector2.zero;
            winnerTextRect.offsetMax = Vector2.zero;
            
            Debug.Log("✅ WinnerText oluşturuldu");
        }

        // Butonları oluştur
        CreateButtons();
    }

    private void CreateButtons()
    {
        if (playAgainButton == null && winnerPanel != null)
        {
            GameObject buttonObj = new GameObject("PlayAgainButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            playAgainButton = buttonObj.AddComponent<Button>();
            UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = Color.green;
            
            // Button text
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Tekrar Oyna";
            buttonText.fontSize = 24;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            // RectTransform'lar
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.2f, 0.2f);
            buttonRect.anchorMax = new Vector2(0.4f, 0.4f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            Debug.Log("✅ PlayAgainButton oluşturuldu");
        }

        if (backToLobbyButton == null && winnerPanel != null)
        {
            GameObject buttonObj = new GameObject("BackToLobbyButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            backToLobbyButton = buttonObj.AddComponent<Button>();
            UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = Color.red;
            
            // Button text
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Lobbye Dön";
            buttonText.fontSize = 24;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            // RectTransform'lar
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.6f, 0.2f);
            buttonRect.anchorMax = new Vector2(0.8f, 0.4f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            Debug.Log("✅ BackToLobbyButton oluşturuldu");
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
        Debug.Log($"📺 ShowWaitingForPlayers çağrıldı: {joinedCount}/{expectedCount}");
        
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            Debug.Log($"✅ StartPanel aktif edildi - Active: {startPanel.activeInHierarchy}");
            
            // Panel'in RectTransform'unu kontrol et
            RectTransform rectTransform = startPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(800, 400);
                Debug.Log($"✅ Panel pozisyon ayarlandı: {rectTransform.anchoredPosition}, boyut: {rectTransform.sizeDelta}");
            }
        }
        else
        {
            Debug.LogError("❌ StartPanel null!");
        }

        if (startText != null)
        {
            startText.text = $"Oyuncular Bekleniyor...\n{joinedCount}/{expectedCount}";
            startText.fontSize = 48;
            startText.color = Color.white;
            Debug.Log($"✅ StartText güncellendi: {startText.text}, Font: {startText.fontSize}");
        }
        else
        {
            Debug.LogError("❌ StartText null!");
        }
        
        // Diğer panelleri gizle
        if (winnerPanel != null) winnerPanel.SetActive(false);
    }

    public void UpdateWaitingTimer(int timeLeft, int joinedCount, int expectedCount)
    {
        if (startText != null)
        {
            startText.text = $"Oyuncular Bekleniyor...\n{joinedCount}/{expectedCount}\n{timeLeft}s";
            Debug.Log($"⏰ Waiting timer güncellendi: {timeLeft}s");
        }
    }

    public void ShowCountdown(int countdownValue)
    {
        Debug.Log($"📺 ShowCountdown çağrıldı: {countdownValue}");
        
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            Debug.Log($"✅ StartPanel countdown için aktif - Active: {startPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("❌ StartPanel null!");
        }

        if (startText != null)
        {
            startText.text = $"YARIŞ BAŞLIYOR!\n{countdownValue}";
            startText.fontSize = 48;
            startText.color = Color.yellow;
            Debug.Log($"✅ Countdown text güncellendi: {startText.text}, Font: {startText.fontSize}");
        }
        else
        {
            Debug.LogError("❌ StartText null!");
        }
    }

    public void UpdateCountdown(int countdownValue)
    {
        if (startText != null)
        {
            startText.text = $"YARIŞ BAŞLIYOR!\n{countdownValue}";
            Debug.Log($"⏰ Countdown güncellendi: {countdownValue}");
        }
    }

    public void HideStartPanels()
    {
        Debug.Log("📺 HideStartPanels çağrıldı");
        
        if (startPanel != null)
        {
            startPanel.SetActive(false);
            Debug.Log("✅ StartPanel gizlendi");
        }
    }

    public void ShowWinner(string winnerName, ulong winnerClientId)
    {
        Debug.Log($"🏆 ShowWinner çağrıldı: {winnerName} (Client {winnerClientId})");
        
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);
            Debug.Log("✅ WinnerPanel gösterildi");
            
            // Butonları başlangıçta gizle
            SetButtonsVisible(false);
            
            // 3 saniye sonra butonları göster
            Invoke(nameof(ShowButtons), 3f);
        }
        else
        {
            Debug.LogError("❌ WinnerPanel null!");
        }

        if (winnerText != null)
        {
            winnerText.text = $"{winnerName} kazandı!!";
            Debug.Log($"✅ WinnerText güncellendi: {winnerText.text}");
        }
        else
        {
            Debug.LogError("❌ WinnerText null!");
        }

        // Start panellerini gizle
        HideStartPanels();
    }

    private void ShowButtons()
    {
        SetButtonsVisible(true);
        Debug.Log("✅ Butonlar 3 saniye sonra gösterildi");
    }

    private void SetButtonsVisible(bool visible)
    {
        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(visible);
        }
        if (backToLobbyButton != null)
        {
            backToLobbyButton.gameObject.SetActive(visible);
        }
        Debug.Log($"🔘 Butonlar {(visible ? "gösterildi" : "gizlendi")}");
    }

    public void ResetUI()
    {
        Debug.Log("🔄 UI Reset edildi");
        
        if (startPanel != null) startPanel.SetActive(false);
        if (winnerPanel != null) winnerPanel.SetActive(false);
        
        SetButtonsVisible(true);
    }

    private void OnPlayAgain()
    {
        Debug.Log("🔄 Play Again tıklandı");
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("1.map", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void OnBackToLobby()
    {
        Debug.Log("🏠 Back to Lobby tıklandı");
        
        // Lobby state'ini temizle
        ClearLobbyState();
        
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.Shutdown();
            }
            else
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
        SceneManager.LoadScene("LobbyBrowserScene");
    }
    
    // Lobby state'ini temizle
    private void ClearLobbyState()
    {
        Debug.Log("🧹 Lobby state temizleniyor...");
        
        // CurrentLobby component'ini bul ve temizle
        CurrentLobby currentLobby = FindObjectOfType<CurrentLobby>();
        if (currentLobby != null)
        {
            currentLobby.currentLobby = null;
            Debug.Log("✅ CurrentLobby temizlendi");
        }
        
        // LobbyRoomUI component'ini bul ve temizle
        LobbyRoomUI lobbyRoomUI = FindObjectOfType<LobbyRoomUI>();
        if (lobbyRoomUI != null)
        {
            // LobbyRoomUI'deki lobby referanslarını temizle
            var lobbyRoomUIField = lobbyRoomUI.GetType().GetField("_currentLobby", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (lobbyRoomUIField != null)
            {
                lobbyRoomUIField.SetValue(lobbyRoomUI, null);
                Debug.Log("✅ LobbyRoomUI _currentLobby temizlendi");
            }
            
            var lobbyIdField = lobbyRoomUI.GetType().GetField("lobbyId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (lobbyIdField != null)
            {
                lobbyIdField.SetValue(lobbyRoomUI, null);
                Debug.Log("✅ LobbyRoomUI lobbyId temizlendi");
            }
        }
        
        // LobbyData Instance'ını temizle (eğer varsa)
        if (LobbyData.Instance != null)
        {
            // LobbyData'nın static instance'ını temizle
            var instanceField = typeof(LobbyData).GetField("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceField != null)
            {
                instanceField.SetValue(null, null);
                Debug.Log("✅ LobbyData Instance temizlendi");
            }
        }
        
        Debug.Log("🧹 Lobby state temizleme tamamlandı - Yeni lobby için hazır");
    }
}