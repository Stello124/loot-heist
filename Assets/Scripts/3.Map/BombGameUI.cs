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
    
    // Yeni butonlar
    private Button newMainMenuButton;
    private Button newPlayAgainButton;

    void Start()
    {
        Debug.Log("🎨 BombGameUI Start çağrıldı - Hiç görünmemeli başlangıçta");
        CreateUIInactive(); // Baştan gizli oluştur
        SetupButtons();
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

        // Eski Main Menu Button'u tamamen gizle
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(false);
            Debug.Log("🔒 Eski MainMenuButton tamamen gizlendi");
        }
        
        // YENİ Main Menu Button oluştur
        if (newMainMenuButton == null)
        {
            GameObject buttonObj = new GameObject("NewMainMenuButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            // Başlangıçta gizli oluştur
            buttonObj.SetActive(false);
            
            newMainMenuButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.9f, 0.1f, 0.1f, 1f); // Kırmızı
            
            // Button hover efekti
            ColorBlock colors = newMainMenuButton.colors;
            colors.normalColor = new Color(0.9f, 0.1f, 0.1f, 1f);
            colors.highlightedColor = new Color(1f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.7f, 0.05f, 0.05f, 1f);
            newMainMenuButton.colors = colors;
            
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "Ana Menü";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 50;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.1f, 0.1f);
            buttonRect.anchorMax = new Vector2(0.4f, 0.2f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            Debug.Log($"🔴 YENİ MainMenuButton oluşturuldu - Anchor: {buttonRect.anchorMin} to {buttonRect.anchorMax}");
        }

        // Eski Play Again Button'u tamamen gizle
        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(false);
            Debug.Log("🔒 Eski PlayAgainButton tamamen gizlendi");
        }
        
        // YENİ Play Again Button oluştur
        if (newPlayAgainButton == null)
        {
            GameObject buttonObj = new GameObject("NewPlayAgainButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            // Başlangıçta gizli oluştur
            buttonObj.SetActive(false);
            
            newPlayAgainButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.1f, 0.9f, 0.1f, 1f); // Yeşil
            
            // Button hover efekti
            ColorBlock colors = newPlayAgainButton.colors;
            colors.normalColor = new Color(0.1f, 0.9f, 0.1f, 1f);
            colors.highlightedColor = new Color(0.3f, 1f, 0.3f, 1f);
            colors.pressedColor = new Color(0.05f, 0.7f, 0.05f, 1f);
            newPlayAgainButton.colors = colors;
            
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "Tekrar Oyna";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 50;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.6f, 0.1f);
            buttonRect.anchorMax = new Vector2(0.9f, 0.2f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            Debug.Log($"🔵 YENİ PlayAgainButton oluşturuldu - Anchor: {buttonRect.anchorMin} to {buttonRect.anchorMax}");
        }
        
        // Panel'i başlangıçta gizle (butonlar zaten gizli oluşturuldu)
        if (winnerPanel != null)
            winnerPanel.SetActive(false);
            
        Debug.Log("🔒 Tüm UI componentleri oluşturuldu ve gizlendi (butonlar baştan gizli)");
    }

    private void SetupButtons()
    {
        // Eski butonları devre dışı bırak
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(false);
        }

        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(false);
        }
        
        // Yeni butonları ayarla
        if (newMainMenuButton != null)
        {
            newMainMenuButton.onClick.RemoveAllListeners();
            newMainMenuButton.onClick.AddListener(OnMainMenuClick);
            Debug.Log("🔴 Yeni MainMenuButton listener eklendi");
        }

        if (newPlayAgainButton != null)
        {
            newPlayAgainButton.onClick.RemoveAllListeners();
            newPlayAgainButton.onClick.AddListener(OnPlayAgainClick);
            Debug.Log("🔵 Yeni PlayAgainButton listener eklendi");
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
            
            // Panel aktif olduğunda butonları tekrar gizle (Host sorunu için)
            if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);
            if (playAgainButton != null) playAgainButton.gameObject.SetActive(false);
            Debug.Log("🔒 Panel aktif olduğunda butonlar tekrar gizlendi");
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

        // ESKİ BUTONLARI ZORUNLU GIZLE
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(false);
            Debug.Log("🔒 Eski MainMenuButton ZORUNLU gizlendi");
        }
        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(false);
            Debug.Log("🔒 Eski PlayAgainButton ZORUNLU gizlendi");
        }
        
        // YENİ BUTONLARI DA GIZLE (3 saniye sonra açılacak)
        if (newMainMenuButton != null)
        {
            newMainMenuButton.gameObject.SetActive(false);
            Debug.Log("🔒 Yeni MainMenuButton gizlendi");
        }
        if (newPlayAgainButton != null)
        {
            newPlayAgainButton.gameObject.SetActive(false);
            Debug.Log("🔒 Yeni PlayAgainButton gizlendi");
        }

        // 3 saniye sonra butonları göster
        Debug.Log("⏰ 3 saniye sonra butonlar gelecek...");
        Invoke(nameof(ShowButtons), 3f);

        Debug.Log($"🏆 Kazanan gösterildi: {winnerName} (Client {winnerClientId})");
    }

    private void ShowButtons()
    {
        Debug.Log("🔘 ShowButtons çağrıldı - 3 saniye geçti");
        
        // YENİ BUTONLARI GÖSTER
        if (newMainMenuButton != null)
        {
            newMainMenuButton.gameObject.SetActive(true);
            Debug.Log("✅ YENİ MainMenuButton 3 saniye sonra aktif edildi");
        }
        else
        {
            Debug.LogError("❌ newMainMenuButton null!");
        }
        
        if (newPlayAgainButton != null)
        {
            newPlayAgainButton.gameObject.SetActive(true);
            Debug.Log("✅ YENİ PlayAgainButton 3 saniye sonra aktif edildi");
        }
        else
        {
            Debug.LogError("❌ newPlayAgainButton null!");
        }
        
        Debug.Log("🔘 ShowButtons tamamlandı - YENİ butonlar aktif");
    }

    public void ResetUI()
    {
        Debug.Log("🔄 UI Reset - Tüm paneller kapatılıyor");
        
        if (gameCanvas != null)
            gameCanvas.gameObject.SetActive(false);
        
        if (winnerPanel != null)
            winnerPanel.SetActive(false);
        
        // Eski butonları gizle
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(false);
        
        if (playAgainButton != null)
            playAgainButton.gameObject.SetActive(false);
            
        // Yeni butonları gizle
        if (newMainMenuButton != null)
            newMainMenuButton.gameObject.SetActive(false);
        
        if (newPlayAgainButton != null)
            newPlayAgainButton.gameObject.SetActive(false);
            
        Debug.Log("✅ UI Reset tamamlandı - Canvas ve tüm butonlar kapalı");
    }

    private void OnMainMenuClick()
    {
        Debug.Log("🏠 Ana menüye dönülüyor...");
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyBrowserScene");
    }

    private void OnPlayAgainClick()
    {
        Debug.Log("🔄 Tekrar oynanacak...");
        
        // Sadece host restart edebilir
        if (NetworkManager.Singleton.IsHost)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("3.map");
        }
    }
}