# 📦 Multiplayer Boost Chest Sistemi

## 🎯 Sistem Özeti

Artık 3.map'te multiplayer uyumlu boost chest sistemi var!

**2 Farklı Yaklaşım:**
1. **NetworkChest.cs** - Yeni, server authoritative sistem (tavsiye edilen)
2. **BoostReceiver.cs** - Eski sistem, network uyumlu hale getirildi

## 🚀 YENİ SİSTEM: NetworkChest

### Özellikler
- ✅ Server authoritative (güvenli)
- ✅ NetworkVariable ile senkronizasyon
- ✅ Visual state management
- ✅ Pickup effects
- ✅ Automatic respawn
- ✅ Multiplayer güvenli

### Kurulum
```
1. Chest prefab'ı oluştur:
   - GameObject oluştur: "SpeedChest" veya "JumpChest"
   - NetworkObject component ekle
   - NetworkChest.cs script ekle
   - Collider (IsTrigger = true) ekle
   - Renderer/Model ekle

2. NetworkChest Inspector ayarları:
   - Chest Type: "speed" veya "jump"
   - Respawn Time: 3 (saniye)
   - Pickup Sound: Ses dosyası
   - Chest Model: 3D model GameObject
   - Pickup Effect: Effect prefab

3. Network Manager'a prefab ekle:
   - Network Manager → Network Prefabs List
   - Chest prefab'larını ekle
```

## 🔄 ESKİ SİSTEM: BoostReceiver (Güncellenmiş)

### Nasıl Çalışır
- Karakterde **BoostReceiver** component'i olmalı
- Chest objelerine **"JumpChest"** veya **"SpeedChest"** tag'i ver
- Collision detection ile chest'leri toplar

### Kurulum
```
1. Palyaco prefab'ına BoostReceiver.cs ekle
2. Inspector ayarları:
   - Boost Duration: 4 saniye
   - Chest Respawn Time: 3 saniye
   - Boosted Jump: 7f
   - Boosted Speed: 7f
   - Jump/Speed Boost Sound: Ses dosyaları

3. Map'te chest objelerine tag ekle:
   - JumpChest tag'i jump chest'lere
   - SpeedChest tag'i speed chest'lere
```

## 🆚 HANGİSİNİ KULLAN?

### NetworkChest (Tavsiye Edilen) ✅
```
+ Server authoritative (güvenli)
+ Visual feedback daha iyi
+ Pickup effects
+ Daha temiz kod
+ Scalable
```

### BoostReceiver (Eski Sistem) ⚠️
```
+ Hızlı setup
+ Mevcut chest'ler için uygun
- Client prediction yok
- Daha az güvenli
```

## 🎮 Kullanım

### NetworkChest
```
1. Chest'e yaklaş
2. Collision trigger olur
3. Server boost uygular
4. Chest kaybolur
5. Respawn time sonra geri gelir
```

### Boost Türleri
- **Speed Boost**: Koşma hızını artırır (4 saniye)
- **Jump Boost**: Zıplama yüksekliğini artırır (4 saniye)

## 🔧 Debug

### Loglar
- `📦 speed chest alındı - Client: X`
- `✨ speed chest respawn oldu!`
- `🦘 Jump boost alındı!`
- `⚡ Speed boost alındı!`

### Test
1. Multiplayer test başlat
2. Chest'e dokunmayı dene
3. Boost'un aktif olduğunu kontrol et
4. Respawn'ı bekle

## 📋 Prefab Checklist

### NetworkChest Prefab
```
✅ NetworkObject component
✅ NetworkChest.cs script
✅ Collider (IsTrigger = true)
✅ Renderer/3D Model
✅ AudioSource (opsiyonel)
✅ "Player" tag detection
```

### Oyuncu Prefab (Palyaco)
```
✅ BoostReceiver.cs component
✅ CharacterMover component (İthappy)
✅ NetworkObject component
✅ "Player" tag
```

## 🎯 Sonuç

**NetworkChest sistemi kullan!** 
- Daha güvenli
- Daha professional
- Multiplayer için optimize edilmiş
- Visual feedback dahil

Eski BoostReceiver sistemi de çalışır ama NetworkChest daha iyi. 🚀