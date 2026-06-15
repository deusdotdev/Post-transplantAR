# Post-transplantAR — Karaciğer Nakli Sonrası Eğitim Uygulaması

Karaciğer nakli sonrası hastaların kendi süreçlerini anlaması için tasarlanmış, **kart bazlı** bir mobil **Arttırılmış Gerçeklik** eğitim uygulaması. 

> **Önemli:** Tüm içerik eğitim amaçlıdır. Tanı koymaz, tedavi önermez. Kişisel kararlar için mutlaka transplant ekibinize danışın.

---

## Ne yapar?

| Kart | Mod | Özet |
|------|-----|------|
| **Nakil sonrası yolculuğum** | AR'sız | 0. gün → 1. hafta → 1. ay → 3. ay → 6–12. ay zaman çizelgesi; her adımda karaciğer rengi/ölçeği + AST/ALT/bilirubin + anlatım |
| **İlaçlarım nereye etki ediyor?** | AR | Modeli yerleştir, ilaç seç; bölgeden ok + etiket çıkar. **Düzenli / Atlanırsa** geçişi ile greftin nasıl korunduğu veya risk arttığı gösterilir |
| **Beslenme önerilerim** | AR'sız | Yapılması ve kaçınılması gerekenler, renk kodlu liste |
| **Karaciğeri keşfet** | AR | Bölgelere dokunarak anatomi bilgisi |


---

## Gereksinimler

| Bileşen | Sürüm / not |
|---------|-------------|
| **Unity** | 2022.3 LTS (proje: `2022.3.62f3`) |
| **Modüller** | Android Build Support, iOS Build Support (Xcode için Mac) |
| **XR** | AR Foundation + ARCore (Android) + ARKit (iOS) — `Packages/manifest.json` içinde tanımlı |
| **Cihaz** | AR destekli telefon/tablet (ARKit veya ARCore) |

---

## Hızlı kurulum (yeni gelen biri için)

### 1. Repoyu al

```bash
git clone <repo-url>
cd GüncelKonularKaraciğer   # veya klonladığınız klasör adı
```

### 2. Unity ile aç

1. [Unity Hub](https://unity.com/download) → **Add** → proje klasörünü seç.
2. Editor **2022.3.x LTS** yoksa Hub üzerinden aynı majör sürümü kur.
3. İlk açılışta paketlerin indirilmesini bekle (AR Foundation, glTFast vb.).

### 3. XR ayarları (bir kez)

1. **Edit → Project Settings → XR Plug-in Management**
2. **Android** sekmesi → **ARCore** işaretle.
3. **iOS** sekmesi → **ARKit** işaretle.

### 4. Sahne ve asset'leri otomatik kur

Unity üst menüsünden sırayla çalıştır (**Play modu kapalı olmalı**):

```
Post-transplantAR → Build AR Scene
Post-transplantAR → Build Home Hub
```

Bu komutlar:

- `EducationHub` (ana ekran, build index **0**) ve `ARMain` sahnelerini kurar/günceller.
- `SimulationState.asset`, `ARLaunchContext.asset`, `LiverModel.prefab` üretir veya yeniler.

### 5. Editörde test

1. **File → Build Settings** → `EducationHub` index 0 olduğunu doğrula.
2. **Play** → ana ekranda 4 kartı dene.
3. AR kartları için Editör'de AR tam çalışmayabilir; gerçek test **fiziksel cihaz** ile yapılır.

### 6. iOS build (Mac + Xcode)

```
Post-transplantAR → Build → iOS (Xcode Project)
```

1. Çıktı: `Builds/iOS/`
2. `Unity-iPhone.xcodeproj` aç → **Signing & Capabilities** → Team seç.
3. iPhone'a **Run**.

### 7. Android build

```
Post-transplantAR → Build → Android (APK)
```

Çıktı: `Builds/Android/PostTransplantAR.apk`

---

## 3B karaciğer modeli

**Varsayılan:** `Assets/_Project/Models/humans_liver.glb` (Sketchfab, CC Attribution).

Model değiştirmek için:

1. `.glb` veya `.fbx` dosyasını `Assets/_Project/Models/` altına koy.
2. Menü: **Post-transplantAR → Reimport Models Folder** (GLB için).
3. Menü: **Post-transplantAR → Import Liver Model**.
4. **Build AR Scene** ve **Build Home Hub** yeniden çalıştır.

Procedural yedek model (`LiverMeshGenerator`) harici dosya olmadan da çalışır.

---

## Proje yapısı

```text
Assets/_Project/
├── Editor/
│   ├── HomeHubBuilder.cs      # Ana ekran (4 kart) kurucu
│   ├── ARSceneBuilder.cs      # AR sahnesi kurucu
│   ├── MobileBuildPipeline.cs # iOS / Android build menüsü
│   └── LiverModelImporter.cs
├── Modules/
│   ├── Education/Runtime/     # İçerik: yolculuk, ilaç, beslenme
│   ├── AR/Runtime/            # Yerleştirme, plane, session
│   ├── Interaction/Runtime/   # Dokunma, döndürme, pinch
│   ├── Simulation/Runtime/    # SimulationState, senaryo mantığı
│   ├── Visuals/Runtime/       # Karaciğer görseli, bölge okları
│   └── UI/Runtime/            # Ekranlar, tema
├── Scenes/
│   ├── EducationHub.unity     # Giriş sahnesi
│   └── ARMain.unity           # AR sahnesi
├── Models/                    # humans_liver.glb
├── Prefabs/LiverModel.prefab  # Bölge marker'ları gömülü
└── SimulationState.asset
```

### Önemli script'ler

| Script | Görev |
|--------|--------|
| `HomeHubController` | Kart yönlendirme, AR modu seçimi |
| `RecoveryJourneyController` | Yolculuk adımları + görsel tetikleme |
| `DrugRegionController` | İlaç/bölge AR, düzenli-atlandı UI |
| `LiverVisualController` | Ölçek, sararma, ilaç uyumu rengi |
| `LiverRegionMarker` | Bölge parlama noktaları |
| `RegionAnnotationArrow` | Ok + 3B etiket |

---

## Çalışma akışı (özet)

**Ana ekran (`EducationHub`):**

1. Uygulama 4 kartla açılır.
2. **Yolculuk:** `RecoveryJourneyController` her adımda `SimulationState` günceller → karaciğer + dashboard yenilenir.
3. **Beslenme:** `NutritionController` + `NutritionLibrary`.
4. **AR kartları:** `ARLaunchContext` modu yazılır → `ARMain` yüklenir.

**AR (`ARMain`):**

1. Düz yüzeye dokun → karaciğer yerleşir.
2. **İlaç modu:** ilaç seç, düzenli/atlandı geçişi yap; bölgeler parlar, oklar çıkar.
3. **Keşfet modu:** bölgeye dokun → bilgi paneli.
4. **← Ana ekran** ile hub'a dön.

---

## Proje belgeleri

### Akademik teslim (`/docs`)

| PDF | İçerik | Puan |
|-----|--------|------|
| [docs/SWOT.pdf](docs/SWOT.pdf) | SWOT analizi | 10 |
| [docs/RAMS.pdf](docs/RAMS.pdf) | RAMS raporu | 5 |
| [docs/THS_report.pdf](docs/THS_report.pdf) | Teknoloji Hazırlık Seviyesi (kullanıma hazırlık) | 5 |
| [docs/Requirements.pdf](docs/Requirements.pdf) | Gereksinim analizi | 5 |
| [docs/UserScenario.pdf](docs/UserScenario.pdf) | Kullanım senaryoları | 5 |

Detay: [docs/README.md](docs/README.md)

### Geliştirme kaynakları

| Belge | İçerik |
|-------|--------|
| [GEREKSINIM_ANALIZI.md](GEREKSINIM_ANALIZI.md) | İşlevsel / işlevsel olmayan gereksinimler, kullanım senaryoları, kabul kriterleri |
| [RAMS.md](RAMS.md) | Güvenilirlik, kullanılabilirlik, bakım, güvenlik |
| [swot.md](swot.md) | SWOT analizi (kaynak) |
| [SETUP_ARFOUNDATION_ANDROID_TR.md](SETUP_ARFOUNDATION_ANDROID_TR.md) | AR Foundation kurulum adımları |

## Geliştirici notları

- Bölge marker konumları `LiverModel.prefab` altındaki `Marker_*` nesnelerinde; model değişince Scene view'da ince ayar gerekebilir.
- `Build Home Hub` çalıştırmadan sahne layout'u güncel olmayabilir; UI değişikliklerinden sonra menüyü yeniden çalıştır.
- İlaç paneli layout'u runtime'da da düzeltilir; yine de **Build AR Scene** önerilir.
- Ek AR kurulum detayı: [SETUP_ARFOUNDATION_ANDROID_TR.md](SETUP_ARFOUNDATION_ANDROID_TR.md)

---

## Lisans ve atıflar

### 3B model

- **Human's Liver** — Sketchfab, [CC Attribution](https://creativecommons.org/licenses/by/4.0/). Dosya: `Assets/_Project/Models/humans_liver.glb`. Model sahibine README ve uygulama içinde atıf verilmelidir.

### Alternatif ücretsiz kaynaklar

- [NIH 3D — Liver, Female (GLB)](https://3d.nih.gov/entries/3DPX-020973) — tıbbi referans anatomi

---

## Sorumluluk reddi

Bu yazılım yalnızca **hasta eğitimi** içindir. Acil şikâyetlerde ve ilaç/beslenme kararlarında transplant ekibinize başvurun.
