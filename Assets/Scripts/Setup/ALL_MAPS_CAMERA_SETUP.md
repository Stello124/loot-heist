# 🎥 TÜM MAPLERDE KAMERA KURULUMU

## 🎯 SORUN
Her mapte "Main Camera" var → MovePlayerInput buna bağlanıyor → Tek kamera problemi

## ✅ ÇÖZÜM (Tüm Maplerde Yap)

### **1.map (Yarış):**
```
1.map.unity aç:
1. Main Camera'yı sil/kapat
2. ThirdPersonCamera ekle (ithappy)
3. Tag: MainCamera
4. Position: Yarış start çizgisinin arkası
```

### **3.map (Bomba):**
```
3.map.unity aç:
✅ Zaten yapıldı (NetworkGameStartManager var)
✅ ThirdPersonCamera mevcut olmalı
```

### **4.map (Köprü):**
```
4.map.unity aç:
1. ❌ Main Camera'yı sil/kapat
2. ✅ ThirdPersonCamera ekle (ithappy)  
3. ✅ Tag: MainCamera
4. ✅ Position: Köprü başlangıcının arkası
```

### **DeneyK2 (Tırmanma):**
```
DeneyK2.unity aç:
1. Main Camera'yı sil/kapat
2. ThirdPersonCamera ekle (ithappy)
3. Tag: MainCamera
4. Position: Tırmanma duvarının arkası
```

## 🎮 TEK YAPILACAK:
```
Her sahneye sadece 1 tane ThirdPersonCamera ekle
↓
GlobalPlayerSpawner otomatik player'ları spawn eder
↓  
MovePlayerInput ThirdPersonCamera'yı bulur
↓
Her player kendi kamerasıyla oynar
```

## 💯 SONUÇ:
- ✅ **Host kendi kamerasıyla oynar**
- ✅ **Client kendi kamerasıyla oynar**  
- ✅ **Kontrol edilebilir karakterler**
- ✅ **Mouse ile kamera dönme**
- ✅ **ESC ile cursor toggle**

## 🚨 NOT:
**Multiplayer'da sadece owner kendi kamerasını kontrol eder!**
Bu ithappy'nin NetworkBehaviour tabanlı MovePlayerInput sayesinde otomatik.