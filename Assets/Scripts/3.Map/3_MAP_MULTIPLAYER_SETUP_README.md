# 🎮 3.Map Multiplayer Dönüşüm Rehberi

## 📋 Yapılan Değişiklikler

### ✅ Yeni Network Scripts
1. **NetworkSpawnManager3Map.cs** - Multiplayer oyuncu spawn sistemi
2. **BombManager.cs** - Network uyumlu bomba sistemi (güncellendi)
3. **PlayerBombToucher.cs** - Network uyumlu bomba transfer sistemi (güncellendi)
4. **PlayerAttack.cs** - Network uyumlu saldırı sistemi (güncellendi)
5. **GameFlowController.cs** - Network uyumlu oyun akış kontrolü (güncellendi)

### 📦 Yedeklenen Scripts
- **SpawnManager_Original_Backup.cs** - Orijinal offline spawn manager
- **BombManager_Original_Backup.cs** - Orijinal offline bomb manager

## 🚀 Kurulum Talimatları

### 1. 3.map Unity Sahnesinde Kurulum

#### A) Eski SpawnManager'ı Devre Dışı Bırak
```
1. 3.map sahnesini Unity'de aç
2. SpawnManager component'ine sahip GameObject'i bul
3. SpawnManager component'ini devre dışı bırak veya kaldır
```

#### B) NetworkSpawnManager3Map Ekle
```
1. Boş bir GameObject oluştur: "NetworkSpawnManager3Map"
2. NetworkSpawnManager3Map.cs script'ini ekle
3. Inspector'da şu ayarları yap:
   - Spawnable Prefabs: "palyaco" prefab'ını listele (Resources/Characters/palyaco)
   - Spawn Points: Mevcut spawn point Transform'larını ata
   - Use Spawn Points: true
   - Debug Mode: true (test için)
   
🎭 NOT: Sistem her zaman "palyaco" prefab kullanır, cloud'dan sadece giyim customization'ı çeker
```

#### C) BombManager'ı Güncelle
```
1. BombManager GameObject'ini bul
2. BombManager component'ini kontrol et (otomatik güncellenmiş olmalı)
3. NetworkBehaviour olarak çalıştığından emin ol
```

### 2. Prefab Ayarları

#### Network Oyuncu Prefabları  
```
"palyaco" prefab'ında olması gerekenler:
✅ NetworkObject component
✅ CharacterBuilder component
✅ NetworkCharacterCustomization component
✅ PlayerBombToucher component (NetworkBehaviour)
✅ PlayerAttack component (NetworkBehaviour)
✅ MovePlayerInput component (İthappy - kamera ve hareket için)
✅ CharacterMover component (İthappy - hareket için)
✅ "Player" tag'i
✅ RightHand child object (bomba için)

🎥 KAMERA: MovePlayerInput otomatik sahneye PlayerCamera component'li kamera arar
```

### 3. Sahneye Kamera Ekle (ÖNEMLİ!)

#### İthappy Kamera Sistemi
```
1. İthappy → Creative_Characters_FREE → Prefabs'ta kamera prefab'ı ara
2. Yoksa manuel oluştur:
   - Boş GameObject: "ThirdPersonCamera" 
   - Camera component ekle
   - ThirdPersonCamera.cs script ekle (İthappy'den)
   - "MainCamera" tag'ı ver
   - Position: (0, 5, -10)

3. 3.map sahnesine ekle
4. Main Camera olarak ayarla
```

### 4. Network Prefabs Listesi
```
Unity'de Network Manager'da oyuncu prefablarını şu listeye ekle:
- Network Manager → Network Prefabs List
- "palyaco" prefab'ını ekle
```

## 🎯 Multiplayer Akış

### 1. Oyun Başlangıcı
```
LobbyBrowserScene → Lobby Room → 3.map
1. Host oyunu başlatır
2. Client'lar bağlanır  
3. NetworkSpawnManager3Map oyuncuları spawn eder
4. MovePlayerInput her oyuncuya kendi kamerasını atar
5. GameFlowController bomba oyununu başlatır
```

### 2. Oyun Mekaniği
```
1. Random bir oyuncuya bomba atanır
2. Bomba sahibi diğer oyunculara dokunarak/saldırarak bombayı transfer eder
3. Süre bittiğinde bomba sahibi oyundan çıkar
4. Kalan 1 oyuncu kalana kadar devam eder
```

### 3. Kamera ve Hareket (İthappy)
```
1. Her oyuncu kendi kamerasını kontrol eder
2. Mouse ile kamera döndürme
3. WASD ile hareket
4. Shift ile koşma
5. ESC ile cursor lock/unlock
```

## 🔧 Troubleshooting

### Sık Karşılaşılan Sorunlar

#### Oyuncular Spawn Olmuyor
```
✅ NetworkManager aktif mi?
✅ NetworkSpawnManager3Map GameObject aktif mi?
✅ Spawn points atanmış mı?
✅ Spawnable prefabs listesi dolu mu?
```

#### Bomba Transfer Çalışmıyor
```
✅ PlayerBombToucher NetworkBehaviour mi?
✅ Oyuncu prefabında "Player" tag'i var mı?
✅ RightHand child object mevcut mu?
✅ BombManager Instance null değil mi?
```

#### Özelleştirmeler Görünmüyor
```
✅ NetworkCharacterCustomization component var mı?
✅ CharacterBuilder component var mı?
✅ CustomizationSaveManager çalışıyor mu?
✅ Cloud'dan data çekiliyor mu? (Console'da 🎨 loglar)
```

#### Kamera Çalışmıyor (İthappy Sistemi)
```
✅ Sahneye ThirdPersonCamera eklendi mi?
✅ ThirdPersonCamera component'i var mı kamerada?
✅ "MainCamera" tag'ı atanmış mı?
✅ MovePlayerInput component'i palyaco prefab'ta var mı?
✅ Console'da "🎥 Kamera owner'a atandı" mesajı var mı?
✅ ESC ile cursor unlock/lock olabiliyor mu?
```

### Debug Logs
```
NetworkSpawnManager3Map ve BombManager debug modlarını açık tutun:
- "[NetworkSpawnManager3Map]" prefix'li loglar
- "💣" prefix'li bomba logları
- "🎨" prefix'li özelleştirme logları
```

## 📊 Performance Notları

### Network Optimizasyonu
```
- Bomba transferi: ServerRPC kullanır
- Özelleştirmeler: NetworkVariable ile senkronize
- Animasyonlar: RPC ile senkronize (CharacterMover)
- UI güncellemeleri: ClientRPC ile
```

### Memory Yönetimi
```
- Oyuncu disconnect'inde otomatik temizlik
- NetworkList otomatik güncelleme
- Bomb visual'ları client-side spawn/destroy
```

## 🎮 Test Senaryoları

### Multiplayer Test
```
1. Host + 1 Client ile test
2. Host + 3 Client ile test  
3. Client disconnect durumu testi
4. Host disconnect durumu testi
```

### Gameplay Test
```
1. Bomba transfer (touch)
2. Bomba transfer (attack)
3. Timer countdown
4. Oyuncu elimination
5. Kazanan belirleme
```

## 📞 Destek

Sorun yaşarsanız:
1. Console log'larını kontrol edin
2. Debug mod'ları açın
3. Network Manager durumunu kontrol edin
4. Prefab ayarlarını doğrulayın

## 📦 4. Boost Chest Sistemi (BONUS)

### NetworkChest Kurulumu
```
1. Chest prefab'ı oluştur:
   - NetworkObject + NetworkChest.cs script
   - Collider (IsTrigger = true)
   - 3D Model + AudioSource

2. NetworkChest ayarları:
   - Chest Type: "speed" veya "jump"
   - Respawn Time: 3 saniye
   - Pickup Sound & Effect

3. PALYACO PREFAB'I DEĞİŞTİRME! (otomatik eklenir)
```

### Chest Türleri
```
🦘 Jump Chest: Zıplama boost'u (4 saniye)
⚡ Speed Chest: Koşma boost'u (4 saniye)
```

## 🎉 Sonuç

3.map artık tam multiplayer uyumlu! 
- ✅ Network spawn sistemi
- ✅ Multiplayer bomb game
- ✅ **Boost chest power-up sistemi**
- ✅ İthappy kamera ve hareket sistemi
- ✅ Karakter özelleştirmeleri korundu
- ✅ Animasyonlar korundu
- ✅ Geriye uyumluluk (offline mod hala çalışır)

**İyi oyunlar!** 🚀