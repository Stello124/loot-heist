using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbyMusicManager : MonoBehaviour
{
    [Header("Lobby Music Settings")]
    public AudioClip lobbyMusic;
    
    [Header("Game Music Settings")]
    public AudioClip map1Music;
    public AudioClip map2Music;
    public AudioClip map3Music;
    public AudioClip map4Music;
    
    [Header("Audio Settings")]
    public float volume = 0.7f;
    public bool fadeInOut = true;
    public float fadeDuration = 1.5f;
    
    private AudioSource audioSource;
    public static LobbyMusicManager Instance;
    
    // Sahne kategorileri
    private readonly string[] lobbyScenes = { "LobbyBrowserScene", "LobbyRoom" };
    private readonly string[] gameScenes = { "1.map", "2.map", "3.map", "4.map" };
    
    // Mevcut çalan müzik türü
    private string currentMusicType = "";
    private AudioClip currentClip;

    void Awake()
    {
        // Singleton pattern - sadece bir tane olsun
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSource();
            Debug.Log("🎵 LobbyMusicManager oluşturuldu - DontDestroyOnLoad aktif");
        }
        else
        {
            Debug.Log("🗑️ Duplicate LobbyMusicManager destroyed");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Scene değişim event'ine abone ol
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 🔍 DEBUG: Mevcut sahne ismini kontrol et
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"🔍 Mevcut sahne: '{currentScene}'");
        Debug.Log($"🔍 IsLobbyScene sonucu: {IsLobbyScene(currentScene)}");
        
        // Sahneye uygun müziği başlat
        if (IsLobbyScene(currentScene))
        {
            Debug.Log("✅ Lobby sahnesinde - lobby müziği başlatılıyor");
            PlayMusicForScene("lobby");
        }
        else if (IsGameScene(currentScene))
        {
            Debug.Log($"✅ Oyun sahnesinde - {currentScene} müziği başlatılıyor");
            PlayMusicForScene(currentScene);
        }
        else
        {
            Debug.Log($"❓ Bilinmeyen sahne: {currentScene}");
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void SetupAudioSource()
    {
        Debug.Log("🔧 SetupAudioSource başlatıldı");
        
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.Log("➕ AudioSource ekleniyor...");
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Debug.Log("✅ AudioSource zaten mevcut");
        }
        
        audioSource.clip = lobbyMusic;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        
        Debug.Log($"🔊 AudioSource hazırlandı - Clip: {(lobbyMusic ? lobbyMusic.name : "NULL")}, Loop: ON, Volume: {volume}");
        
        // Audio Listener kontrolü
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener == null)
        {
            Debug.LogWarning("⚠️ Sahnede AudioListener yok! Ses duyulmayabilir.");
        }
        else
        {
            Debug.Log($"✅ AudioListener bulundu: {listener.gameObject.name}");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        Debug.Log($"🌍 Scene yüklendi: {sceneName}");
        
        if (IsLobbyScene(sceneName))
        {
            // Lobby sahnesindeyiz - lobby müziğini çal
            PlayMusicForScene("lobby");
        }
        else if (IsGameScene(sceneName))
        {
            // Oyun sahnesindeyiz - o map'in müziğini çal
            PlayMusicForScene(sceneName);
        }
        else
        {
            // Bilinmeyen sahne - müziği durdur
            StopAllMusic();
        }
    }

    public void PlayMusicForScene(string sceneType)
    {
        Debug.Log($"🎵 PlayMusicForScene çağrıldı: {sceneType}");
        
        if (audioSource == null)
        {
            Debug.LogError("❌ AudioSource NULL!");
            return;
        }

        // Sahneye uygun müziği seç
        AudioClip targetClip = GetMusicForScene(sceneType);
        
        if (targetClip == null)
        {
            Debug.LogWarning($"⚠️ {sceneType} için müzik dosyası atanmamış!");
            StopAllMusic();
            return;
        }

        // Aynı müzik çalıyorsa değişiklik yapma
        if (currentMusicType == sceneType && audioSource.isPlaying && audioSource.clip == targetClip)
        {
            Debug.Log($"🎵 {sceneType} müziği zaten çalıyor - devam ediyor");
            return;
        }

        // Yeni müziği başlat
        StartNewMusic(targetClip, sceneType);
    }

    private void StartNewMusic(AudioClip clip, string musicType)
    {
        Debug.Log($"🎵 Yeni müzik başlatılıyor: {clip.name} ({musicType})");
        
        currentMusicType = musicType;
        currentClip = clip;
        
        // Eski müziği durdur
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Yeni müziği ayarla
        audioSource.clip = clip;
        
        if (fadeInOut)
        {
            Debug.Log("🌊 Fade-in ile başlatılıyor...");
            StartCoroutine(FadeIn());
        }
        else
        {
            Debug.Log("🎵 Direct play...");
            audioSource.volume = volume;
            audioSource.Play();
        }
        
        Debug.Log($"🎵 Müzik başlatıldı: {clip.name}");
    }

    private AudioClip GetMusicForScene(string sceneType)
    {
        switch (sceneType.ToLower())
        {
            case "lobby":
                return lobbyMusic;
            case "1.map":
                return map1Music;
            case "2.map":
                return map2Music;
            case "3.map":
                return map3Music;
            case "4.map":
                return map4Music;
            default:
                Debug.LogWarning($"❓ Bilinmeyen sahne türü: {sceneType}");
                return null;
        }
    }

    [System.Obsolete("StartLobbyMusic() deprecated. Use PlayMusicForScene() instead.")]
    public void StartLobbyMusic()
    {
        Debug.Log("⚠️ StartLobbyMusic() deprecated - PlayMusicForScene() kullanılıyor");
        PlayMusicForScene("lobby");
    }

    public void StopAllMusic()
    {
        if (audioSource == null || !audioSource.isPlaying) 
            return;

        Debug.Log($"🔇 Müzik durduruluyor: {currentMusicType}");

        if (fadeInOut)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            audioSource.Stop();
        }
        
        currentMusicType = "";
        currentClip = null;
        Debug.Log("🔇 Tüm müzik durduruldu");
    }

    [System.Obsolete("StopLobbyMusic() deprecated. Use StopAllMusic() instead.")]
    public void StopLobbyMusic()
    {
        Debug.Log("⚠️ StopLobbyMusic() deprecated - StopAllMusic() kullanılıyor");
        StopAllMusic();
    }

    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log($"⏸️ Müzik duraklatıldı: {currentMusicType}");
        }
    }

    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
            Debug.Log($"▶️ Müzik devam ettiriliyor: {currentMusicType}");
        }
    }

    // Backward compatibility
    [System.Obsolete("Use PauseMusic() instead.")]
    public void PauseLobbyMusic() => PauseMusic();
    
    [System.Obsolete("Use ResumeMusic() instead.")]
    public void ResumeLobbyMusic() => ResumeMusic();

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
        Debug.Log($"🔊 Lobby müzik sesi: {volume:F2}");
    }

    private bool IsLobbyScene(string sceneName)
    {
        foreach (string lobbyScene in lobbyScenes)
        {
            if (sceneName == lobbyScene)
                return true;
        }
        return false;
    }

    private bool IsGameScene(string sceneName)
    {
        foreach (string gameScene in gameScenes)
        {
            if (sceneName == gameScene)
                return true;
        }
        return false;
    }

    private IEnumerator FadeIn()
    {
        audioSource.volume = 0f;
        audioSource.Play();
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, timer / fadeDuration);
            yield return null;
        }
        
        audioSource.volume = volume;
        Debug.Log("🎵 Fade-in tamamlandı");
    }

    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }
        
        audioSource.volume = 0f;
        audioSource.Stop();
        Debug.Log("🔇 Fade-out tamamlandı");
    }

    // Inspector'da test için
    [ContextMenu("🎵 Test Lobby Music")]
    public void TestLobbyMusic()
    {
        Debug.Log("🎮 Lobby müzik testi!");
        PlayMusicForScene("lobby");
    }

    [ContextMenu("🗺️ Test Map1 Music")]
    public void TestMap1Music()
    {
        Debug.Log("🎮 Map1 müzik testi!");
        PlayMusicForScene("1.map");
    }
    
    [ContextMenu("🗺️ Test Map3 Music")]
    public void TestMap3Music()
    {
        Debug.Log("🎮 Map3 müzik testi!");
        PlayMusicForScene("3.map");
    }

    [ContextMenu("🔇 Test Stop Music")]
    public void TestStopMusic()
    {
        Debug.Log("🎮 Stop müzik testi!");
        StopAllMusic();
    }
    
    [ContextMenu("🔍 Debug Current State")]
    public void DebugCurrentState()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"🔍 Mevcut sahne: '{currentScene}'");
        Debug.Log($"🔍 Current music type: '{currentMusicType}'");
        Debug.Log($"🔍 Current clip: {(currentClip ? currentClip.name : "NULL")}");
        Debug.Log($"🔍 AudioSource playing: {(audioSource ? audioSource.isPlaying.ToString() : "NULL")}");
        Debug.Log($"🔍 Volume: {volume}");
        Debug.Log($"🔍 Assigned clips: Lobby={lobbyMusic?.name}, Map1={map1Music?.name}, Map2={map2Music?.name}, Map3={map3Music?.name}, Map4={map4Music?.name}");
    }
}