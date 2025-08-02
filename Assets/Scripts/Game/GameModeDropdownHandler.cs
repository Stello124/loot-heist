using UnityEngine;
using UnityEngine.UI;

public class GameModeDropdownHandler : MonoBehaviour
{
    [SerializeField] private Dropdown gameModeDropdown;

    public static string SelectedGameMode { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // Sahne değişiminde kaybolmasın
    }

    private void Start()
    {
        gameModeDropdown.ClearOptions();
        gameModeDropdown.AddOptions(new System.Collections.Generic.List<string> { "DeneyK2", "Köprü","Bomba","Yarış" }); // Seçenekleri ekledik

        gameModeDropdown.onValueChanged.AddListener(OnGameModeChanged);
        gameModeDropdown.value = 0;
        OnGameModeChanged(0); // Otomatik çalıştır
    }

    private void OnGameModeChanged(int index)
    {
        SelectedGameMode = gameModeDropdown.options[index].text;
        Debug.Log("🎮 Seçilen mod: " + SelectedGameMode);
    }
}
