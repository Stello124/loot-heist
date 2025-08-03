# 🟡 4.Map Platform Race - DETAYLI KURULUM REHBERİ

## 🎯 NE YAPACAĞIZ?

4.map'e 1.map'teki aynı bekleme sistemi + platform race:
- ⏰ Oyuncular 30 saniye bekler
- 🚀 8 saniye countdown
- 🏁 Platform race başlar
- 🔄 Düşen başa döner (yok olmaz!)
- 🏆 İlk bitiren kazanır

## 🔧 ADIM ADIM KURULUM

### **ADIM 1: 4.map Sahnesini Aç**
```
Unity'de:
1. File → Open Scene
2. Scene/4.map.unity'yi seç
3. Sahne yüklensin
```

### **ADIM 2: PlatformGameManager Oluştur**

#### A) GameObject Oluştur:
```
1. Hierarchy'de sağ tık
2. Create Empty
3. İsim: "PlatformGameManager"
```

#### B) Network Component'leri Ekle:
```
1. PlatformGameManager'ı seç
2. Inspector'da Add Component
3. Network Object ekle
4. Add Component tekrar
5. PlatformGameManager script'ini ekle
```

#### C) Inspector Ayarları:
```
PlatformGameManager (Script):
├── Waiting Time: 30
├── Countdown Time: 8
├── Platform UI: (Şimdilik boş bırak - ADIM 3'te atayacağız)
├── Last Player Wins: ❌ FALSE (ÖNEMLİ!)
└── Finish Area: (Boş bırak)
```

### **ADIM 3: PlatformUI Oluştur**

#### A) GameObject Oluştur:
```
1. Hierarchy'de sağ tık
2. Create Empty  
3. İsim: "PlatformUI"
```

#### B) Script Ekle:
```
1. PlatformUI'yi seç
2. Inspector'da Add Component
3. PlatformUI script'ini ekle
4. Script otomatik UI oluşturacak!
```

#### C) PlatformGameManager'a Bağla:
```
1. PlatformGameManager'ı seç
2. Inspector'da Platform UI alanını bul
3. Hierarchy'den PlatformUI'yi sürükle
4. Platform UI alanına bırak
```

### **ADIM 4: Bitiş Çizgisi Oluştur (ÖNEMLİ!)**

#### A) Bitiş Alanı GameObject'i:
```
1. Platform'ların sonunda (bitiş noktası) sağ tık
2. 3D Object → Cube
3. İsim: "FinishLineTrigger"
4. Position: Bitiş noktası koordinatları
5. Scale: X=10, Y=5, Z=2 (geniş olsun)
```

#### B) Trigger Ayarları:
```
1. FinishLineTrigger'ı seç
2. Inspector'da Box Collider bul
3. Is Trigger: ✅ TIKLA (ÖNEMLİ!)
4. MeshRenderer'ı kapat (görünmez olsun)
```

#### C) Network Component'leri:
```
1. Add Component → Network Object
2. Add Component → PlatformFinishTrigger
3. Inspector'da:
   - Is Finish Line: ✅ TRUE
   - Finish Area Effect: (opsiyonel)
   - Finish Sound: (opsiyonel)
```

### **ADIM 5: Düşme Bölgesi Oluştur (ÇOK ÖNEMLİ!)**

#### A) Düşme Alanı GameObject'i:
```
1. Platform'ların altında (çok aşağıda) sağ tık
2. 3D Object → Cube  
3. İsim: "FallTrigger"
4. Position: Y = -50 (platform'ların çok altında)
5. Scale: X=200, Y=10, Z=200 (çok büyük olsun)
```

#### B) Trigger Ayarları:
```
1. FallTrigger'ı seç
2. Inspector'da Box Collider bul
3. Is Trigger: ✅ TIKLA (ZORUNLU!)
4. MeshRenderer'ı kapat (görünmez olsun)
```

#### C) Network Component'leri:
```
1. Add Component → Network Object
2. Add Component → PlatformRespawnTrigger
```

#### D) Respawn Ayarları (KRITIK!):
```
PlatformRespawnTrigger (Script):
├── Eliminate On Fall: ❌ FALSE (düşen başa dönsün!)
├── Respawn Delay: 1 (saniye)
├── Respawn Points: (ADIM 6'da ayarlayacağız)
├── Fall Sound: (opsiyonel)
└── Fall Effect: (opsiyonel)
```

### **ADIM 6: Respawn Points Oluştur**

#### A) Başlangıç Noktalarını Bul:
```
Sahneyi gez ve başlangıç platform'larını not et.
Oyuncuların spawn olduğu yerlerin koordinatları lazım.
```

#### B) Respawn Point'leri Oluştur:
```
Her başlangıç noktası için:
1. Hierarchy'de sağ tık
2. Create Empty
3. İsim: "RespawnPoint1", "RespawnPoint2", vb.
4. Position: Başlangıç platform'larının üzerine koy
   Örnek: X=0, Y=5, Z=0 (platform'un 2-3 unit üzerinde)
```

#### C) FallTrigger'a Respawn Points'leri Bağla:
```
1. FallTrigger'ı seç
2. Inspector'da PlatformRespawnTrigger script'ini bul
3. Respawn Points array'ini genişlet
4. Size: 4 (kaç respawn point'in varsa)
5. Element 0-3'e RespawnPoint1, 2, 3, 4'ü sürükle
```

### **ADIM 7: Player Tag Kontrolü**

#### A) Player Prefab'ını Kontrol Et:
```
1. Resources/Characters/palyaco prefab'ını aç
2. Inspector'da Tag'i kontrol et
3. Tag: "Player" olmalı (yoksa ayarla)
```

### **ADIM 8: Test Hazırlığı**

#### A) Sahne Kontrolü:
```
Hierarchy'de şunlar olmalı:
├── PlatformGameManager (NetworkObject + Script)
├── PlatformUI (Script)
├── FinishLineTrigger (NetworkObject + Trigger + Script)
├── FallTrigger (NetworkObject + Trigger + Script)
├── RespawnPoint1 (Empty GameObject)
├── RespawnPoint2 (Empty GameObject)
├── RespawnPoint3 (Empty GameObject)
└── RespawnPoint4 (Empty GameObject)
```

#### B) Kritik Ayarlar Kontrolü:
```
✅ PlatformGameManager → Last Player Wins = FALSE
✅ FinishLineTrigger → Is Trigger = TRUE
✅ FinishLineTrigger → Is Finish Line = TRUE
✅ FallTrigger → Is Trigger = TRUE
✅ FallTrigger → Eliminate On Fall = FALSE
✅ FallTrigger → Respawn Points dolu
✅ Player prefab → Tag = "Player"
```

## 🧪 TEST ETME

### **1. Play Mode Test:**
```
1. Unity'de Play bas
2. Console'da şu mesajları bekle:
   - "🟡 PlatformGameManager başlatıldı (4.map)"
   - "📺 ShowWaitingUI çağrıldı (4.map)"
   
3. UI'da şunu görmeli:
   - "Oyuncular Bekleniyor... Platform Race"
```

### **2. Düşme Test:**
```
1. Karakteri platform'dan düşür
2. FallTrigger'a çarptığında:
   - Console: "🟡 Player düştü - başa dönüyor"
   - Karakter yok olmamalı!
   - 1 saniye sonra respawn point'e dönmeli
```

### **3. Bitiş Test:**
```
1. Karakteri FinishLineTrigger'a götür
2. Console: "🏆 Player finished"
3. Kazanan UI görünmeli
```

## 🐛 SORUN GİDERME

### **Düşen Karakter Yok Oluyor:**
```
❌ Eliminate On Fall = TRUE olabilir
✅ FallTrigger → Eliminate On Fall = FALSE yap

❌ Respawn Points boş olabilir  
✅ Respawn Points array'ini doldur

❌ Player Tag yanlış olabilir
✅ Player prefab Tag = "Player" olmalı

❌ Is Trigger kapalı olabilir
✅ FallTrigger → Is Trigger = TRUE olmalı
```

### **Oyun Başlamıyor:**
```
❌ NetworkObject eksik olabilir
✅ PlatformGameManager'da NetworkObject var mı?

❌ UI bağlanmamış olabilir
✅ Platform UI alanı dolu mu?
```

### **Bitiş Çizgisi Çalışmıyor:**
```
❌ Is Finish Line = FALSE olabilir
✅ FinishLineTrigger → Is Finish Line = TRUE

❌ Trigger kapalı olabilir  
✅ FinishLineTrigger → Is Trigger = TRUE
```

## 📍 RESPAWN POINT YERLEŞTİRME

### **Doğru Yerleştirme:**
```
1. Platform'ların ÜZERİNE koy (içine değil!)
2. Y koordinatı platform'dan 2-3 unit yukarıda
3. Oyuncunun güvenle durabileceği yerde
4. Platform kenarından uzak

Örnek koordinat:
Platform Y=0 ise → RespawnPoint Y=3
```

### **Görselleştirme:**
```
Respawn Point'leri test etmek için:
1. RespawnPoint'i seç
2. Inspector'da Transform bul  
3. Position'ı kopyala
4. Character'i manuel o pozisyona koy
5. Güvenli mi kontrol et
```

## 🎯 FINAL KONTROL LİSTESİ

Kurulum tamamlandıktan sonra kontrol et:

```
✅ PlatformGameManager var ve NetworkObject'i var
✅ PlatformUI var ve PlatformGameManager'a bağlı
✅ FinishLineTrigger var, trigger aktif, Is Finish Line = true
✅ FallTrigger var, trigger aktif, Eliminate On Fall = false
✅ 4 adet RespawnPoint var ve FallTrigger'a bağlı
✅ RespawnPoint'ler platform'ların üzerinde
✅ Player prefab'ının tag'i "Player"
✅ Last Player Wins = false
```

## 🚀 ÇALIŞMA AKIŞI

Kurulum tamamlandıktan sonra:

```
1. Oyuna gir → 30 saniye bekleme
2. Countdown → "PLATFORM RACE BAŞLIYOR!"  
3. Hareket et → Platform'larda yarış
4. Düş → 1 saniye sonra başa dön
5. Bitiş çizgisine ulaş → KAZAN!
```

**BU REHBERİ TAKİP ET, DÜŞEN KARAKTER BAŞA DÖNECEK!** 🔄🏁