# Post-transplantAR - Karaciğer Nakli Sonrası AR Eğitim Uygulaması

Bu proje, karaciğer nakli sonrası hastaya yalnızca AR destekli görsel eğitim sunmak için tasarlanmıştır.

## Proje Amacı

- Hastaya AR ile karaciğer anatomisini ve ameliyat sonrası süreci anlatıp hastayı bilgilendirmek amaçlanmaktadır. Hasta karaciğer resmine tıkladığında karaciğer hakkında detayları veren AR uygulaması geliştirilecektir.

## Kapsam

- Tek odak: AR eğitim deneyimi
- Model yerleştirme, döndürme, ölçekleme ve bilgi noktaları
- Android (ARCore) ve iOS (ARKit) desteği

## Teknoloji Yığını

- Unity (önerilen: 2022.3 LTS)
- AR Foundation
- ARCore XR Plugin (Android)
- ARKit XR Plugin (iOS)
- C#

## Güncel Proje Yapısı

```text
Assets/_Project/
├── Bootstrap/
│   ├── SceneBootstrap.cs
│   └── ARExperienceCoordinator.cs
└── Modules/
    ├── AR/
    │   └── Runtime/Controllers/
    │       ├── ARSessionController.cs
    │       ├── PlaneDetectionMonitor.cs
    │       └── ARPlacementController.cs
    ├── Interaction/
    │   └── Runtime/Input/
    │       ├── TapToPlaceInput.cs
    │       └── ModelManipulator.cs
    └── UI/
        └── Runtime/Screens/
            ├── ARSetupGuide.cs
            └── SafetyDisclaimerScreen.cs
```

## Modül Sorumlulukları

- `AR`:
  - `ARSessionController`: cihaz AR uyumluluğu kontrolü, oturum yaşam döngüsü, desteklenmeyen cihazda durum bildirimi (Availability)
  - `PlaneDetectionMonitor`: algılanan düzlemleri izler, yerleştirme uygunluğunu raporlar (Reliability)
  - `ARPlacementController`: raycast ile 3D model yerleştirme/yeniden konumlandırma, gating ve event'ler
- `Interaction`:
  - `TapToPlaceInput`: tek parmak dokunuşuyla yerleştirme
  - `ModelManipulator`: tek parmak döndürme, iki parmak (pinch) ölçekleme
- `UI`:
  - `ARSetupGuide`: durum/yönlendirme metinleri + kalıcı "tanı koymaz" güvenlik satırı
  - `SafetyDisclaimerScreen`: ilk kullanımda onay gerektiren sorumluluk reddi ekranı (Safety)
- `Bootstrap`:
  - `SceneBootstrap`: kare hızı, vSync ve ekran uyuma ayarları
  - `ARExperienceCoordinator`: modülleri birbirine bağlayan merkezi akış (Maintainability)

## Çalışma Akışı

1. `SafetyDisclaimerScreen` ilk açılışta güvenlik uyarısını gösterir ve onay ister.
2. `ARSessionController` cihaz uyumluluğunu kontrol eder; desteklenmiyorsa bilgilendirici fallback gösterilir.
3. `PlaneDetectionMonitor` düz bir yüzey bulunca yerleştirmeyi etkinleştirir.
4. Kullanıcı ekrana dokunur → `TapToPlaceInput` → `ARPlacementController` modeli yerleştirir.
5. `ModelManipulator` ile model döndürülüp ölçeklenebilir.
6. `ARSetupGuide` her adımda uygun yönlendirme metnini gösterir.
7. `ARExperienceCoordinator` tüm bu olayları koordine eder.

## Sahne Kurulumu (Inspector Bağlantıları)

Tek sahnede şu GameObject'leri kurup script'leri bağlayın:

- `XR Origin (AR)` üzerine: `ARRaycastManager`, `ARPlaneManager` (+ `PlaneDetectionMonitor`)
- `AR Session` üzerine: `ARSession` (+ `ARSessionController` referansı)
- Boş `Coordinator` objesi: `ARExperienceCoordinator` — tüm referansları buraya bağlayın
- Boş `Input` objesi: `TapToPlaceInput`, `ModelManipulator` (her ikisinde `ARPlacementController` referansı)
- Canvas: `ARSetupGuide` (statusText + safetyText) ve `SafetyDisclaimerScreen` (panel + buton)

## Kurulum

Detaylar için: `SETUP_ARFOUNDATION_ANDROID_TR.md`

Kısa özet:
- Unity Hub ile editor + Android/iOS modüllerini kur
- Projeyi Unity ile aç
- XR Plug-in Management içinde Android için ARCore, iOS için ARKit aç
- Sahneye `AR Session` ve `XR Origin (AR)` ekle
- `AR Plane Manager` ve `AR Raycast Manager` bileşenlerini bağla

## Not

Bu sürüm yalnızca AR eğitim kapsamındadır.
