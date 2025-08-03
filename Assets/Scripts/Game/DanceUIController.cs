using UnityEngine;
using UnityEngine.UI;

public class DanceUIController : MonoBehaviour
{
    [Header("🎯 Basit Dairesel Dans Menüsü")]
    public GameObject dancePanel;
    public Button[] danceButtons = new Button[4];
    
    private bool isMenuOpen = false;
    
    void Start()
    {
        // lobbyroom hariç her yerde çalış
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (currentScene == "lobbyroom")
        {
            Debug.Log($"🚫 Dans menüsü devre dışı: {currentScene}");
            gameObject.SetActive(false);
            return;
        }
        
        Debug.Log($"✅ Dans menüsü aktif: {currentScene}");
        CreateDanceMenu();
        HideMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"🔥 T tuşuna basıldı! Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            Debug.Log($"🔥 isMenuOpen: {isMenuOpen}, dancePanel: {(dancePanel != null ? "VAR" : "YOK")}");
            
            if (isMenuOpen)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
        }
    }

    void CreateDanceMenu()
    {
        Debug.Log("🔥 CreateDanceMenu çağrıldı!");
        
        // Ayrı Canvas ve Panel oluştur
        if (dancePanel == null)
        {
            Debug.Log("🔥 Canvas oluşturuluyor...");
            // Her zaman ayrı Canvas oluştur
            GameObject canvasObj = new GameObject("DanceCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // En üstte gözüksün
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("🎨 Ayrı Dans Canvas'ı oluşturuldu");
            
            Debug.Log("🔥 Panel oluşturuluyor...");
            dancePanel = new GameObject("DancePanel");
            dancePanel.transform.SetParent(canvasObj.transform, false);
            
            RectTransform panelRect = dancePanel.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(250, 250);
            
            // Ekran ortasında konumlandır
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            
            Debug.Log($"🔥 Panel oluşturuldu: {dancePanel.name}");
        }
        else
        {
            Debug.Log("🔥 dancePanel zaten var!");
        }
        
        // 4 tane button oluştur
        for (int i = 0; i < 4; i++)
        {
            if (danceButtons[i] == null)
            {
                CreateDanceButton(i);
            }
        }
        
        Debug.Log("✅ Basit dans menüsü oluşturuldu");
    }
    
    void CreateDanceButton(int index)
    {
        Debug.Log($"🔥 CreateDanceButton çağrıldı: Index {index}");
        
        // Button oluştur
        GameObject buttonObj = new GameObject($"DanceButton_{index}");
        buttonObj.transform.SetParent(dancePanel.transform, false);
        
        // Button component
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.25f, 0.6f, 0.9f);
        
        // Button hover colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.25f, 0.6f, 0.9f);
        colors.highlightedColor = new Color(0.25f, 0.35f, 0.8f, 1f);
        colors.pressedColor = new Color(0.1f, 0.2f, 0.5f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        button.colors = colors;
        
        // Text ekle
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = GetDanceName(index);
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontStyle = FontStyle.Bold;
        
        // Text RectTransform
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Button pozisyonu (dairesel)
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(90, 90);
        
        // 90 derece arayla, 12:00'dan başlayarak saat yönünde
        float angle = ((index * 90f) - 90f) * Mathf.Deg2Rad; // -90 ile 12:00'dan başlar
        float radius = 110f;
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        buttonRect.anchoredPosition = new Vector2(x, y);
        
        // Click event - DEBUG EKLİ
        int buttonIndex = index;
        Debug.Log($"🔥 Button {index} için onClick event ekleniyor");
        
        button.onClick.AddListener(() => {
            Debug.Log($"🔥🔥🔥 BUTTON {buttonIndex} TIKLANDI! TEST TEST TEST!");
            Debug.Log($"🔥🔥🔥 CLICK EVENT ÇALIŞIYOR!");
            OnDanceButtonClick(buttonIndex);
        });
        
        // Manual test click event
        Debug.Log($"🔥 Button {buttonIndex} click event test ediliyor...");
        button.onClick.Invoke();
        Debug.Log($"🔥 Button {buttonIndex} manual invoke tamamlandı");
        
        // Button'un interactable olduğundan emin ol
        button.interactable = true;
        
        danceButtons[index] = button;
        
        Debug.Log($"✅ Button {index} oluşturuldu ve event atandı");
    }
    

    
    void ShowMenu()
    {
        Debug.Log("🔥 ShowMenu çağrıldı!");
        
        if (dancePanel == null) 
        {
            Debug.Log("🔥 dancePanel null, CreateDanceMenu çağrılıyor");
            CreateDanceMenu();
        }
        
        if (dancePanel == null)
        {
            Debug.LogError("🚨 CreateDanceMenu sonrası hala dancePanel null!");
            return;
        }
        
        // Ekran ortasında göster
        RectTransform panelRect = dancePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero; // Tam ortada
        
        dancePanel.SetActive(true);
        isMenuOpen = true;
        
        // Mouse'u serbest bırak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Dans isimlerini güncelle
        UpdateDanceNames();
        
        Debug.Log("🖱️ Dans menüsü ekran ortasında açıldı");
    }
    
    void HideMenu()
    {
        if (dancePanel != null)
        {
            dancePanel.SetActive(false);
        }
        
        isMenuOpen = false;
        
        // Mouse'u kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("🖱️ Dans menüsü kapandı");
    }
    
    void UpdateDanceNames()
    {
        Debug.Log("🔥 UpdateDanceNames çağrıldı");
        
        for (int i = 0; i < danceButtons.Length; i++)
        {
            if (danceButtons[i] != null)
            {
                Text buttonText = danceButtons[i].GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    string danceName = GetDanceName(i);
                    buttonText.text = danceName;
                    Debug.Log($"🔥 Button {i} text güncellendi: '{danceName}'");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Button {i}'da Text component bulunamadı!");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ danceButtons[{i}] null!");
            }
        }
    }
    
    private string GetDanceName(int index)
    {
        // Cloud Save'den dans isimlerini çek
        if (DanceSlotManager.Instance != null)
        {
            string danceName = DanceSlotManager.Instance.GetAssignedDance(index);
            if (!string.IsNullOrEmpty(danceName))
                return danceName;
        }
        
        // Fallback: Default dance isimleri
        string[] defaultDances = { "Flair", "Rumba", "Twerk", "Twist" };
        return index < defaultDances.Length ? defaultDances[index] : "Flair";
    }
    
    void OnDanceButtonClick(int index)
    {
        // Cloud Save'den dans ismini al
        string danceName = GetDanceName(index);
        
        Debug.Log($"🔥 OnDanceButtonClick BAŞLADI: {danceName}");
        Debug.Log($"[Simple Dance] {danceName} seçildi");
        
        // Menüyü kapat
        HideMenu();
        
        // Direkt Animator'a trigger gönder (DanceSlotManager bypass)
        Debug.Log($"🔥 Player Animator arıyor...");
        
        // Player'ın Animator'ını bul
        Animator playerAnimator = null;
        
        // NetworkObject ile player bul
        var networkObjects = FindObjectsOfType<Unity.Netcode.NetworkObject>();
        Debug.Log($"🔥 Toplam {networkObjects.Length} NetworkObject bulundu");
        
        foreach (var netObj in networkObjects)
        {
            Debug.Log($"🔥 NetworkObject kontrol ediliyor: '{netObj.name}', IsOwner: {netObj.IsOwner}");
            
            // Animator var mı kontrol et (isim kontrolünden önce)
            Animator animator = netObj.GetComponent<Animator>();
            Debug.Log($"🔥 {netObj.name} - Animator var mı: {animator != null}");
            
            if ((netObj.IsOwner && netObj.name.Contains("Player")) || 
                netObj.name.Contains("player") || 
                netObj.name.Contains("Palyaco") ||
                netObj.name.Contains("palyaco"))
            {
                Debug.Log($"🔥 İsim eşleşti: {netObj.name}");
                
                if (animator != null)
                {
                    playerAnimator = animator;
                    Debug.Log($"✅ Player Animator bulundu: {netObj.name}");
                    break;
                }
                else
                {
                    Debug.LogWarning($"❌ {netObj.name} objesinde Animator component yok!");
                }
            }
            
            // EĞER HİÇ EŞLEŞME YOKSA VE ANIMATOR VARSA ONU AL
            if (playerAnimator == null && animator != null && netObj.IsOwner)
            {
                Debug.Log($"🔥 Fallback: IsOwner obje kullanılıyor: {netObj.name}");
                playerAnimator = animator;
                break;
            }
        }
        
        // Eğer bulamazsa normal GameObject ara
        if (playerAnimator == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Player") || obj.name.Contains("player") || 
                    obj.name.Contains("Palyaco") || obj.name.Contains("palyaco"))
                {
                    playerAnimator = obj.GetComponent<Animator>();
                    if (playerAnimator != null)
                    {
                        Debug.Log($"✅ Player Animator bulundu (normal): {obj.name}");
                        break;
                    }
                }
            }
        }
        
        if (playerAnimator != null)
        {
            string triggerName = $"Play{danceName}";
            Debug.Log($"🔥 Trigger gönderiliyor: {triggerName}");
            
            // Direkt state oyna
            try
            {
                playerAnimator.Play($"{danceName}_Dance");
                Debug.Log($"✅ STATE OYNADI: {danceName}_Dance");
            }
            catch
            {
                try
                {
                    playerAnimator.Play(danceName);
                    Debug.Log($"✅ STATE OYNADI: {danceName}");
                }
                catch
                {
                    Debug.LogWarning($"❌ State oynatılamadı: {danceName}");
                }
            }
            
            // Trigger de gönder
            playerAnimator.SetTrigger(triggerName);
            Debug.Log($"✅ Trigger gönderildi: {triggerName}");
        }
        else
        {
            Debug.LogError("❌ Player Animator bulunamadı!");
        }
        
        // DanceSlotManager üzerinden dans oynat
        if (DanceSlotManager.Instance != null)
        {
            DanceSlotManager.Instance.PlayDance(danceName);
        }
        
        // Multiplayer için EmoteController'ı da çağır
        EmoteController emoteController = FindObjectOfType<EmoteController>();
        if (emoteController != null)
        {
            emoteController.OnDanceSelected(danceName);
        }
    }
    
    string FormatDanceName(string danceName)
    {
        if (string.IsNullOrEmpty(danceName)) return "Boş";
        
        string formatted = System.Text.RegularExpressions.Regex.Replace(danceName, "([a-z])([A-Z])", "$1 $2");
        return char.ToUpper(formatted[0]) + formatted.Substring(1);
    }
}