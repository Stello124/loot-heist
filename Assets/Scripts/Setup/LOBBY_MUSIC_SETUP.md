# 🎵 Lobby Musik Sistemi Kurulum Rehberi

## 🎯 Sistem Özeti

LobbyBrowserScene ve LobbyRoom sahneleri arasında sürekli çalan müzik sistemi.

**Özellikler:**
- ✅ DontDestroyOnLoad - Sahne geçişlerinde müzik devam eder
- ✅ Lobby sahneleri: LobbyBrowserScene, LobbyRoom
- ✅ Oyun sahnelerinde otomatik durur: 1.map, 2.map, 3.map, 4.map
- ✅ Fade In/Out efektleri
- ✅ Volume kontrolü
- ✅ Loop şeklinde çalar

## 🚀 KURULUM TALİMATLARI

### 1. LobbyBrowserScene Kurulumu

#### A) LobbyMusicManager GameObject Oluştur
```
1. LobbyBrowserScene sahnesini Unity'de aç
2. Boş GameObject oluştur: "LobbyMusicManager"
3. Add Component → LobbyMusicManager.cs script'ini ekle
4. Add Component → AudioSource (otomatik eklenir)
```

#### B) Inspector Ayarları
```
LobbyMusicManager component'inde:
- Lobby Music: Music/AnaMenu/game-gaming-minecraft-background-music-377647 mp3'ünü sürükle
- Volume: 0.7 (istediğin ses seviyesi)
- Fade In Out: ✅ (yumuşak geçişler için)
- Fade Duration: 1.5 (saniye)
```

### 2. AudioSource Ayarları (Otomatik)
```
LobbyMusicManager scripti otomatik olarak şu ayarları yapar:
✅ Loop: true
✅ Play On Awake: false  
✅ Spatial Blend: 0 (2D ses)
✅ Volume: Inspector'daki değer
```

### 3. Test Etme

#### Inspector'da Test Buttonları
```
LobbyMusicManager component'inde:
- Right click → Test Start Music
- Right click → Test Stop Music
```

#### Runtime Test
```
1. LobbyBrowserScene'i play et
   → Müzik başlamalı (fade-in ile)

2. Scene geçişi simülasyonu:
   → 1.map sahnesine geç → Müzik durmalı
   → LobbyRoom sahnesine geç → Müzik başlamalı
```

## 🎮 ÇALIŞMA MANTIĞI

### Sahne Geçiş Akışı
```
LobbyBrowserScene → LobbyRoom
     🎵 MÜZİK DEVAM EDER 🎵

LobbyRoom → 1.map (veya diğer oyun sahneleri)  
     🔇 MÜZİK DURUR

1.map → LobbyBrowserScene/LobbyRoom
     🎵 MÜZİK YENİDEN BAŞLAR
```

### Singleton Sistemi
```
- Sadece 1 LobbyMusicManager instance olur
- DontDestroyOnLoad ile sahneler arası kalır
- Yeni sahneye duplicate gelirse kendini yok eder
```

### Scene Detection
```
Lobby Sahneleri: ["LobbyBrowserScene", "LobbyRoom"]
Oyun Sahneleri: ["1.map", "2.map", "3.map", "4.map"]

OnSceneLoaded event'inde:
→ Lobby sahnesiyse: StartLobbyMusic()
→ Oyun sahnesiyse: StopLobbyMusic()
```

## 🔧 ADVANCED AYARLAR

### Volume Kontrolü
```csharp
// Runtime'da ses seviyesi değiştir
LobbyMusicManager.Instance.SetVolume(0.5f);
```

### Manuel Kontrol
```csharp
// Müziği duraklat (pause)
LobbyMusicManager.Instance.PauseLobbyMusic();

// Müziği devam ettir
LobbyMusicManager.Instance.ResumeLobbyMusic();

// Tamamen durdur
LobbyMusicManager.Instance.StopLobbyMusic();

// Yeniden başlat
LobbyMusicManager.Instance.StartLobbyMusic();
```

### Yeni Sahne Eklemek
```csharp
// LobbyMusicManager.cs içinde:

// Lobby sahne eklemek için:
private readonly string[] lobbyScenes = { 
    "LobbyBrowserScene", 
    "LobbyRoom", 
    "NewLobbyScene"  // ← Yeni sahne
};

// Oyun sahne eklemek için:
private readonly string[] gameScenes = { 
    "1.map", "2.map", "3.map", "4.map", 
    "5.map"  // ← Yeni oyun sahnesi
};
```

## 🐛 SORUN GİDERME

### Müzik Çalmıyor?
```
✅ LobbyMusicManager GameObject aktif mi?
✅ AudioSource component var mı?
✅ Lobby Music dosyası atanmış mı?
✅ Volume > 0 mı?
✅ Audio Listener sahnede var mı?
```

### Müzik Durmuyor?
```
✅ Scene ismi doğru yazılmış mı? (Case sensitive)
✅ gameScenes array'inde sahne ismi var mı?
✅ OnSceneLoaded event çalışıyor mu? (Console logları kontrol et)
```

### Duplicate Sound?
```
✅ LobbyBrowserScene'de sadece 1 tane LobbyMusicManager var mı?
✅ Singleton pattern çalışıyor mu?
✅ Console'da "Duplicate destroyed" mesajı var mı?
```

### Fade Çalışmıyor?
```
✅ Fade In Out = true mi?
✅ Fade Duration > 0 mı?
✅ AudioSource volume'u runtime'da değişiyor mu?
```

## 📊 DEBUG LOGLARI

Şu mesajları bekle:
```
🎵 LobbyMusicManager oluşturuldu - DontDestroyOnLoad aktif
🔊 AudioSource hazırlandı - Loop: ON, Volume: 0.7
🌍 Scene yüklendi: LobbyBrowserScene
🎵 Lobby müziği başlatıldı: game-gaming-minecraft-background-music-377647
🎵 Fade-in tamamlandı
🌍 Scene yüklendi: 1.map
🔇 Lobby müziği durduruldu
🔇 Fade-out tamamlandı
```

## 🎉 SONUÇ

Artık:
- ✅ LobbyBrowserScene'de müzik başlar
- ✅ LobbyRoom'a geçince müzik devam eder
- ✅ Oyun sahnelerine geçince durur
- ✅ Lobby'e dönünce tekrar başlar
- ✅ Yumuşak fade geçişleri
- ✅ Tüm sistem otomatik çalışır

**MÜZİKLİ LOBBY DENEYİMİ HAZIR! 🎵🚀**