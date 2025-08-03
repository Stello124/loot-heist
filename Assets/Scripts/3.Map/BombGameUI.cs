using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class BombGameUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas gameCanvas;
    public GameObject winnerPanel;
    public Text winnerText;
    public Button mainMenuButton;
    public Button playAgainButton;
    
    // YENİ BUTONLAR - Gizli tutulacak
    private Button newMainMenuButton;
    private Button newPlayAgainButton;

    void Start()
    {
        Debug.Log("🎨 BombGameUI Start çağrıldı - Hiç görünmemeli başlangıçta");
        CreateUIInactive(); // Baştan gizli oluştur
        SetupButtons();
        CreateNewButtons(); // Yeni butonları oluştur
        Debug.Log("🎨 BombGameUI Start tamamlandı - Tamamen gizli");
    }

    private void CreateUIInactive()
    {
        // Canvas oluştur ama HEMEN gizle
        if (gameCanvas == null)
        {
            // Mevcut Canvas'ı ara
            gameCanvas = FindObjectOfType<Canvas>();
            
            if (gameCanvas == null)
            {
                GameObject canvasObj = new GameObject("BombGameCanvas");
                gameCanvas = canvasObj.AddComponent<Canvas>();
                gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameCanvas.sortingOrder = 1000;
                
                // CanvasScaler ekle - Host/Client uyumluluk için
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f; // Genişlik ve yükseklik arası denge
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // HEMEN GİZLE - hiç görünmesin
                canvasObj.SetActive(false);
                
                Debug.Log("🎨 BombGameCanvas YENİ oluşturuldu ve HEMEN gizlendi");
            }
            else
            {
                // Mevcut Canvas'ı da gizle
                gameCanvas.gameObject.SetActive(false);
                Debug.Log("🎨 Mevcut Canvas bulundu ve gizlendi");
            }
        }
        
        CreateUIComponents(); // UI'yi oluştur ama gizli
    }

    private void CreateUIComponents()
    {
        // Canvas artık mevcut, UI componentlerini oluştur

        // Winner Panel oluştur
        if (winnerPanel == null)
        {
            GameObject panelObj = new GameObject("WinnerPanel");
            panelObj.transform.SetParent(gameCanvas.transform, false);
            
            winnerPanel = panelObj;
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.9f); // Daha koyu yap
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Panel'i en üstte tut
            panelObj.transform.SetAsLastSibling();
            
            Debug.Log("🏆 WinnerPanel oluşturuldu - Koyu arkaplan");
        }

        // Winner Text oluştur
        if (winnerText == null)
        {
            GameObject textObj = new GameObject("WinnerText");
            textObj.transform.SetParent(winnerPanel.transform, false);
            
            winnerText = textObj.AddComponent<Text>();
            winnerText.text = ""; // Başlangıçta boş
            winnerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            winnerText.fontSize = 80; // Daha büyük
            winnerText.color = Color.yellow; // Sarı renk
            winnerText.alignment = TextAnchor.MiddleCenter;
            winnerText.fontStyle = FontStyle.Bold; // Kalın
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.4f); // Daha geniş alan
            textRect.anchorMax = new Vector2(0.9f, 0.7f); // Daha geniş alan
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Debug.Log("📝 WinnerText oluşturuldu - Büyük sarı text");
        }

        // Main Menu Button oluştur - GİZLİ TUTULACAK
        if (mainMenuButton == null)
        {
            GameObject buttonObj = new GameObject("MainMenuButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            // BAŞTAN GİZLİ OLUŞTUR
            buttonObj.SetActive(false);
            
            mainMenuButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
            
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "Ana Menü";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 40; // Daha büyük yazı
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.15f, 0.15f); // Daha büyük alan
            buttonRect.anchorMax = new Vector2(0.45f, 0.25f); // Daha büyük alan
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            Debug.Log($"🔴 MainMenuButton oluşturuldu - GİZLİ TUTULACAK");
        }

        // Play Again Button oluştur - GİZLİ TUTULACAK
        if (playAgainButton == null)
        {
            GameObject buttonObj = new GameObject("PlayAgainButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            // BAŞTAN GİZLİ OLUŞTUR
            buttonObj.SetActive(false);
            
            playAgainButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 0.9f);
            
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "Tekrar Oyna";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 40; // Daha büyük yazı
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.55f, 0.15f); // Daha büyük alan
            buttonRect.anchorMax = new Vector2(0.85f, 0.25f); // Daha büyük alan
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            Debug.Log($"🔵 PlayAgainButton oluşturuldu - GİZLİ TUTULACAK");
        }
        
        // Panel'i başlangıçta gizle (butonlar zaten gizli oluşturuldu)
        if (winnerPanel != null)
            winnerPanel.SetActive(false);
            
        Debug.Log("🔒 Tüm UI componentleri oluşturuldu ve gizlendi (eski butonlar baştan gizli)");
    }

    private void CreateNewButtons()
    {
        // YENİ KIRMIZI BUTON - "oyunu kapatmak için h ye bas"
        GameObject redButtonObj = new GameObject("NewRedButton");
        redButtonObj.transform.SetParent(winnerPanel.transform, false);
        
        // Başlangıçta gizli oluştur
        redButtonObj.SetActive(false);
        
        newMainMenuButton = redButtonObj.AddComponent<Button>();
        Image redButtonImage = redButtonObj.AddComponent<Image>();
        redButtonImage.color = new Color(0.9f, 0.1f, 0.1f, 0.9f); // Kırmızı
        
        GameObject redButtonTextObj = new GameObject("Text");
        redButtonTextObj.transform.SetParent(redButtonObj.transform, false);
        Text redButtonText = redButtonTextObj.AddComponent<Text>();
        redButtonText.text = "oyunu kapatmak için h ye bas";
        redButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        redButtonText.fontSize = 35; // Büyük yazı
        redButtonText.color = Color.white;
        redButtonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform redButtonRect = redButtonObj.GetComponent<RectTransform>();
        redButtonRect.anchorMin = new Vector2(0.1f, 0.15f); // Sol taraf
        redButtonRect.anchorMax = new Vector2(0.45f, 0.25f);
        redButtonRect.offsetMin = Vector2.zero;
        redButtonRect.offsetMax = Vector2.zero;
        
        RectTransform redButtonTextRect = redButtonTextObj.GetComponent<RectTransform>();
        redButtonTextRect.anchorMin = Vector2.zero;
        redButtonTextRect.anchorMax = Vector2.one;
        redButtonTextRect.offsetMin = Vector2.zero;
        redButtonTextRect.offsetMax = Vector2.zero;
        
        // Hover efekti ekle
        ColorBlock colors = newMainMenuButton.colors;
        colors.normalColor = new Color(0.9f, 0.1f, 0.1f, 0.9f);
        colors.highlightedColor = new Color(1f, 0.2f, 0.2f, 1f);
        colors.pressedColor = new Color(0.7f, 0.05f, 0.05f, 1f);
        newMainMenuButton.colors = colors;
        
        Debug.Log("🔴 Yeni kırmızı buton oluşturuldu - 'oyunu kapatmak için h ye bas'");
        
        // YENİ YEŞİL BUTON - "menüye dönmek için K ye bas"
        GameObject greenButtonObj = new GameObject("NewGreenButton");
        greenButtonObj.transform.SetParent(winnerPanel.transform, false);
        
        // Başlangıçta gizli oluştur
        greenButtonObj.SetActive(false);
        
        newPlayAgainButton = greenButtonObj.AddComponent<Button>();
        Image greenButtonImage = greenButtonObj.AddComponent<Image>();
        greenButtonImage.color = new Color(0.1f, 0.9f, 0.1f, 0.9f); // Yeşil
        
        GameObject greenButtonTextObj = new GameObject("Text");
        greenButtonTextObj.transform.SetParent(greenButtonObj.transform, false);
        Text greenButtonText = greenButtonTextObj.AddComponent<Text>();
        greenButtonText.text = "menüye dönmek için K ye bas";
        greenButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        greenButtonText.fontSize = 35; // Büyük yazı
        greenButtonText.color = Color.white;
        greenButtonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform greenButtonRect = greenButtonObj.GetComponent<RectTransform>();
        greenButtonRect.anchorMin = new Vector2(0.55f, 0.15f); // Sağ taraf
        greenButtonRect.anchorMax = new Vector2(0.9f, 0.25f);
        greenButtonRect.offsetMin = Vector2.zero;
        greenButtonRect.offsetMax = Vector2.zero;
        
        RectTransform greenButtonTextRect = greenButtonTextObj.GetComponent<RectTransform>();
        greenButtonTextRect.anchorMin = Vector2.zero;
        greenButtonTextRect.anchorMax = Vector2.one;
        greenButtonTextRect.offsetMin = Vector2.zero;
        greenButtonTextRect.offsetMax = Vector2.zero;
        
        // Hover efekti ekle
        ColorBlock greenColors = newPlayAgainButton.colors;
        greenColors.normalColor = new Color(0.1f, 0.9f, 0.1f, 0.9f);
        greenColors.highlightedColor = new Color(0.2f, 1f, 0.2f, 1f);
        greenColors.pressedColor = new Color(0.05f, 0.7f, 0.05f, 1f);
        newPlayAgainButton.colors = greenColors;
        
        Debug.Log("🟢 Yeni yeşil buton oluşturuldu - 'menüye dönmek için K ye bas'");
    }

    private void SetupButtons()
    {
        // ESKİ BUTONLAR - Hiç kullanılmayacak, sadece gizli tutulacak
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            // Eski buton hiç çalışmasın
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            // Eski buton hiç çalışmasın
        }
        
        // YENİ BUTONLAR - Gerçek işlevsellik
        if (newMainMenuButton != null)
        {
            newMainMenuButton.onClick.RemoveAllListeners();
            newMainMenuButton.onClick.AddListener(OnNewMainMenuClick);
        }

        if (newPlayAgainButton != null)
        {
            newPlayAgainButton.onClick.RemoveAllListeners();
            newPlayAgainButton.onClick.AddListener(OnNewPlayAgainClick);
        }
    }

    public void ShowWinner(string winnerName, ulong winnerClientId)
    {
        Debug.Log($"🏆 ShowWinner çağrıldı: {winnerName}");
        
        // Eğer UI oluşturulmamışsa oluştur
        if (gameCanvas == null || winnerPanel == null)
        {
            Debug.Log("⚠️ UI componentleri eksik, oluşturuluyor...");
            CreateUIInactive();
            CreateNewButtons();
        }
        
        // Canvas'ı aktif et
        if (gameCanvas != null)
        {
            gameCanvas.gameObject.SetActive(true);
            
            // Canvas scaling debug
            CanvasScaler scaler = gameCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                Debug.Log($"✅ Canvas aktif - Scaler: {scaler.uiScaleMode}, Ref: {scaler.referenceResolution}");
            }
            else
            {
                Debug.Log("✅ Canvas aktif edildi - Scaler yok");
            }
        }
        else
        {
            Debug.LogError("❌ gameCanvas null!");
        }

        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);
            Debug.Log("✅ WinnerPanel aktif edildi");
            
            // ESKİ BUTONLARI ZORUNLU GIZLE
            if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);
            if (playAgainButton != null) playAgainButton.gameObject.SetActive(false);
            Debug.Log("🔒 Eski butonlar zorunlu gizlendi");
        }
        else
        {
            Debug.LogError("❌ winnerPanel null!");
        }

        if (winnerText != null)
        {
            winnerText.text = $"{winnerName} Kazandı!";
            Debug.Log($"✅ Winner text set: {winnerName} Kazandı!");
        }
        else
        {
            Debug.LogError("❌ winnerText null!");
        }

        // YENİ BUTONLARI ZORUNLU GIZLE (başlangıçta)
        if (newMainMenuButton != null)
        {
            newMainMenuButton.gameObject.SetActive(false);
            Debug.Log("🔒 Yeni kırmızı buton zorunlu gizlendi");
        }
        if (newPlayAgainButton != null)
        {
            newPlayAgainButton.gameObject.SetActive(false);
            Debug.Log("🔒 Yeni yeşil buton zorunlu gizlendi");
        }

        // 3 saniye sonra YENİ butonları göster
        Debug.Log("⏰ 3 saniye sonra YENİ butonlar gelecek...");
        Invoke(nameof(ShowNewButtons), 3f);

        Debug.Log($"🏆 Kazanan gösterildi: {winnerName} (Client {winnerClientId})");
    }

    private void ShowNewButtons()
    {
        Debug.Log("🔘 ShowNewButtons çağrıldı - 3 saniye geçti");
        
        if (newMainMenuButton != null)
        {
            newMainMenuButton.gameObject.SetActive(true);
            Debug.Log("✅ Yeni kırmızı buton 3 saniye sonra aktif edildi");
        }
        else
        {
            Debug.LogError("❌ newMainMenuButton null!");
        }
        
        if (newPlayAgainButton != null)
        {
            newPlayAgainButton.gameObject.SetActive(true);
            Debug.Log("✅ Yeni yeşil buton 3 saniye sonra aktif edildi");
        }
        else
        {
            Debug.LogError("❌ newPlayAgainButton null!");
        }
        
        Debug.Log("🔘 ShowNewButtons tamamlandı - Her iki YENİ buton da aktif");
    }

    public void ResetUI()
    {
        Debug.Log("🔄 UI Reset - Tüm paneller kapatılıyor");
        
        if (gameCanvas != null)
            gameCanvas.gameObject.SetActive(false);
        
        if (winnerPanel != null)
            winnerPanel.SetActive(false);
        
        // ESKİ BUTONLARI GIZLE
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(false);
        
        if (playAgainButton != null)
            playAgainButton.gameObject.SetActive(false);
            
        // YENİ BUTONLARI GIZLE
        if (newMainMenuButton != null)
            newMainMenuButton.gameObject.SetActive(false);
        
        if (newPlayAgainButton != null)
            newPlayAgainButton.gameObject.SetActive(false);
            
        Debug.Log("✅ UI Reset tamamlandı - Canvas kapalı");
    }

    // ESKİ BUTON FONKSİYONLARI - Hiç kullanılmayacak
    private void OnMainMenuClick()
    {
        Debug.Log("🚫 Eski buton fonksiyonu - Hiç çalışmamalı");
    }

    private void OnPlayAgainClick()
    {
        Debug.Log("🚫 Eski buton fonksiyonu - Hiç çalışmamalı");
    }
    
    // YENİ BUTON FONKSİYONLARI - Gerçek işlevsellik
    private void OnNewMainMenuClick()
    {
        Debug.Log("🏠 YENİ buton: Ana menüye dönülüyor...");
        
        // Lobby state'ini temizle
        ClearLobbyState();
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyBrowserScene");
    }

    private void OnNewPlayAgainClick()
    {
        Debug.Log("🔄 YENİ buton: Tekrar oynanacak...");
        
        // Sadece host restart edebilir
        if (NetworkManager.Singleton.IsHost)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("3.map");
        }
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