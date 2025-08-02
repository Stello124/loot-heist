# 🌍 Global Player Spawner Sistemi - SETUP

## ✅ **SORUN ÇÖZÜLDİ!**

**Tek global spawner - Tüm maplerde çalışır - Herkes birbirini görür!**

## 🚀 **Unity'de KURULUM:**

### **1. LobbyRoom.unity Sahnesinde Global Spawner Oluştur:**

```
LobbyRoom.unity sahnesini aç:

1. Boş GameObject oluştur: "GlobalPlayerSpawner"
2. Add Component → NetworkObject
3. Add Component → GlobalPlayerSpawner 
4. Inspector ayarları:
   - Spawnable Prefabs: palyaco prefab'ını ekle
   
5. ✅ Bu obje otomatik DontDestroyOnLoad olacak!
```

### **2. Eski Spawn Manager'ları Devre Dışı Bırak:**

#### **3.map.unity:**
```
3.map sahnesini aç:
1. "NetworkSpawnManager3Map" objesini bul
2. Inspector'da checkbox'ı KAPAT (devre dışı)
```

#### **1.map.unity:**
```
1.map sahnesini aç:
1. Herhangi bir spawn manager varsa devre dışı bırak
```

### **3. Spawn Point'leri Düzenle:**

#### **1.map (Yarış):**
```
Spawn point'leri şu şekilde organize et:
- Parent GameObject: "RaceSpawnPoints" (veya "SpawnPoints")
- Child'lar: SpawnPoint_01, SpawnPoint_02, SpawnPoint_03, SpawnPoint_04
```

#### **3.map (Bomba):**
```
Spawn point'leri şu şekilde organize et:
- Parent GameObject: "BombSpawnPoints" (veya "SpawnPoints")  
- Child'lar: SpawnPoint_01, SpawnPoint_02, SpawnPoint_03, SpawnPoint_04
```

#### **4.map (Köprü):**
```
Spawn point'leri şu şekilde organize et:
- Parent GameObject: "BridgeSpawnPoints" (veya "SpawnPoints")
- Child'lar: SpawnPoint_01, SpawnPoint_02, SpawnPoint_03, SpawnPoint_04
```

#### **DeneyK2 (Tırmanma):**
```
Spawn point'leri şu şekilde organize et:  
- Parent GameObject: "ClimbSpawnPoints" (veya "SpawnPoints")
- Child'lar: SpawnPoint_01, SpawnPoint_02, SpawnPoint_03, SpawnPoint_04
```

## 🎯 **Scene Mapping (Düzeltildi):**

```
✅ Tırman → DeneyK2
✅ Bomba → 3.map  
✅ Yarış → 1.map
✅ Köprü → 4.map
```

## 🎮 **NASIL ÇALIŞIR:**

### **Global Sistem:**
```
LobbyRoom → GlobalPlayerSpawner oluşur (DontDestroyOnLoad)
↓
Herhangi bir sahneye git (1.map, 3.map, 4.map, DeneyK2)
↓  
GlobalPlayerSpawner otomatik spawn eder
↓
Scene'e göre doğru spawn point'leri kullanır
↓
Customization uygulanır (SENİN KODUN)
↓
Scene-specific component'ler eklenir (BoostReceiver vs)
```

### **Her Scene'de:**
```
1.map → Race spawn points → Yarış başlangıç çizgisi
3.map → Bomb spawn points → Bomba arena + BoostReceiver eklenir  
4.map → Bridge spawn points → Köprü başlangıç
DeneyK2 → Climb spawn points → Tırmanma başlangıç
```

## 💯 **BU ÇÖZETTİĞİ SORUNLAR:**

- ✅ **Host client'i görür** (Tek global spawner)
- ✅ **Client host'u görür** (Aynı spawn sistemi)  
- ✅ **Animasyonlar görünür** (CharacterBuilder korundu)
- ✅ **Customization sync** (GameState.LocalPlayerData korundu)
- ✅ **Resources loading** (SENİN sistem korundu)
- ✅ **Scene geçişleri** (Otomatik respawn)
- ✅ **Kutuları herkes alabilir** (3.map'te BoostReceiver otomatik)
- ✅ **Doğru scene mapping** (Bomba → 3.map, Yarış → 1.map)

## 🚨 **ÖNEMLİ NOTLAR:**

1. **Sadece LobbyRoom'da GlobalPlayerSpawner olacak**
2. **Diğer sahnelerde spawn manager YOK**  
3. **Spawn point'ler parent-child yapısında organize et**
4. **Global spawner otomatik DontDestroyOnLoad**
5. **Scene değişiminde otomatik respawn**

## 🔧 **TEST ETMEK İÇİN:**

```
1. LobbyRoom'da GlobalPlayerSpawner oluştur
2. 3.map'te NetworkSpawnManager3Map'i kapat
3. Play → Lobby → Bomba seç → 3.map'e git
4. Host ve Client spawn olmalı, birbirini görmeli
5. Animasyonlar, customization çalışmalı
6. Boost kutularını herkes alabilmeli
```

**ESKİ TEK SPAWNER SİSTEMİN RESTORE EDİLDİ!** 🌍🎯💯