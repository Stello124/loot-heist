# 🟡 4.Map Platform Multiplayer Kurulum Rehberi

## 🎯 Platform Multiplayer Senkronizasyonu

### ✅ Yapılan Değişiklikler:

#### 1. **InstantBreakPlatform.cs** (Anında Kırılan Platformlar)
- **NetworkBehaviour**'a çevrildi
- **NetworkVariable<bool> isBroken** eklendi
- **ServerRpc/ClientRpc** ile senkronizasyon
- Host bastığında tüm client'lar görür
- Client bastığında host görür

#### 2. **BreakablePlatform.cs** (1.map'teki Zamanlı Kırılan Platformlar)
- **NetworkBehaviour**'a çevrildi
- **NetworkVariable<bool> isBreaking** ve **isBroken** eklendi
- **ServerRpc/ClientRpc** ile senkronizasyon
- Yanıp sönme ve kırılma tüm client'larda senkronize

#### 3. **PlatformSpawner.cs** (Platform Oluşturucu)
- **NetworkBehaviour**'a çevrildi
- Platformları **NetworkObject** olarak spawn eder
- Sadece **Server** platformları oluşturur

## 🔧 Unity'de Kurulum:

### **ADIM 1: Platform Prefab'larını Hazırla**

#### A) Normal Platform Prefab:
```
1. Normal platform prefab'ını seç
2. Add Component → Network Object
3. Add Component → OneTimeBreakPlatform (eğer kırılacaksa)
4. Inspector'da:
   - Platform To Destroy: Platform'un görsel objesini ata
```

#### B) Instant Break Platform Prefab:
```
1. Instant break platform prefab'ını seç
2. Add Component → Network Object
3. Add Component → OneTimeBreakPlatform
4. Inspector'da:
   - Platform To Destroy: Platform'un görsel objesini ata
```

#### C) Timed Respawn Platform Prefab:
```
1. Timed respawn platform prefab'ını seç
2. Add Component → Network Object
3. Add Component → BreakablePlatform
4. Inspector'da:
   - Platform To Disable: Platform'un görsel objesini ata
   - Warning Duration: 1.5
   - Blink Interval: 0.1
   - Reappear Delay: 3
```

### **ADIM 2: PlatformSpawner'ı Ayarla**

#### A) PlatformSpawner GameObject:
```
1. 4.map sahnesinde PlatformSpawner'ı bul
2. Add Component → Network Object
3. Inspector'da PlatformSpawner (Script):
   - Normal Platform Prefab: Normal platform prefab'ını ata
   - Instant Break Platform Prefab: Instant break prefab'ını ata
   - Timed Respawn Platform Prefab: Timed respawn prefab'ını ata
   - Step Count: 20
   - Step Spacing: 2.5
   - Lane Spacing: 2
```

### **ADIM 3: NetworkManager'da Prefab'ları Kaydet**

#### A) NetworkManager Ayarları:
```
1. NetworkManager'ı seç
2. Inspector'da NetworkManager (Script):
   - Network Prefabs → Spawnable Prefabs listesine:
     * Normal platform prefab'ını ekle
     * Instant break platform prefab'ını ekle
     * Timed respawn platform prefab'ını ekle
```

## 🎮 Test Senaryoları:

### **Test 1: Host Platform Kırma**
```
1. Host olarak oyuna gir
2. Bir platforma bas
3. Platform kırılsın
4. Client'ın da platformun kırıldığını görmesi gerekir
```

### **Test 2: Client Platform Kırma**
```
1. Client olarak oyuna gir
2. Bir platforma bas
3. Platform kırılsın
4. Host'un da platformun kırıldığını görmesi gerekir
```

### **Test 3: Zamanlı Platform**
```
1. Zamanlı kırılan platforma bas
2. Platform yanıp sönsün (tüm client'larda)
3. Platform kırılsın (tüm client'larda)
4. 3 saniye sonra yeniden oluşsun (tüm client'larda)
```

## 🐛 Sorun Giderme:

### **Platform Kırılmıyor:**
```
❌ Platform prefab'ında NetworkObject yok
✅ Platform prefab'ına NetworkObject ekle

❌ Platform prefab'ı NetworkManager'da kayıtlı değil
✅ NetworkManager → Spawnable Prefabs'e ekle

❌ OneTimeBreakPlatform/BreakablePlatform component'i yok
✅ Platform prefab'ına ilgili component'i ekle
```

### **Platform Sadece Bir Client'ta Kırılıyor:**
```
❌ Platform prefab'ında NetworkObject yok
✅ Platform prefab'ına NetworkObject ekle

❌ PlatformSpawner NetworkObject olarak spawn etmiyor
✅ PlatformSpawner'da NetworkObject.Spawn() çağrıldığından emin ol
```

### **Platform Yeniden Oluşmuyor:**
```
❌ BreakablePlatform'da Respawn sistemi çalışmıyor
✅ Reappear Delay değerini kontrol et
✅ Server'da RespawnPlatformClientRpc çağrıldığından emin ol
```

## 📝 Debug Logları:

### **Başarılı Platform Kırma:**
```
💥 Platform kırılıyor: Platform_1 - Player: Player1
💥 Platform kırıldı (Client): Platform_1
```

### **Başarılı Zamanlı Platform:**
```
💥 BreakablePlatform tetiklendi: Platform_2 - Player: Player1
💥 BreakablePlatform yanıp sönmeye başladı: Platform_2
💥 BreakablePlatform kırıldı: Platform_2
🔄 BreakablePlatform yeniden oluştu: Platform_2
```

## ✅ Sonuç:

Artık 4.map'teki platformlar tamamen multiplayer senkronize:
- **Host** platforma bastığında **Client** görür
- **Client** platforma bastığında **Host** görür
- **Zamanlı platformlar** tüm client'larda senkronize çalışır
- **Platform spawn** sistemi server tarafından yönetilir

🎮 **Test et ve platformların multiplayer'da düzgün çalıştığını doğrula!** 