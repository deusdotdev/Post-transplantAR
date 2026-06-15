# Gereksinim Analizi — Post-transplantAR

**Belge sürümü:** 1.0  
**Tarih:** Haziran 2026  
**Proje:** Karaciğer nakli sonrası hasta eğitim uygulaması (Unity, iOS/Android)

---

## 1. Amaç

Bu belge, Post-transplantAR uygulamasının işlevsel ve işlevsel olmayan gereksinimlerini tanımlar. Hedef kitle: proje geliştiricileri, test edenler, akademik danışmanlar ve süreci değerlendiren paydaşlar.

Uygulama **hasta eğitimi** sunar; **tanı koymaz**, **tedavi önermez**, **kişisel risk hesabı yapmaz**.

---

## 2. Paydaşlar

| Paydaş | Beklenti |
|--------|----------|
| **Nakil sonrası hasta** | Sürecini anlamak, ilaç ve beslenmeyi öğrenmek, görsel destek almak |
| **Transplant hemşiresi / eğitim hemşiresi** | Standart, tekrarlanabilir eğitim materyali |
| **Transplant hekimi / koordinatör** | Yanlış yönlendirme yapmayan, ekibe yönlendiren içerik |
| **Geliştirici / öğrenci ekibi** | Modüler, Unity üzerinde sürdürülebilir mimari |
| **Akademik danışman** | Literatüre uygun, savunulabilir kapsam ve sınırlar |

---

## 3. Sistem bağlamı

```text
[Hasta] ──► [Mobil uygulama: EducationHub]
                 ├── Yolculuk (AR'sız)
                 ├── Beslenme (AR'sız)
                 └── ARMain (isteğe bağlı)
                       ├── İlaç → bölge
                       └── Anatomi keşfi
```

- **Giriş noktası:** `EducationHub` (kart bazlı ana ekran)
- **AR:** Yalnızca 2. ve 4. karttan; AR olmadan uygulama kullanılabilir
- **Veri:** Sunucu yok; içerik ve durum yerel (`ScriptableObject`, sahne içi)

---

## 4. İşlevsel gereksinimler

### 4.1 Ana ekran ve gezinme

| ID | Gereksinim | Öncelik | Durum |
|----|------------|---------|-------|
| FR-01 | Uygulama 4 kartlı ana ekranla açılmalıdır. | Yüksek | ✅ |
| FR-02 | Her kart ilgili modüle yönlendirmelidir. | Yüksek | ✅ |
| FR-03 | Alt modüllerde «Ana ekran» ile hub'a dönülebilmelidir. | Yüksek | ✅ |
| FR-04 | AR kartları doğrudan kamera açmadan önce mod seçimi yapmalıdır. | Yüksek | ✅ |

### 4.2 Nakil sonrası yolculuk

| ID | Gereksinim | Öncelik | Durum |
|----|------------|---------|-------|
| FR-10 | Zaman çizelgesi en az 5 adım içermelidir (0. gün, 1. hafta, 1. ay, 3. ay, 6–12. ay). | Yüksek | ✅ |
| FR-11 | Her adımda hasta dostu başlık ve açıklama metni gösterilmelidir. | Yüksek | ✅ |
| FR-12 | Her adımda örnek klinik değerler (AST, ALT, bilirubin) sunulmalıdır. | Orta | ✅ |
| FR-13 | Her adımda karaciğer görseli (renk/ölçek) adımla uyumlu değişmelidir. | Yüksek | ✅ |
| FR-14 | İleri / geri ile adımlar arasında gezinilebilmelidir. | Yüksek | ✅ |
| FR-15 | İlerleme göstergesi (örn. 2/5) görüntülenmelidir. | Düşük | ✅ |

### 4.3 İlaçlar ve bölge eşlemesi (AR)

| ID | Gereksinim | Öncelik | Durum |
|----|------------|---------|-------|
| FR-20 | Kullanıcı AR'de karaciğer modelini düz yüzeye yerleştirebilmelidir. | Yüksek | ✅ |
| FR-21 | En az 5 immünosupresif / destek ilacı listelenmelidir. | Yüksek | ✅ |
| FR-22 | İlaç seçilince ilgili karaciğer bölgesi vurgulanmalıdır. | Yüksek | ✅ |
| FR-23 | Bölgeden ok ve kısa etiket çıkmalıdır. | Yüksek | ✅ |
| FR-24 | «Düzenli alındığında» ve «Atlanırsa» senaryoları arasında geçiş yapılabilmelidir. | Yüksek | ✅ |
| FR-25 | Atlanırsa senaryosunda risk metni, kırmızı vurgu ve karaciğer tonu değişmelidir. | Yüksek | ✅ |
| FR-26 | Model döndürülebilmeli ve pinch ile ölçeklenebilmelidir (ölçek kalıcı). | Orta | ✅ |

### 4.4 Beslenme

| ID | Gereksinim | Öncelik | Durum |
|----|------------|---------|-------|
| FR-30 | Beslenme önerileri gruplanmış listeler halinde sunulmalıdır. | Yüksek | ✅ |
| FR-31 | Yapılması ve kaçınılması gerekenler görsel olarak ayrılmalıdır. | Orta | ✅ |
| FR-32 | Kişisel diyet için ekibe danışma uyarısı yer almalıdır. | Yüksek | ✅ |

### 4.5 Anatomi keşfi (AR)

| ID | Gereksinim | Öncelik | Durum |
|----|------------|---------|-------|
| FR-40 | Kullanıcı karaciğer bölgesine dokunarak bilgi alabilmelidir. | Yüksek | ✅ |
| FR-41 | Sağ lob, sol lob, safra yolları ve damar girişi tanıtılmalıdır. | Yüksek | ✅ |
| FR-42 | Keşfet modunda otomatik model dönüşü kapalı olmalıdır (inceleme kolaylığı). | Orta | ✅ |

### 4.6 AR altyapısı

| ID | Gereksinim | Öncelik | Durum |
|----|------------|---------|-------|
| FR-50 | AR Foundation ile düzlem algılama yapılmalıdır. | Yüksek | ✅ |
| FR-51 | AR desteklenmeyen cihazda kullanıcı bilgilendirilmelidir. | Orta | ✅ |
| FR-52 | Yerleştirme öncesi kullanıcıya kısa yönlendirme metni gösterilmelidir. | Orta | ✅ |

### 4.7 Güvenlik ve uyumluluk (işlevsel)

| ID | Gereksinim | Öncelik | Durum |
|----|------------|---------|-------|
| FR-60 | «Eğitim amaçlıdır; tanı koymaz» uyarısı görünür olmalıdır. | Yüksek | ✅ |
| FR-61 | İlaç atlama senaryosunda acil durumda ekibe başvuru hatırlatması olmalıdır. | Yüksek | ✅ |
| FR-62 | İçerik kişisel tanı veya doz ayarı iddiası taşımamalıdır. | Yüksek | ✅ |

---

## 5. İşlevsel olmayan gereksinimler

### 5.1 Platform ve performans

| ID | Gereksinim | Hedef |
|----|------------|-------|
| NFR-01 | iOS (ARKit) ve Android (ARCore) desteği | Zorunlu |
| NFR-02 | Unity 2022.3 LTS uyumluluğu | Zorunlu |
| NFR-03 | AR modunda kabul edilebilir kare hızı | ≥ 24 FPS (orta segment cihaz) |
| NFR-04 | AR'siz modlarda akıcı UI | ≥ 30 FPS |

### 5.2 Kullanılabilirlik

| ID | Gereksinim | Hedef |
|----|------------|-------|
| NFR-10 | Tek elle kullanım (kart seçimi, ileri/geri) | Desteklenmeli |
| NFR-11 | Okunaklı tipografi (yaşlı hasta profili) | Min. 22–26 pt gövde metni |
| NFR-12 | Yüksek kontrastlı klinik açık tema | Uygulandı |
| NFR-13 | AR etkileşim alanı ile alt panel çakışmamalı | UI katmanları ayrılmış |

### 5.3 Bakım ve genişletilebilirlik

| ID | Gereksinim | Hedef |
|----|------------|-------|
| NFR-20 | İçerik kod dışı kütüphanelerde (`*Library.cs`) | Uygulandı |
| NFR-21 | Sahne kurulumu Editor menüsü ile tekrarlanabilir | `Build Home Hub`, `Build AR Scene` |
| NFR-22 | Modüler klasör yapısı (`Modules/`) | Uygulandı |
| NFR-23 | Tek doğruluk kaynağı simülasyon verisi | `SimulationState` ScriptableObject |

### 5.4 Güvenilirlik

| ID | Gereksinim | Hedef |
|----|------------|-------|
| NFR-30 | Çökme oranı (crash-free oturum) | ≥ %99 (hedef) |
| NFR-31 | AR yerleştirme başarısı (uygun ortamda) | ≥ %95 (hedef) |
| NFR-32 | Ağ bağlantısı olmadan tam çalışma | Zorunlu (offline-first) |

### 5.5 Güvenlik ve gizlilik

| ID | Gereksinim | Hedef |
|----|------------|-------|
| NFR-40 | Hasta kimliği / PHI toplanmamalı | Zorunlu |
| NFR-41 | Hesap / giriş zorunluluğu olmamalı | Mevcut kapsam |
| NFR-42 | Konum verisi üçüncü tarafa gönderilmemeli | Zorunlu |

---

## 6. Kullanım senaryoları (özet)

### UC-01: İyileşme yolculuğunu inceleme

1. Hasta uygulamayı açar.
2. «Nakil sonrası yolculuğum» kartını seçer.
3. 0. gün adımını okur; karaciğerin sarımsı/küçük göründüğünü fark eder.
4. «İleri» ile sonraki dönemlere geçer; laboratuvar değerlerinin ve görselin değiştiğini görür.

**Başarı kriteri:** En az 5 adım sorunsuz gezinilir; görsel ve metin senkron değişir.

### UC-02: İlaç etkisini AR'de öğrenme

1. Hasta «İlaçlarım nereye etki ediyor?» kartını seçer.
2. Kamerayı düz yüzeye tutar, modele dokunur.
3. Takrolimus seçer; «Düzenli» modda yeşil ok ve koruyucu metin görür.
4. «Atlanırsa»ya geçer; kırmızı uyarı, risk metni ve karaciğer tonu değişir.

**Başarı kriteri:** İki mod arasında geçişte UI çakışmaz; görsel geri alınmaz (pinch zoom korunur).

### UC-03: Beslenme listesine bakma

1. Hasta beslenme kartını açar.
2. Önerilen ve kaçınılacak maddeleri okur.
3. Alt uyarıda ekibe danışma notunu görür.

### UC-04: Anatomi keşfi

1. Hasta keşfet kartını açar, modeli yerleştirir.
2. Sağ lob marker'ına dokunur; bölge bilgisi görünür.

---

## 7. İçerik gereksinimleri

| Alan | Kaynak dosya | Güncelleme |
|------|--------------|------------|
| Yolculuk adımları | `RecoveryJourneyContent.cs` | Klinik danışman onayı önerilir |
| İlaç–bölge | `DrugRegionLibrary.cs` | Merkez protokolüne göre |
| Beslenme | `NutritionLibrary.cs` | Diyetisyen onayı önerilir |
| Bölge anatomisi | `DrugRegionLibrary.GetRegions()` | Sabit eğitim metni |

Tüm metinler **örnek / tipik gidişat** olarak sunulmalı; bireysel hasta verisi içermemelidir.

---

## 8. Kısıtlar ve varsayımlar

### Kısıtlar

- Tek geliştirici bilgisayarı + fiziksel cihaz ile test (CI/CD yok).
- Karaciğer modeli tek mesh; loblar ayrı mesh değil (marker tabanlı vurgu).
- Sunucu/API entegrasyonu yok.
- Sadece Türkçe arayüz (mevcut sürüm).

### Varsayımlar

- Kullanıcı akıllı telefon sahibidir.
- AR modları için yeterli aydınlatma ve düz yüzey vardır.
- Hasta eğitimi transplant ekibi ile birlikte, yüz yüze veya telefonla desteklenir.
- İçerik tıbbi danışman tarafından gözden geçirilmiş veya gözden geçirilecektir.

---

## 9. Kapsam dışı (bu sürüm)

| Madde | Gerekçe |
|-------|---------|
| Gerçek hasta verisi / EMR entegrasyonu | Kapsam ve KVKK karmaşıklığı |
| Push bildirim / ilaç hatırlatıcı | Ayrı ürün özelliği |
| Çoklu dil | Zaman kısıtı |
| Tele-sağlık / canlı doktor bağlantısı | Altyapı gerektirir |
| Kişiselleştirilmiş risk skoru | Tıbbi cihaz / SaMD sınırına yaklaşır |
| Web veya VR sürümü | Mobil AR odaklı proje |

---

## 10. Kabul kriterleri (release)

Aşağıdakiler sağlandığında sürüm «kabul edilmiş» sayılır:

- [ ] `EducationHub` ve `ARMain` build'e dahil; hub index 0.
- [ ] 4 kartın tamamı açılır ve geri dönüş çalışır.
- [ ] Yolculukta 5 adım + görsel değişim doğrulanır.
- [ ] İlaç AR'de 5 ilaç + düzenli/atlandı geçişi çalışır.
- [ ] Beslenme listesi eksiksiz render edilir.
- [ ] iOS fiziksel cihazda AR yerleştirme başarılı.
- [ ] README ve kurulum adımları yeni geliştirici tarafından izlenebilir.
- [ ] «Tanı koymaz» uyarıları görünür.

---

## 11. Gereksinim – bileşen izlenebilirliği

| Gereksinim | Ana bileşen |
|------------|-------------|
| FR-01–04 | `HomeHubController`, `HomeHubBuilder` |
| FR-10–15 | `RecoveryJourneyController`, `RecoveryJourneyContent`, `LiverVisualController` |
| FR-20–26 | `DrugRegionController`, `DrugRegionLibrary`, `ARPlacementController`, `ModelManipulator` |
| FR-30–32 | `NutritionController`, `NutritionLibrary` |
| FR-40–42 | `DrugRegionController` (explore modu), `LiverRegionMarker` |
| FR-50–52 | `ARSessionController`, `PlaneDetectionMonitor`, `ARPlacementPrompt` |
| FR-60–62 | `SafetyDisclaimerScreen`, `ArTopBarCleanup`, içerik metinleri |
| NFR-20–23 | `SimulationState`, Editor build pipeline |

---

## 12. Gelecek sürüm adayları (backlog)

| ID | Özellik | Öncelik |
|----|---------|---------|
| BL-01 | İlaç atlama modunda hafif şişme animasyonu (pinch ile uyumlu) | Orta |
| BL-02 | 0. gün için ayrı «stresli» 3B model | Düşük |
| BL-03 | İçerik CMS / JSON dışa aktarma | Orta |
| BL-04 | Hemşire paneli / kullanım analitiği (anonim) | Düşük |
| BL-05 | İngilizce dil desteği | Düşük |

---

## 13. İlgili belgeler

- [README.md](README.md) — Kurulum ve proje özeti
- [SETUP_ARFOUNDATION_ANDROID_TR.md](SETUP_ARFOUNDATION_ANDROID_TR.md) — XR kurulum detayı
- [RAMS.md](RAMS.md) — Güvenilirlik, kullanılabilirlik, bakım, güvenlik değerlendirmesi
- [swot.md](swot.md) — SWOT analizi

---

*Bu belge canlı dokümandır; kapsam değiştikçe sürüm numarası artırılmalıdır.*
