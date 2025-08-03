# 🧗 DeneyK2 Spawn Point Düzeltmesi

## 🚨 SORUN:
- GlobalPlayerSpawner "ClimbSpawnPoints" arıyor
- DeneyK2.unity'de "ClimbSpawnPoints" yok
- Eski "PlayerSpawner" var

## ✅ ÇÖZÜM (Unity'de yap):

### **DeneyK2.unity sahnesinde:**

```
1. Eski PlayerSpawner'ı sil veya devre dışı bırak

2. Yeni parent oluştur:
   - GameObject: "ClimbSpawnPoints"
   - Position: Tırmanma duvarının önü

3. Child spawn point'ler ekle:
   - "SpawnPoint_01" (Player 1 için)
   - "SpawnPoint_02" (Player 2 için)  
   - "SpawnPoint_03" (Player 3 için)
   - "SpawnPoint_04" (Player 4 için)

4. Position'ları tırmanma başlangıç noktalarına koy
```

### **Hızlı Kurulum:**
```
ClimbSpawnPoints (Parent)
├── SpawnPoint_01 (0, 0, 0)
├── SpawnPoint_02 (2, 0, 0)  
├── SpawnPoint_03 (4, 0, 0)
└── SpawnPoint_04 (6, 0, 0)
```

## 🎯 SONUÇ:
- ✅ GlobalPlayerSpawner "ClimbSpawnPoints" bulacak
- ✅ Player'lar sırayla spawn olacak
- ✅ DeneyK2 çalışacak