# Kullanım Senaryoları — Post-transplantAR

**Belge sürümü:** 1.0  
**Tarih:** Haziran 2026  
**Proje:** Karaciğer nakli sonrası hasta eğitim uygulaması

---

## 1. Amaç

Bu belge, Post-transplantAR uygulamasının hedef kullanıcıları, temel kullanım akışlarını ve başarı kriterlerini tanımlar. Senaryolar test, demo ve akademik değerlendirme için referans niteliğindedir.

**Kapsam notu:** Uygulama push bildirim, ilaç saati hatırlatıcı, alarm veya doz takibi **sunmaz**. İlaç modülündeki «✓ Düzenli / ✗ Atlanırsa» geçişi yalnızca **eğitim amaçlı görsel senaryo** gösterir; gerçek ilaç programını yönetmez.

---

## 2. Kullanıcı profilleri (persona)

### Persona A — Ayşe (52, nakil sonrası hasta)

- Nakilden 3 hafta sonra; evde iyileşiyor.
- Akıllı telefon kullanabiliyor; AR deneyimi yok.
- İlaçların ne işe yaradığını görsel olarak öğrenmek istiyor.
- **Hedef:** Sürecini anlamak, AR'de ilaç–bölge eşlemesini incelemek.

### Persona B — Mehmet (61, hasta yakını / eş)

- Eşinin tedavi sürecini destekliyor.
- AR model üzerinde basit anlatım yapmak istiyor.
- **Hedef:** Uygulamadaki görsel ve metinlerle eşine eğitim desteği vermek.

### Persona C — Hemşire Elif (transplant eğitim hemşiresi)

- Yüz yüze eğitimi destekleyecek materyal arıyor.
- Uygulamanın tanı koymadığından emin olmak istiyor.
- **Hedef:** Standart, tekrarlanabilir hasta eğitimi.

---

## 3. Senaryo UC-01: İyileşme yolculuğunu inceleme

| Alan | Değer |
|------|-------|
| **Aktör** | Ayşe (hasta) |
| **Ön koşul** | Uygulama yüklü; internet gerekmez |
| **Tetikleyici** | «Nakil sonrası yolculuğum» kartına dokunma |

### Ana akış

1. Hasta uygulamayı açar; 4 kartlı ana ekranı görür.
2. «Nakil sonrası yolculuğum» kartını seçer.
3. 0. gün adımı açılır: sarımsı/küçük karaciğer görseli, AST/ALT/bilirubin örnek değerleri ve açıklama metni görünür.
4. «İleri» ile 1. hafta, 1. ay, 3. ay ve 6–12. ay adımlarına geçer.
5. Her adımda görsel renk/ölçek ve laboratuvar değerleri güncellenir.
6. «Ana ekran» ile hub'a döner.

### Alternatif akış

- **A1:** Hasta «Geri» ile önceki adıma dönebilir.
- **A2:** Son adımda ileri butonu pasif veya döngüsel davranır.

### Başarı kriterleri

- 5 adım sorunsuz gezinilir.
- Görsel ve metin her adımda senkron değişir.
- İşlem 3 dakika içinde tamamlanabilir (ilk kullanım).

### Başarısızlık / istisna

- Uygulama çökerse: yeniden açıldığında hub'dan devam edilebilir (durum kalıcılığı zorunlu değil).

---

## 4. Senaryo UC-02: İlaç etkisini AR'de öğrenme

| Alan | Değer |
|------|-------|
| **Aktör** | Ayşe veya Mehmet |
| **Ön koşul** | AR destekli cihaz; kamera izni verilmiş; yeterli aydınlatma |
| **Tetikleyici** | «İlaçlarım nereye etki ediyor?» kartı |

### Ana akış

1. Kullanıcı ilaç kartını seçer; AR oturumu başlar.
2. «Düz bir yüzeye bakın» yönlendirmesini okur.
3. Düzlem algılanınca ekrana dokunarak karaciğer modelini yerleştirir.
4. Alttaki ilaç listesinden **Takrolimus** seçer.
5. **✓ Düzenli** modda: yeşil ok, koruyucu metin, sağlıklı karaciğer tonu görür (eğitim senaryosu; gerçek ilaç kaydı değil).
6. **✗ Atlanırsa** moda geçer: kırmızı uyarı, risk metni, karaciğer tonu değişir (yine yalnızca eğitim amaçlı).
7. Modeli iki parmakla büyütür; ölçek sabit kalır.
8. Başka ilaç seçerek farklı bölgeleri inceler.

### Alternatif akış

- **A1:** Düzlem yoksa yerleştir butonu veya fallback ile tahmini konuma yerleştirme.
- **A2:** AR desteklenmeyen cihazda bilgilendirme ekranı gösterilir.

### Başarı kriterleri

- Model bir kez yerleştirildikten sonra telefon döndürülünce kaybolmaz (dünya anchor).
- Düzenli / Atlanırsa geçişinde UI çakışmaz.
- Pinch zoom kalıcıdır; senaryo değişince geri sıçramaz.

### Güvenlik notu

- Atlanırsa metninde «ekibinize başvurun» uyarı metni görünür.
- Uygulama doz veya tedavi değişikliği önermez.

---

## 5. Senaryo UC-03: Beslenme önerilerine bakma

| Alan | Değer |
|------|-------|
| **Aktör** | Ayşe |
| **Ön koşul** | Uygulama açık |
| **Tetikleyici** | Beslenme kartı |

### Ana akış

1. Hasta beslenme kartını açar.
2. «Önerilenler» ve «Kaçınılması gerekenler» listelerini okur.
3. Alt kısımda «Kişisel diyet için ekibinize danışın» uyarısını görür.
4. Ana ekrana döner.

### Başarı kriterleri

- Tüm liste maddeleri okunaklı render edilir.
- Uyarı metni görünürdür.

---

## 6. Senaryo UC-04: Karaciğeri keşfet (anatomi AR)

| Alan | Değer |
|------|-------|
| **Aktör** | Mehmet (hasta yakını) veya Hemşire Elif |
| **Ön koşul** | AR destekli cihaz |
| **Tetikleyici** | «Karaciğeri keşfet» kartı |

### Ana akış

1. Kullanıcı keşfet kartını seçer.
2. Modeli düz yüzeye yerleştirir.
3. Sağ lob marker'ına dokunur; bölge adı ve kısa açıklama görünür.
4. Sol lob, safra yolları ve damar girişi marker'larını sırayla inceler.
5. Model otomatik dönmez (inceleme modu).

### Başarı kriterleri

- En az 4 bölge marker'ı dokunulabilir ve bilgi gösterir.
- Keşfet modunda otomatik turntable dönüş kapalıdır.

---

## 7. Senaryo UC-05: Hemşire ile birlikte eğitim

| Alan | Değer |
|------|-------|
| **Aktör** | Hemşire Elif + Ayşe |
| **Ön koşul** | Taburculuk eğitimi randevusu |
| **Tetikleyici** | Hemşirenin uygulamayı hasta telefonunda göstermesi |

### Ana akış

1. Hemşire önce «Tanı koymaz» uyarısını vurgular.
2. Yolculuk modunda 0. gün ve 1. ay adımlarını birlikte okurlar.
3. İlaç AR modunda Takrolimus «Düzenli / Atlanırsa» farkını gösterir.
4. Hasta eve gittikten sonra uygulamayı tek başına tekrar açar.

### Başarı kriterleri

- Hemşire 15 dakikada 3 modülü demo edebilir.
- Hasta tek başına UC-01 ve UC-03'ü tekrarlayabilir (kullanılabilirlik hedefi).

---

## 8. Senaryo özeti tablosu

| ID | Senaryo | Mod | AR | Öncelik |
|----|---------|-----|-----|---------|
| UC-01 | İyileşme yolculuğu | Hub | Hayır | Yüksek |
| UC-02 | İlaç etkisi | ARMain | Evet | Yüksek |
| UC-03 | Beslenme | Hub | Hayır | Orta |
| UC-04 | Anatomi keşfi | ARMain | Evet | Yüksek |
| UC-05 | Hemşire eşliğinde eğitim | Karma | Kısmi | Orta |

---

## 9. Kabul testi kontrol listesi

- [ ] UC-01: 5 adım, görsel değişimi doğrulandı
- [ ] UC-02: 5 ilaç + düzenli/atlandı geçişi
- [ ] UC-02: AR yerleştirme sonrası telefon hareketi ile model sabit
- [ ] UC-03: Beslenme listesi + uyarı metni
- [ ] UC-04: 4 bölge dokunma + bilgi paneli
- [ ] Tüm senaryolarda «Tanı koymaz» uyarısı görünür

---

*Bu belge `GEREKSINIM_ANALIZI.md` Bölüm 6'nın genişletilmiş sürümüdür.*
