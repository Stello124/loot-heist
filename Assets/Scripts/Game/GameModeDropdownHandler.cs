using UnityEngine;
using UnityEngine.UI;

public class GameModeDropdownHandler : MonoBehaviour
{
    [SerializeField] private Dropdown gameModeDropdown;

    // 🔧 Static property için backing field
    private static string _selectedGameMode = "DeneyK2"; // Default değer

    public static string SelectedGameMode
    {
        get
        {
            Debug.Log($"🎮 SelectedGameMode GET çağrıldı: '{_selectedGameMode}'");
            return _selectedGameMode;
        }
        private set
        {
            _selectedGameMode = value;
            Debug.Log($"🎮 SelectedGameMode SET edildi: '{_selectedGameMode}'");
        }
    }

    private void Awake()
    {
        Debug.Log("🎮 GameModeDropdownHandler Awake çalıştı");
        DontDestroyOnLoad(this.gameObject);

        // 🔧 Eğer zaten bir instance varsa, bunu yok et
        GameModeDropdownHandler[] handlers = FindObjectsOfType<GameModeDropdownHandler>();
        if (handlers.Length > 1)
        {
            Debug.Log("🔄 Duplicate GameModeDropdownHandler bulundu, yok ediliyor");
            Destroy(this.gameObject);
            return;
        }
    }

    private void Start()
    {
        Debug.Log("🎮 GameModeDropdownHandler Start çalıştı");

        if (gameModeDropdown == null)
        {
            Debug.LogError("❌ GameModeDropdown atanmamış!");
            return;
        }

        // Dropdown'ı ayarla
        gameModeDropdown.ClearOptions();
        gameModeDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "DeneyK2", "Köprü", "Bomba", "Yarış"
        });

        gameModeDropdown.onValueChanged.AddListener(OnGameModeChanged);

        // 🔧 Default değeri ayarla
        gameModeDropdown.value = 0;
        OnGameModeChanged(0); // İlk değeri set et

        Debug.Log($"🎮 Dropdown ayarlandı. Mevcut değer: {SelectedGameMode}");
    }

    private void OnGameModeChanged(int index)
    {
        if (gameModeDropdown == null || index < 0 || index >= gameModeDropdown.options.Count)
        {
            Debug.LogError($"❌ Geçersiz dropdown index: {index}");
            return;
        }

        string newMode = gameModeDropdown.options[index].text;
        SelectedGameMode = newMode;

        Debug.Log($"🎮 Dropdown değişti - Index: {index}, Mod: '{newMode}'");
    }

    // 🔧 Manuel test için public method
    public void ForceSetGameMode(string mode)
    {
        SelectedGameMode = mode;
        Debug.Log($"🔧 Manuel olarak set edildi: '{mode}'");
    }

    // 🔧 Debug için
    private void Update()
    {
        // T tuşuna basıldığında debug bilgisi ver
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"🔍 DEBUG - Current SelectedGameMode: '{SelectedGameMode}'");
            Debug.Log($"🔍 DEBUG - Dropdown value: {(gameModeDropdown != null ? gameModeDropdown.value.ToString() : "NULL")}");
        }
    }
}