using UnityEngine;
using System.Collections.Generic;

public class CharacterBuilder : MonoBehaviour
{
    public SkinnedMeshRenderer AccessoriesRenderer;
    public SkinnedMeshRenderer BodyRenderer;
    public SkinnedMeshRenderer FaceRenderer;
    public SkinnedMeshRenderer FullBodyRenderer;
    public SkinnedMeshRenderer GlassesRenderer;
    public SkinnedMeshRenderer GlovesRenderer;
    public SkinnedMeshRenderer HairstyleRenderer;
    public SkinnedMeshRenderer HatRenderer;
    public SkinnedMeshRenderer MustacheRenderer;
    public SkinnedMeshRenderer OuterwearRenderer;
    public SkinnedMeshRenderer PantsRenderer;
    public SkinnedMeshRenderer ShoesRenderer;
    public SkinnedMeshRenderer TShirtRenderer;

    private string meshPath = "CharacterParts/";

    public void ApplyCustomization(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogWarning("❌ ApplyCustomization → PlayerData boş.");
            return;
        }

        data.RestoreCustomizationData();
        var parts = data.CustomizationData;

        TrySet(parts, "Hat", HatRenderer);
        TrySet(parts, "Hairstyle", HairstyleRenderer);
        TrySet(parts, "Shoes", ShoesRenderer);
        TrySet(parts, "Body", BodyRenderer);
        TrySet(parts, "Glasses", GlassesRenderer);
        TrySet(parts, "Mustache", MustacheRenderer);
        TrySet(parts, "Gloves", GlovesRenderer);
        TrySet(parts, "Accessories", AccessoriesRenderer);
        TrySet(parts, "Faces", FaceRenderer);
        TrySet(parts, "TShirt", TShirtRenderer);
        TrySet(parts, "Outerwear", OuterwearRenderer);
        TrySet(parts, "Pants", PantsRenderer);
        // FullBodyRenderer özel kullanımda değerlendirilebilir
    }

    private void TrySet(Dictionary<string, string> config, string slot, SkinnedMeshRenderer renderer)
    {
        if (config.TryGetValue(slot, out string meshName))
            SetMesh(renderer, meshPath + slot + "/" + meshName);
    }

    void SetMesh(SkinnedMeshRenderer renderer, string fullPath)
    {
        Mesh mesh = Resources.Load<Mesh>(fullPath);
        if (mesh != null)
            renderer.sharedMesh = mesh;
        else
            Debug.LogWarning($"❌ Mesh bulunamadı: {fullPath}");
    }

    public void ApplyRandomCustomization(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogError("❌ PlayerData null → random atama iptal.");
            return;
        }

        Dictionary<string, string> config = new Dictionary<string, string>();

        AssignRandomMesh(HatRenderer, "Hat", config);
        AssignRandomMesh(HairstyleRenderer, "Hairstyle", config);
        AssignRandomMesh(ShoesRenderer, "Shoes", config);
        AssignRandomMesh(BodyRenderer, "Body", config); // Zorunlu
        AssignRandomMesh(GlassesRenderer, "Glasses", config);
        AssignRandomMesh(MustacheRenderer, "Mustache", config);
        AssignRandomMesh(GlovesRenderer, "Gloves", config);
        AssignRandomMesh(AccessoriesRenderer, "Accessories", config);
        AssignRandomMesh(FaceRenderer, "Faces", config);
        AssignRandomMesh(TShirtRenderer, "TShirt", config);
        AssignRandomMesh(OuterwearRenderer, "Outerwear", config);
        AssignRandomMesh(PantsRenderer, "Pants", config);

        data.CustomizationData = config;

        if (config.Count == 0)
        {
            Debug.LogError("❌ Random meshler atanmadı → CustomizationData boş.");
            return;
        }

        data.BakeCustomizationData(); // 🔧 Cloud Save için veri listeye dönüştürüldü
        Debug.Log($"✅ Random sonrası {config.Count} mesh → CustomizationData güncellendi ve bake edildi.");
    }

    private void AssignRandomMesh(SkinnedMeshRenderer renderer, string slotName, Dictionary<string, string> config)
    {
        Mesh[] options = Resources.LoadAll<Mesh>(meshPath + slotName + "/");

        if (options == null || options.Length == 0)
        {
            Debug.LogWarning($"❌ {slotName} → mesh klasörü boş veya bulunamadı.");
            return;
        }

        // Body → boş bırakılmaz
        if (slotName == "Body")
        {
            Mesh selected = options[0];
            renderer.sharedMesh = selected;
            config[slotName] = selected.name;
            Debug.Log($"✅ {slotName} → zorunlu mesh atandı: {selected.name}");
            return;
        }

        // Diğer slotlar → %25 boş bırakma ihtimali
        if (Random.value < 0.25f)
        {
            renderer.sharedMesh = null;
            Debug.Log($"🔄 {slotName} → boş bırakıldı.");
            return;
        }

        int index = Random.Range(0, options.Length);
        Mesh mesh = options[index];
        renderer.sharedMesh = mesh;
        config[slotName] = mesh.name;

        Debug.Log($"✅ {slotName} → mesh atandı: {mesh.name}");
    }
}
