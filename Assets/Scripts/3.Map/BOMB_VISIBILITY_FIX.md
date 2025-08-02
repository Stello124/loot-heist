# 💣 Bomb Görünürlük Düzeltmesi

## 🎯 Problem
Oyuncunun elindeki bomba tüm maplerde görünüyordu.

## ✅ Çözüm
BombManager'a scene kontrolü eklendi.

### Değişiklikler:

1. **Scene Kontrolü:** Bomba sadece "3.map" sahnesinde görünür
2. **Scene Değişim Temizliği:** Diğer sahnelere geçişte bomba otomatik temizlenir

### Kod Değişiklikleri:

```csharp
// UpdateBombVisualClientRpc() içinde:
string currentScene = SceneManager.GetActiveScene().name;
if (currentScene != "3.map")
{
    Debug.Log($"💣 Bomba görseli iptal edildi - Sahne: {currentScene}");
    return;
}

// Scene değişim event'i:
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name != "3.map" && bombVisual != null)
    {
        Destroy(bombVisual);
        bombVisual = null;
    }
}
```

## 🎮 Sonuç

**Artık bomba:**
- ✅ 3.map'te görünür  
- ✅ 1.map'te görünmez
- ✅ Lobby'de görünmez
- ✅ Diğer sahnelerde görünmez

**Otomatik temizleme:**
- ✅ Scene değişiminde bomba temizlenir
- ✅ Memory leak yok
- ✅ Performance optimizasyonu