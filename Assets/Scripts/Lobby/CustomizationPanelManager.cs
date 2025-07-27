using System.Collections.Generic;
using UnityEngine;

public class CustomizationPanelManager : MonoBehaviour
{
    [SerializeField] private Transform visualContainer;
    private GameObject currentCharacter;
    private CharacterBuilder builder;

    void Start()
    {
        InitializeCharacter();
    }

    private void InitializeCharacter()
    {
        if (visualContainer.childCount > 1)
        {
            Debug.LogWarning("⚠️ VisualContainer içinde birden fazla karakter prefabı var! Sadece bir tane olmalı.");
            for (int i = visualContainer.childCount - 1; i > 0; i--)
            {
                Destroy(visualContainer.GetChild(i).gameObject);
            }
        }

        if (visualContainer.childCount > 0)
        {
            currentCharacter = visualContainer.GetChild(0).gameObject;
            builder = currentCharacter.GetComponent<CharacterBuilder>();

            if (builder == null)
            {
                Debug.LogError("❌ CharacterBuilder prefab içinde eksik.");
                return;
            }

            ApplySavedCustomizationInstantly(); // 👈 Başlangıçta da uygulanıyor
        }
        else
        {
            Debug.LogWarning("⚠️ VisualContainer içinde karakter prefab bulunamadı.");
        }
    }

    /// <summary>
    /// Bu fonksiyon GameManager tarafından da tetiklenebilir.
    /// Panel açılmadan görünüm uygulamak için kullanılır.
    /// </summary>
    public void ApplySavedCustomizationInstantly()
    {
        if (builder == null)
        {
            Debug.LogWarning("❌ ApplySavedCustomizationInstantly → builder null.");
            return;
        }

        var data = GameState.LocalPlayerData;
        if (data != null)
        {
            data.EnsureCustomizationReady();

            if (data.CustomizationData == null || data.CustomizationData.Count == 0)
            {
                Debug.LogWarning("⚠️ CustomizationData boş → default kayıt atanıyor.");

                data.CustomizationData = new Dictionary<string, string>()
                {
                    { "Body", "palyaco" }
                };
                data.BakeCustomizationData();
            }

            builder.ApplyCustomization(data);
            Debug.Log("🎬 ApplySavedCustomizationInstantly → görünüm uygulandı.");
        }
        else
        {
            Debug.LogWarning("⚠️ GameState.LocalPlayerData boş → görünüm uygulanamadı.");
        }
    }

    // Randomize tuşuna basınca tetiklenecek fonksiyon
    public void OnRandomizeButtonClicked()
    {
        if (builder == null)
        {
            Debug.LogWarning("⛔ CharacterBuilder erişilemedi.");
            return;
        }

        var data = GameState.LocalPlayerData;
        if (data == null)
        {
            Debug.LogWarning("❌ PlayerData bulunamadı.");
            return;
        }

        builder.ApplyRandomCustomization(data);  // PlayerData'daki görünümü rastgele değiştirir

        builder.ApplyCustomization(data);        // Yeni görünümü sahneye uygular

        data.BakeCustomizationData();            // Dictionary ve List dönüştürme işlemi

        CustomizationSaveManager.Instance.SaveCustomizationToCloud(data);  // Cloud'a kaydet

        Debug.Log("✅ Random + Apply + Cloud Save başarıyla tamamlandı.");
    }
}
