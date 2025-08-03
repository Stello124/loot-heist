# 🎥 Kamera Başlangıç Pozisyonu Düzeltmesi

## 🚨 SORUN:
Kamera oyun başlayınca 0,0,0'a gidiyor, sonra player'a geçiyor

## ✅ BASIT ÇÖZÜM:

### **Her sahnede Main Camera pozisyonunu spawn noktalarına yakın koy:**

```
1.map → Main Camera'yı yarış çizgisinin arkasına koy
3.map → Main Camera'yı bomba arena'nın ortasına koy  
4.map → Main Camera'yı köprü başlangıcına koy
DeneyK2 → Main Camera'yı tırmanma duvarının önüne koy
```

### **Alternatif: CameraStartFix Script (Advanced):**
```
Main Camera'ya CameraStartFix.cs ekle:
- Player atanana kadar kamerayı sabit tutar
- Player spawn olunca ThirdPersonCamera'yı aktif eder
- Hareket etmesini önler
```

## 🎯 SONUÇ:
- ✅ Kamera doğru pozisyonda başlar
- ✅ Player spawn olunca düzgün geçiş yapar
- ✅ 0,0,0 sorunu çözülür