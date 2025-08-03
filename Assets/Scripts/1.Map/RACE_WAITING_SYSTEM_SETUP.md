# ⏰ 1.Map Bekleme ve Countdown Sistemi - KURULUM

## 🎯 YENİ OYUN AKIŞI:

```
1. Host/Client oyuna girer → Karakterler DONUK kalır ❄️
2. 30 saniye bekle diğer oyuncuları ⏰
3. Herkes geldi → 8 saniye countdown 🚀
4. Countdown bitti → Karakterler hareket edebilir! 🏃‍♂️
5. İlk bitiren → Kazanır, herkes donuyor 🏆
```

## 🔧 UNITY'DE KURULUM:

### **1. Eski RaceGameManager'ı Devre Dışı Bırak:**
```
1.map.unity'de:
1. "RaceGameManager" objesini bul
2. Inspector'da checkbox'ı KAPAT (devre dışı)
```

### **2. RaceWaitingManager Oluştur:**
```
1. Boş GameObject: "RaceWaitingManager"  
2. Add Component: NetworkObject
3. Add Component: RaceWaitingManager
4. Inspector ayarları:
   ✅ Waiting Time = 30 (saniye)
   ✅ Countdown Time = 8 (saniye)  
   ✅ Race UI = RaceCanvas'taki RaceUI component'ini bağla
```

## 🎮 ÇALIŞMA AKIŞI:

### **Phase 1: Waiting (30 saniye)**
```
Ekran: "⏰ OYUNCULAR BEKLENİYOR
       2/4 Oyuncu Hazır  
       25 saniye kaldı"
       
Karakterler: DONUK ❄️ (hareket edemez)
```

### **Phase 2: Countdown (8 saniye)**  
```
Ekran: "🚀 YARIŞ BAŞLIYOR!
       3"
       
Karakterler: Hala DONUK ❄️
```

### **Phase 3: Game Active**
```
Ekran: Panel kaybolur
Karakterler: SERBEST 🏃‍♂️ (hareket edebilir)
```

### **Phase 4: Game Ended**
```
Ekran: "🏆 [Kazanan] OYUNU KAZANDI!"
Karakterler: Tekrar DONUK ❄️
```

## 💻 KODDA NELER OLUYOR:

### **Player Freeze/Unfreeze:**
```csharp
// Karakterleri dondur
CharacterMover.enabled = false;
MovePlayerInput.enabled = false;

// Karakterleri serbest bırak  
CharacterMover.enabled = true;
MovePlayerInput.enabled = true;
```

### **Network Sync:**
```csharp
NetworkVariable<GameState> currentGameState
NetworkVariable<float> waitingTimer  
NetworkVariable<float> countdownTimer
NetworkList<ulong> joinedPlayers
```

## 🧪 TEST ETMEK İÇİN:

### **Console'da beklenen mesajlar:**
```
⏰ "Oyuncular bekleniyor - Timer başladı"
👤 "Player joined: 0 (1/4)"  
🚀 "Countdown başladı"
🏁 "Oyun başladı - Oyuncular hareket edebilir!"
🧊 "Oyuncular donduruldu/serbest bırakıldı"
```

### **UI'de görülecekler:**
```
1. "⏰ OYUNCULAR BEKLENİYOR" paneli
2. Countdown: "🚀 YARIŞ BAŞLIYOR! 3"
3. Panel kaybolur, hareket edebilir
4. Bitiş çizgisinde kazanan paneli
```

## 🚨 ÖNEMLİ NOTLAR:

### **Player Count Logic:**
```
- Expected count = LobbyRoom'daki kişi sayısı (şimdilik connected clients)
- Herkes geldi = Countdown başlar
- 30 saniye doldu = Mevcut oyuncularla başlar
```

### **Freeze Logic:**
```
- Sadece CharacterMover ve MovePlayerInput disable edilir
- Kamera çalışmaya devam eder
- NetworkObject etkilenmez
```

### **Finish Line:**
```
- Sadece GameState.GameActive'ken kabul edilir
- Bekleme/Countdown sırasında göz ardı edilir
```

## 💯 SONUÇ:

- ✅ **30 saniye waiting** → Oyuncular bekler
- ✅ **8 saniye countdown** → Hazırlanır  
- ✅ **Game active** → Yarış başlar
- ✅ **Player freeze/unfreeze** → Kontrollü hareket
- ✅ **Network sync** → Herkeste aynı anda
- ✅ **UI feedback** → Anlık durum gösterimi

**ESKİ RACEMANAGER'I KAPAT, YENİSİNİ KUR, TEST ET!** ⏰🎯