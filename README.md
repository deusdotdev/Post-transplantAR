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
    ├── Simulation/
    │   └── Runtime/ (SimulationState, SimulationController)
    ├── Visuals/
    │   └── Runtime/ (LiverVisualController, LiverMeshGenerator)
    └── UI/
        └── Runtime/
            ├── Theme/UITheme.cs
            └── Screens/
                ├── ARSetupGuide.cs
                ├── SafetyDisclaimerScreen.cs
                ├── AnatomyIntroScreen.cs
                ├── EducationFlowController.cs
                ├── ClinicalDashboard.cs
                ├── ScenarioHUD.cs
                └── LiverAnatomyInfoPanel.cs
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
- `Simulation` / `Visuals` / `UI`:
  - `EducationFlowController`: güvenlik uyarısı → anatomi girişi → eğitim paneli
  - `ClinicalDashboard`: sağlık çubuğu, klinik değerler, uyarı bandı, red detay kutuları
  - `LiverAnatomyInfoPanel`: sağ/sol lob ve safra bilgi kartları (README bilgi noktaları)
  - `ScenarioHUD`: dört senaryo ve aksiyon butonları

## Çalışma Akışı

1. `SafetyDisclaimerScreen` güvenlik uyarısını gösterir (RAMS Safety).
2. `AnatomyIntroScreen` karaciğer işlevlerine kısa giriş sunar.
3. `ARSessionController` cihaz uyumluluğunu kontrol eder.
4. `PlaneDetectionMonitor` düz yüzey bulunca yerleştirmeyi açar.
5. Kullanıcı dokunur → model AR'de yerleşir; `LiverVisualController` senaryo verisine göre görünümü günceller.
6. `EducationMenuController`: **ana menü → senaryo ekranı** (referans FlowManager gibi tek panel aktif); anatomi ayrı alt menü.
7. Senaryo ekranında yalnızca o senaryoya ait aksiyonlar + klinik dashboard + «Ana menü».
8. `ModelManipulator` ile döndürme/ölçekleme; `ARSetupGuide` üst durum şeridi.

## Simülasyon ve Görsel Katman (Senaryo Sistemi)

LiverTransplantAR örneğinden esinlenilen, veri-merkezli senaryo sistemi:

- `Modules/Simulation/Runtime/Data/SimulationState.cs` — `ScriptableObject` tek doğruluk kaynağı (büyüme, sağlık, AST/ALT/Bilirubin, ilaç uyumu)
- `Modules/Simulation/Runtime/SimulationController.cs` — senaryo mantığı; **event-driven** (`StateChanged`), her frame string üretmez
- `Modules/Visuals/Runtime/LiverVisualController.cs` — veriyi görsele bağlar (büyüme→ölçek, bilirubin→sararma, bağışıklık→şişme, fibrozis→koyulaşma, yağlı diyet→steatoz tonu). `MaterialPropertyBlock` kullanır, özel shader gerektirmez
- `Modules/Visuals/Runtime/LiverMeshGenerator.cs` — karaciğeri **koddan üretir** (iki loblu); dış 3B model/lisans gerektirmez
- `Modules/UI/Runtime/Screens/ScenarioHUD.cs` — başlık/açıklama/klinik metin + buton aksiyonları

Senaryolar:
- **Onarım** — haftalık rejenerasyon (büyüme + klinik değerlerin normalleşmesi)
- **İlaç Uyumu** — düzenli/aksatma dallanması (aksatınca bağışıklık saldırısı, sararma, şişme)
- **Red / Rejeksiyon** — aşamalı süreç (erken → akut → kronik): damar tıkanıklığı, fibrozis, belirgin sararma
- **Yaşam Tarzı** — sağlıklı beslenme/egzersiz vs. yağlı diyet (steatoz/yağlanma) karşılaştırması

## Editor Komutları (otomatik kurulum)

Üst menü `Post-transplantAR`:

- **Build AR Scene** — gerçek AR sahnesi (telefonda test için)
- **Build Simulation Preview (No AR)** — AR olmadan, Editor'de Play ile test edilebilen senaryo önizlemesi (procedural karaciğer + butonlu HUD). Telefon gerekmez.

Her iki komut da gerektiğinde şu asset'leri otomatik üretir: `SimulationState.asset`, `Materials/LiverMaterial.mat`, `Prefabs/LiverModel.prefab`.

## 3B Model

İki seçenek desteklenir:

1. **Procedural (varsayılan):** Karaciğer koddan üretilir (`LiverMeshGenerator`); dış dosya/lisans gerekmez.
2. **Gerçek model (önerilen görünüm):** Sketchfab/NIH 3D'den CC lisanslı `.fbx` veya `.glb` indirilir.
   - İndirilen dosya `Assets/_Project/Models/` klasörüne bırakılır.
   - `.glb` için `com.unity.cloud.gltfast` (6.12.0) paketi `manifest.json`'a eklidir (otomatik içe aktarır).
   - Menüden **`Post-transplantAR > Import Liver Model (Models klasöründen)`** çalıştırılır.
   - Araç modeli AR için ~18 cm'e ölçekler, `LiverVisualController` ile bağlar ve `Prefabs/LiverModel.prefab` olarak kaydeder.
   - Her iki sahne kurucusu (AR + Preview) prefab varsa otomatik onu kullanır.
   - CC Attribution lisansı için model sahibine künyede atıf verilmelidir.

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

## Credits (Model Atıfları)

- Karaciğer 3B modeli (`Assets/_Project/Models/humans_liver.glb`): "Human's Liver", Sketchfab üzerinden CC Attribution lisansıyla. Kullanılan modelin sahibine ve lisansına bağlı kalınmalıdır.

> Not: Farklı bir model kullanılırsa bu bölüm güncellenmeli; CC Attribution gereği model sahibinin adı ve kaynak bağlantısı eklenmelidir.

## Not

Bu sürüm yalnızca AR eğitim kapsamındadır.
