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
        CreateUIIfMissing();
        SetupButtons();
        ResetUI();
    }

    private void CreateUIIfMissing()
    {
        // Canvas oluştur
        if (gameCanvas == null)
        {
            GameObject canvasObj = new GameObject("BombGameCanvas");
            gameCanvas = canvasObj.AddComponent<Canvas>();
            gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameCanvas.sortingOrder = 1000;
            
            // CanvasScaler ekle
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("🎨 BombGameCanvas oluşturuldu");
        }

        // Winner Panel oluştur
        if (winnerPanel == null)
        {
            GameObject panelObj = new GameObject("WinnerPanel");
            panelObj.transform.SetParent(gameCanvas.transform, false);
            
            winnerPanel = panelObj;
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Debug.Log("🏆 WinnerPanel oluşturuldu");
        }

        // Winner Text oluştur
        if (winnerText == null)
        {
            GameObject textObj = new GameObject("WinnerText");
            textObj.transform.SetParent(winnerPanel.transform, false);
            
            winnerText = textObj.AddComponent<Text>();
            winnerText.text = "Kazanan: ";
            winnerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            winnerText.fontSize = 60;
            winnerText.color = Color.white;
            winnerText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.2f, 0.6f);
            textRect.anchorMax = new Vector2(0.8f, 0.8f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Debug.Log("📝 WinnerText oluşturuldu");
        }

        // Main Menu Button oluştur
        if (mainMenuButton == null)
        {
            GameObject buttonObj = new GameObject("MainMenuButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            mainMenuButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
            
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "Ana Menü";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 30;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.2f, 0.3f);
            buttonRect.anchorMax = new Vector2(0.45f, 0.4f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            Debug.Log("🔴 MainMenuButton oluşturuldu");
        }

        // Play Again Button oluştur
        if (playAgainButton == null)
        {
            GameObject buttonObj = new GameObject("PlayAgainButton");
            buttonObj.transform.SetParent(winnerPanel.transform, false);
            
            playAgainButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 0.9f);
            
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "Tekrar Oyna";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 30;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.55f, 0.3f);
            buttonRect.anchorMax = new Vector2(0.8f, 0.4f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            Debug.Log("🔵 PlayAgainButton oluşturuldu");
        }
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
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);
        }

        if (winnerText != null)
        {
            winnerText.text = $"{winnerName} Kazandı!";
        }

        // 3 saniye sonra butonları göster
        Invoke(nameof(ShowButtons), 3f);

        Debug.Log($"🏆 Kazanan gösterildi: {winnerName} (Client {winnerClientId})");
    }

    private void ShowButtons()
    {
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(true);
        
        if (playAgainButton != null)
            playAgainButton.gameObject.SetActive(true);
    }

    public void ResetUI()
    {
        if (winnerPanel != null)
            winnerPanel.SetActive(false);
        
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(false);
        
        if (playAgainButton != null)
            playAgainButton.gameObject.SetActive(false);
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