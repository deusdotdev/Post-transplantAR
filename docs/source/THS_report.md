# THS Raporu — Teknoloji Hazırlık Seviyesi
## Post-transplantAR (Karaciğer Nakli Sonrası AR Eğitim Uygulaması)

**Belge sürümü:** 1.0  
**Tarih:** Haziran 2026  
**Proje ekibi:** Post-transplantAR geliştirme grubu

---

## 1. Amaç

Bu rapor, Post-transplantAR uygulamasının **Teknoloji Hazırlık Seviyesi (THS / TRL)** değerlendirmesini sunar. THS, bir teknolojinin laboratuvar ortamından gerçek kullanıma ne kadar hazır olduğunu 1–9 arası ölçekle ifade eder.

**Hedef soru:** Uygulama hasta eğitiminde kullanıma ne kadar hazır?

---

## 2. THS ölçeği (özet)

| Seviye | Tanım | Post-transplantAR karşılığı |
|--------|-------|-----------------------------|
| THS 1 | Temel ilkeler gözlemlendi | Sağlık eğitiminde AR faydası literatürde tanımlandı |
| THS 2 | Teknoloji konsepti formüle edildi | Kart bazlı hub + seçmeli AR akışı tasarlandı |
| THS 3 | Deneysel kavram kanıtı | Unity prototipi; tek modül AR yerleştirme |
| THS 4 | Laboratuvarda doğrulandı | Editor + simülatörde tüm modüller çalışır |
| THS 5 | İlgili ortamda doğrulandı | Fiziksel telefonda AR, UI ve içerik test edildi |
| THS 6 | İlgili ortamda gösterildi | iOS/Android cihazda uçtan uca hasta akışı demo edildi |
| THS 7 | Operasyonel ortamda prototip | Gerçek hasta profiline yakın kullanıcı testi yapılabilir |
| THS 8 | Sistem tamamlandı ve nitelendirildi | Klinik pilot + regresyon + içerik onayı gerekir |
| THS 9 | Operasyonel ortamda kanıtlandı | Hastane/taburculuk paketi olarak yaygın kullanım |

---

## 3. Genel THS değerlendirmesi

| Kriter | Değerlendirme |
|--------|---------------|
| **Mevcut genel THS** | **THS 6** |
| **Hedef THS (akademik teslim)** | THS 6–7 |
| **Üretim / klinik dağıtım için hedef** | THS 8 |

**Gerekçe:** Uygulama Unity 2022.3 LTS üzerinde modüler olarak geliştirildi; 4 ana modül (yolculuk, ilaç AR, beslenme, anatomi keşfi) fiziksel AR destekli cihazlarda çalışır durumda. Ancak klinik pilot çalışma, çok merkezli doğrulama ve resmi içerik onayı tamamlanmadığı için THS 8–9 seviyesine ulaşılmamıştır.

---

## 4. Alt sistem bazında THS

### 4.1 Ana ekran ve gezinme (EducationHub)

| Alan | Seviye | Kanıt |
|------|--------|-------|
| Kart bazlı navigasyon | THS 6 | `EducationHub` sahnesi, 4 kart, geri dönüş |
| Mod yönlendirme | THS 6 | `ARLaunchContext` ile ilaç / keşfet ayrımı |
| Editor ile yeniden kurulum | THS 5 | `Build Home Hub` menüsü |

**Not:** Çoklu dil ve erişilebilirlik sertifikasyonu yok → THS 7 için eksik.

### 4.2 Nakil sonrası yolculuk (AR'sız)

| Alan | Seviye | Kanıt |
|------|--------|-------|
| Zaman çizelgesi (5+ adım) | THS 6 | `RecoveryJourneyController`, adım metinleri |
| Görsel–metin senkronu | THS 6 | `LiverVisualController` adım bazlı renk/ölçek |
| Klinik değer gösterimi | THS 5 | Örnek AST/ALT/bilirubin; gerçek EMR yok |

**Risk:** İçerik tıbbi danışman onayı formalize edilmedi → klinik kullanımda THS 7 engeli.

### 4.3 İlaçlar ve bölge eşlemesi (AR)

| Alan | Seviye | Kanıt |
|------|--------|-------|
| AR yerleştirme | THS 6 | `ARPlacementController`, düzlem + anchor |
| İlaç–bölge eşlemesi | THS 6 | `DrugRegionLibrary`, marker + ok |
| Düzenli / Atlanırsa senaryosu | THS 6 | Görsel ve metin birlikte güncellenir |
| Pinch zoom / döndürme | THS 6 | `ModelManipulator` |

**Risk:** Düşük ışıkta yerleştirme başarısı cihaza bağlı → saha testi genişletilmeli.

### 4.4 Beslenme modülü

| Alan | Seviye | Kanıt |
|------|--------|-------|
| Gruplanmış listeler | THS 6 | `NutritionLibrary`, `NutritionController` |
| Kişisel diyet uyarısı | THS 6 | Ekibe danışma metni |

### 4.5 Anatomi keşfi (AR)

| Alan | Seviye | Kanıt |
|------|--------|-------|
| Bölgeye dokunarak bilgi | THS 6 | `LiverRegionMarker`, `DrugRegionController` explore |
| Otomatik dönüş kapatma | THS 6 | Keşfet modunda `SetAutoRotate(false)` |

### 4.6 AR altyapısı

| Alan | Seviye | Kanıt |
|------|--------|-------|
| AR Foundation 5.2 | THS 6 | ARKit + ARCore manifest |
| Düzlem algılama | THS 6 | `PlaneDetectionMonitor` |
| Desteklenmeyen cihaz fallback | THS 5 | `ARSessionController`, `ARSetupGuide` |
| Dünya anchor sabitleme | THS 6 | `ARAnchorManager` entegrasyonu |

### 4.7 Güvenlik ve uyumluluk

| Alan | Seviye | Kanıt |
|------|--------|-------|
| «Tanı koymaz» uyarıları | THS 6 | `SafetyDisclaimerScreen`, üst şerit |
| PHI / sunucu yok | THS 6 | Offline-first, yerel içerik |
| KVKK / GDPR resmi denetim | THS 3 | Politika dokümanı ve DPIA yapılmadı |
| App Store sağlık beyanı süreci | THS 4 | Henüz mağaza yayını yok |

---

## 5. Kullanıma hazırlık matrisi

| Boyut | Durum | Açıklama |
|-------|-------|----------|
| **Teknik olgunluk** | ✅ Yüksek | Modüller entegre, build pipeline mevcut |
| **İçerik olgunluğu** | ⚠️ Orta | Eğitim metinleri var; klinik onay formal değil |
| **Kullanıcı testi** | ⚠️ Orta | Sınırlı cihaz ve kullanıcı profili |
| **Operasyonel dağıtım** | ❌ Düşük | Hastane entegrasyonu, güncelleme kanalı yok |
| **Regülasyon** | ⚠️ Orta | Eğitim aracı sınırı korunuyor; resmi uyum eksik |
| **Sürdürülebilirlik** | ✅ Yüksek | Modüler kod, Editor build araçları |

---

## 6. THS 7'ye yükseltmek için gerekenler

1. **En az 10 hasta / hasta yakını ile görev tabanlı kullanılabilirlik testi** (SUS anketi + görev tamamlama süresi).
2. **En az 5 farklı AR cihazında** yerleştirme başarı oranı ölçümü (hedef ≥ %95).
3. **Transplant hemşiresi veya hekiminden içerik onayı** (ilaç, beslenme, yolculuk metinleri).
4. **Gizlilik politikası ve veri işleme aydınlatması** (KVKK uyumlu kısa metin).
5. **Release notları + bilinen sorunlar listesi** her build ile birlikte.

---

## 7. THS 8–9 için uzun vadeli adımlar

| Adım | Hedef THS |
|------|-----------|
| Pilot hastane / transplant merkezinde 4 haftalık deneme | THS 7→8 |
| İçerik versiyonlama ve uzaktan güncelleme (CMS) | THS 8 |
| App Store / Play Store yayını ve geri bildirim döngüsü | THS 8 |
| Çok merkezli kullanım metrikleri ve crash analitiği | THS 8→9 |
| Taburculuk eğitim paketine resmi entegrasyon | THS 9 |

---

## 8. Sonuç

Post-transplantAR, **akademik proje ve erken saha demosu** için **THS 6** seviyesindedir: teknoloji ilgili ortamda (mobil AR cihaz) uçtan uca gösterilebilir ve hasta eğitim senaryolarını destekler. Klinik rutinde güvenle önerilebilmesi için içerik onayı, genişletilmiş kullanıcı testi ve operasyonel süreçlerle **THS 7–8** hedeflenmelidir.

Uygulama **tanı veya tedavi aracı değildir**; THS değerlendirmesi de bu sınır içinde yapılmıştır.

---

## 9. İlgili belgeler

- `docs/Requirements.pdf` — İşlevsel ve işlevsel olmayan gereksinimler
- `docs/UserScenario.pdf` — Kullanım senaryoları
- `docs/RAMS.pdf` — Güvenilirlik, kullanılabilirlik, bakım, güvenlik
- `docs/SWOT.pdf` — SWOT analizi
