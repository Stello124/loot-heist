# 💣 Bomb Visibility Çözümü

## 🎯 Problem
Palyaco prefab'ında statik olarak koyulmuş **BombVisual** objesi tüm sahnelerde görünüyordu.

## ✅ Çözüm
**BombVisibilityController** script'i ile scene-based visibility kontrol.

### Sistem Nasıl Çalışıyor:

#### 1. Otomatik Algılama
```csharp
// NetworkPlayerSpawnerK spawn ettikten sonra:
AddBombVisibilityController(obj);

// Prefabta BombVisual var mı kontrol et:
Transform[] allChildren = playerObj.GetComponentsInChildren<Transform>(true);
foreach (Transform child in allChildren)
{
    if (child.name == "BombVisual")
    {
        hasBombVisual = true;
        break;
    }
}
```

#### 2. Dinamik Script Ekleme
```csharp
// Sadece bomba varsa BombVisibilityController ekle:
if (hasBombVisual)
{
    playerObj.AddComponent<BombVisibilityController>();
}
```

#### 3. Scene-Based Control
```csharp
// BombVisibilityController.cs içinde:
private void UpdateBombVisibility()
{
    string currentScene = SceneManager.GetActiveScene().name;
    bool shouldShowBomb = (currentScene == "3.map");
    
    bombVisual.SetActive(shouldShowBomb);
}
```

### Hangi Dosyalar Etkilendi:

1. **`Scripts/Player/BombVisibilityController.cs`** (YENİ)
   - Scene kontrolü yapıyor
   - BombVisual'ı enable/disable ediyor

2. **`Scripts/Player/NetworkPlayerSpawnerK.cs`** (GÜNCELLENDİ)
   - `AddBombVisibilityController()` method eklendi

3. **`Scripts/1.Map/NetworkPlayerSpawnerK_1Map.cs`** (GÜNCELLENDİ)  
   - `AddBombVisibilityController()` method eklendi

### Avantajları:

- ✅ **Prefab'a dokunmuyor** - güvenli
- ✅ **Otomatik algılama** - bomba varsa çalışır
- ✅ **Scene-aware** - sahne değişiminde otomatik
- ✅ **Performance friendly** - gereksiz obje yok
- ✅ **Memory safe** - scene event cleanup

## 🎮 Sonuç

**ARTIK:**
- 🏠 Lobby → Bomba YOK ✅  
- 🏁 1.map → Bomba YOK ✅
- 💣 3.map → Bomba VAR ✅
- 🏠 Diğer → Bomba YOK ✅

**PREFAB BOZULMADI - SADECE RUNTİME KONTROL EKLENDİ!** 💣🎯