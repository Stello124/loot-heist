using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using System.Collections;

public class NetworkCharacterCustomization : NetworkBehaviour
{
    [Header("Customization")]
    public CharacterBuilder characterBuilder;

    private NetworkVariable<FixedString512Bytes> customizationJson =
        new NetworkVariable<FixedString512Bytes>("", NetworkVariableReadPermission.Everyone);

    void Update()
    {
        if (Time.time % 3f < 0.1f)
        {
            NetworkCharacterCustomization[] allCustomizations = FindObjectsOfType<NetworkCharacterCustomization>();
            Debug.Log($"🔍 === SAHNEDE TOPLAM: {allCustomizations.Length} ===");

            foreach (var custom in allCustomizations)
            {
                string jsonPreview = custom.customizationJson.Value.ToString();
                jsonPreview = jsonPreview.Length > 50 ? jsonPreview.Substring(0, 50) + "..." : jsonPreview;

                Debug.Log($"   - ClientID: {custom.OwnerClientId}, IsOwner: {custom.IsOwner}, " +
                         $"GameObject: {custom.gameObject.name}, JSON: '{jsonPreview}'");
            }
            Debug.Log($"🔍 === SON ===");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"🌐 SPAWN DEBUG - IsOwner: {IsOwner}, ClientID: {OwnerClientId}, JSON: '{customizationJson.Value}'");

        if (characterBuilder == null)
        {
            characterBuilder = GetComponent<CharacterBuilder>();
            if (characterBuilder == null)
            {
                characterBuilder = GetComponentInChildren<CharacterBuilder>();
            }

            if (characterBuilder != null)
            {
                Debug.Log($"✅ CharacterBuilder otomatik bulundu: {gameObject.name}");
            }
            else
            {
                Debug.LogError($"❌ CharacterBuilder bulunamadı: {gameObject.name}");
                return;
            }
        }

        customizationJson.OnValueChanged += OnCustomizationChanged;

        if (IsOwner)
        {
            LoadAndApplyCustomization();

            if (IsServer)
            {
                StartCoroutine(ResendCustomizationAfterSpawn());
            }
        }

        if (IsClient && !IsOwner)
        {
            Debug.Log($"🔍 Client'ta başkasının karakteri spawn oldu - JSON kontrol ediliyor: ClientID {OwnerClientId}");

            string json = customizationJson.Value.ToString();
            if (!string.IsNullOrEmpty(json))
            {
                Debug.Log($"✅ JSON zaten dolu - doğrudan uygulanıyor: ClientID {OwnerClientId}");
                ApplyCustomizationFromJson(json);
            }
            else
            {
                StartCoroutine(WaitAndApplyCustomization());
            }
        }
    }

    private IEnumerator ResendCustomizationAfterSpawn()
    {
        yield return new WaitForSeconds(1f);

        string json = customizationJson.Value.ToString();
        if (!string.IsNullOrEmpty(json))
        {
            Debug.Log($"📡 Spawn sonrası JSON tekrar gönderiliyor: {json}");
            SetCustomizationServerRpc(json);
        }
    }

    private IEnumerator WaitAndApplyCustomization()
    {
        int attempts = 0;
        while (attempts < 10)
        {
            if (!string.IsNullOrEmpty(customizationJson.Value.ToString()))
            {
                Debug.Log($"✅ Attempt {attempts + 1} başarılı - özelleştirme uygulanıyor: ClientID {OwnerClientId}");
                ApplyCustomizationFromJson(customizationJson.Value.ToString());
                yield break;
            }

            Debug.Log($"🔄 Attempt {attempts + 1} - JSON boş, bekleniyor: ClientID {OwnerClientId}");
            attempts++;
            yield return new WaitForSeconds(1f);
        }

        Debug.LogError($"❌ 10 saniye sonra JSON hala boş: ClientID {OwnerClientId}");
    }

    private async void LoadAndApplyCustomization()
    {
        Debug.Log($"🔄 CloudSave'den özelleştirme yükleniyor: ClientID {OwnerClientId}");

        var playerData = await CustomizationSaveManager.Instance.LoadCustomizationFromCloud();

        if (playerData != null)
        {
            Debug.Log($"📦 Customization JSON: {JsonUtility.ToJson(playerData)}");

            characterBuilder.ApplyCustomization(playerData);
            Debug.Log($"✅ Kendi özelleştirmemiz uygulandı: ClientID {OwnerClientId}");

            string json = JsonUtility.ToJson(playerData);
            SetCustomizationServerRpc(json);
        }
        else
        {
            Debug.LogWarning($"⚠️ CloudSave'den veri yüklenemedi: ClientID {OwnerClientId}");
        }
    }

    [ServerRpc]
    private void SetCustomizationServerRpc(string json)
    {
        Debug.Log($"📡 Server'a özelleştirme gönderiliyor: ClientID {OwnerClientId}");
        customizationJson.Value = json;
    }

    private void OnCustomizationChanged(FixedString512Bytes oldValue, FixedString512Bytes newValue)
    {
        Debug.Log($"🔄 JSON değişti! ClientID: {OwnerClientId}, Old: '{oldValue}', New: '{newValue}'");
        Debug.Log($"🔍 IsOwner: {IsOwner}, JSON boş mu: {string.IsNullOrEmpty(newValue.ToString())}");

        if (!string.IsNullOrEmpty(newValue.ToString()))
        {
            Debug.Log($"✅ NetworkVariable değişikliği uygulanıyor: ClientID {OwnerClientId}, IsOwner: {IsOwner}");
            ApplyCustomizationFromJson(newValue.ToString());
        }
        else
        {
            Debug.Log($"⚠️ JSON boş, özelleştirme uygulanmıyor: ClientID {OwnerClientId}");
        }
    }

    private void ApplyCustomizationFromJson(string json)
    {
        try
        {
            Debug.Log($"🎨 JSON uygulanıyor: ClientID {OwnerClientId}, JSON: {json}");

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"JSON boş! ClientID: {OwnerClientId}");
                return;
            }

            if (characterBuilder == null)
            {
                characterBuilder = GetComponent<CharacterBuilder>();
                if (characterBuilder == null)
                {
                    characterBuilder = GetComponentInChildren<CharacterBuilder>();
                }

                if (characterBuilder == null)
                {
                    Debug.LogError($"❌ CharacterBuilder null! ClientID: {OwnerClientId}");
                    return;
                }
            }

            PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);

            if (playerData == null)
            {
                Debug.LogError($"❌ JSON parse başarısız! ClientID: {OwnerClientId}");
                return;
            }

            characterBuilder.ApplyCustomization(playerData);
            Debug.Log($"✅ Network'ten özelleştirme uygulandı: ClientID {OwnerClientId}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ JSON parse hatası: {ex.Message}, JSON: {json}, ClientID: {OwnerClientId}");
        }
    }

    void OnDestroy()
    {
        if (customizationJson != null)
        {
            customizationJson.OnValueChanged -= OnCustomizationChanged;
        }
    }
}