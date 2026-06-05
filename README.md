# Post-transplantAR - Karaciğer Nakli Sonrası AR Eğitim Uygulaması

Karaciğer nakli olmuş hastanın kendi sürecini tanıması için tasarlanmış, kart bazlı bir eğitim uygulaması. AR isteğe bağlıdır ve yalnızca ilgili karttan açılır.

## Proje Amacı

- Hastanın "nakil sonrası vücudumda/karaciğerimde ne değişiyor?" sorusuna rehberli bir yolculukla yanıt vermek.
- İlaçların karaciğerin hangi bölgesine etki ettiğini AR'da oklar ve etiketlerle göstermek.
- Nakil sonrası beslenme için yapılması/kaçınılması gerekenleri sade biçimde sunmak.
- Tüm içerik eğitim amaçlıdır; tanı koymaz, kişisel kararlar için transplant ekibine danışılmalıdır.

## Ana ekran (kart bazlı)

Uygulama doğrudan kameraya girmez; bir ana ekranla açılır:

1. **Nakil sonrası yolculuğum** (AR'sız) — 0. gün → 1. hafta → 1. ay → 3. ay → uzun dönem zaman çizelgesi; her adımda karaciğer görseli + klinik değerler + hasta dostu anlatım.
2. **İlaçlarım nereye etki ediyor? (AR)** — model yerleştirilir, ilaç seçilince ilgili bölgeden ok + etiket çıkar ve bölge parlar.
3. **Beslenme önerilerim** (AR'sız) — gruplanmış, renk kodlu (yap/kaçın) liste.
4. **Karaciğeri keşfet (AR)** — bölgeleri AR'da yakından inceleme.

## Kapsam

- Kart bazlı ana ekran (`EducationHub` sahnesi, giriş sahnesi)
- AR yalnızca 2. ve 4. karttan açılır (`ARMain` sahnesi)
- Bölge çapaları + ok/etiket görselleştirmesi (tek mesh üzerinde, alt-mesh ayrımı gerektirmez)
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
  - `ScenarioHUD`: iki senaryo ve aksiyon butonları

## Çalışma Akışı

Ana ekran (`HomeHubController`):
1. Uygulama `EducationHub` sahnesiyle açılır; 4 kart gösterilir.
2. **Yolculuk** kartı: `RecoveryJourneyController` adım adım `SimulationState`'i günceller; `LiverVisualController` görseli (sararma/şişme/ölçek) ve `ClinicalDashboard` klinik değerleri yansıtır.
3. **Beslenme** kartı: `NutritionController` içeriği `NutritionLibrary`'den doldurur.
4. **AR kartları**: `HomeHubController` `ARLaunchContext.CurrentMode`'u (DrugRegion / ExploreAnatomy) yazar ve `ARMain` sahnesini yükler.

AR sahnesi (`ARMain`):
1. `SafetyDisclaimerScreen` → `AnatomyIntroScreen` (kısa giriş).
2. `ARSessionController` cihaz uyumluluğunu, `PlaneDetectionMonitor` düz yüzeyi kontrol eder.
3. Kullanıcı dokunur → `ARPlacementController` modeli yerleştirir (bölge çapaları modelde gömülüdür).
4. `DrugRegionController` modeli görünce paneli açar: moda göre ilaç ya da bölge listesi; seçince `LiverRegionMarker` parlar ve `RegionAnnotationArrow` ok + etiket çizer.
5. `← Ana ekran` butonu (`SceneNavigator`) `EducationHub`'a döner.

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

## Editor Komutları (otomatik kurulum)

Üst menü `Post-transplantAR`:

- **Build Home Hub** — kart bazlı ana ekran (`EducationHub`); giriş sahnesi yapılır (build index 0). Yolculuk ve beslenme bu sahnededir.
- **Build AR Scene** — AR sahnesi (`ARMain`); ilaç→bölge ve keşif modları. Liver prefab'ına bölge çapalarını ekler.
- **Build Simulation Preview (No AR)** — eski, AR'sız senaryo önizlemesi (geliştirici testi için).

Önerilen kurulum sırası: önce **Build AR Scene**, sonra **Build Home Hub** (böylece her iki sahne de build ayarlarına eklenir, `EducationHub` index 0 olur).

Komutlar gerektiğinde şu asset'leri otomatik üretir: `SimulationState.asset`, `ARLaunchContext.asset`, `Materials/LiverMaterial.mat`, `Prefabs/LiverModel.prefab`.

## Yeni modüller (bu sürüm)

- `Modules/Education/Runtime/`: `ARLaunchContext` (sahne yönlendirme), `RecoveryJourneyContent` (yolculuk adımları), `DrugRegionLibrary` (ilaç→bölge + bölge metinleri), `NutritionLibrary` (beslenme).
- `Modules/UI/Runtime/Screens/`: `HomeHubController`, `RecoveryJourneyController`, `DrugRegionController`, `NutritionController`, `SceneNavigator`.
- `Modules/Visuals/Runtime/`: `LiverRegionMarker` (bölge çapası + parlama), `RegionAnnotationArrow` (ok + dünya-uzayı etiket).
- `Editor/`: `HomeHubBuilder` (ana ekran kurucu), `UiBuildKit` (ortak uGUI yardımcıları).

> Bölge çapalarının konumu, kullanılan karaciğer modeline göre Scene view'da elle ince ayar gerektirebilir (çapalar `LiverModel.prefab` altında görünür `Marker_*` nesneleridir).

## 3B Model

İki seçenek desteklenir:

1. **Procedural (varsayılan):** Karaciğer koddan üretilir (`LiverMeshGenerator`); dış dosya/lisans gerekmez.
2. **Gerçek model (önerilen görünüm):** Sketchfab/NIH 3D'den CC lisanslı `.fbx` veya `.glb` indirilir.
   - İndirilen dosya `Assets/_Project/Models/` klasörüne bırakılır.
   - `.glb` için `com.unity.cloud.gltfast` (6.12.0) gerekir; paket yüklenince **`Post-transplantAR > Reimport Models Folder`** sonra **`Import Liver Model`** çalıştırılır.
   - GLB import olmazsa menü **procedural** karaciğer prefab'ı oluşturur (AR yine çalışır). `.fbx` en sorunsuz seçenektir.
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
