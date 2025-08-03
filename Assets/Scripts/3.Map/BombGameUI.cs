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

        // Main Menu Button oluştur
        if (mainMenuButton == null)
        {
            GameObject buttonObj = new GameObject("MainMenuButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            // Başlangıçta gizli oluştur
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
            
            Debug.Log($"🔴 MainMenuButton oluşturuldu - Anchor: {buttonRect.anchorMin} to {buttonRect.anchorMax}, Başlangıçta: {buttonObj.activeInHierarchy}");
        }

        // Play Again Button oluştur
        if (playAgainButton == null)
        {
            GameObject buttonObj = new GameObject("PlayAgainButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            // Başlangıçta gizli oluştur
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
            
            Debug.Log($"🔵 PlayAgainButton oluşturuldu - Anchor: {buttonRect.anchorMin} to {buttonRect.anchorMax}, Başlangıçta: {buttonObj.activeInHierarchy}");
        }
        
        // Panel'i başlangıçta gizle (butonlar zaten gizli oluşturuldu)
        if (winnerPanel != null)
            winnerPanel.SetActive(false);
            
        Debug.Log("🔒 Tüm UI componentleri oluşturuldu ve gizlendi (butonlar baştan gizli)");
    }

    private void SetupButtons()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClick);
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(OnPlayAgainClick);
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

        // BUTONLARI ZORUNLU GIZLE (Host'ta otomatik görünüyor sorunu)
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(false);
            Debug.Log("🔒 MainMenuButton ZORUNLU gizlendi");
        }
        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(false);
            Debug.Log("🔒 PlayAgainButton ZORUNLU gizlendi");
        }

        // 3 saniye sonra butonları göster
        Debug.Log("⏰ 3 saniye sonra butonlar gelecek...");
        Invoke(nameof(ShowButtons), 3f);

        Debug.Log($"🏆 Kazanan gösterildi: {winnerName} (Client {winnerClientId})");
    }

    private void ShowButtons()
    {
        Debug.Log("🔘 ShowButtons çağrıldı - 3 saniye geçti");
        
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
            Debug.Log("✅ MainMenuButton 3 saniye sonra aktif edildi");
        }
        else
        {
            Debug.LogError("❌ mainMenuButton null!");
        }
        
        if (playAgainButton != null)
        {
            playAgainButton.gameObject.SetActive(true);
            Debug.Log("✅ PlayAgainButton 3 saniye sonra aktif edildi");
        }
        else
        {
            Debug.LogError("❌ playAgainButton null!");
        }
        
        Debug.Log("🔘 ShowButtons tamamlandı - Her iki buton da aktif");
    }

    public void ResetUI()
    {
        Debug.Log("🔄 UI Reset - Tüm paneller kapatılıyor");
        
        if (gameCanvas != null)
            gameCanvas.gameObject.SetActive(false);
        
        if (winnerPanel != null)
            winnerPanel.SetActive(false);
        
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(false);
        
        if (playAgainButton != null)
            playAgainButton.gameObject.SetActive(false);
            
        Debug.Log("✅ UI Reset tamamlandı - Canvas kapalı");
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