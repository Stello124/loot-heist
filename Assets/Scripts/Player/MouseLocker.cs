using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseLocker : MonoBehaviour
{
    [SerializeField] private string[] exemptScenes = { "LobbyBrowserScene", "LobbyRoom" };

    private bool isCursorLocked = false;
    private string currentScene = "";

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[MouseLocker] Baþlangýç sahnesi: {currentScene}");
        UnlockCursor();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
        Debug.Log($"[MouseLocker] Sahne yüklendi: {currentScene}");
        UnlockCursor();
    }

    void Update()
    {
        if (IsExemptScene(currentScene))
        {
            // Muaf sahnelerde her zaman fare serbest ve görünür
            UnlockCursor();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ESC basýldýðýnda fare kilidini aç veya kapat
            if (isCursorLocked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }
        else if (!isCursorLocked && Input.GetMouseButtonDown(0))
        {
            // Fare kilitli deðilse ve sol týklama varsa fareyi kilitle
            LockCursor();
        }
    }

    bool IsExemptScene(string sceneName)
    {
        foreach (var scene in exemptScenes)
        {
            if (sceneName == scene)
                return true;
        }
        return false;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
        Debug.Log("[MouseLocker] Fare kilitlendi.");
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
        Debug.Log("[MouseLocker] Fare açýldý.");
    }
}