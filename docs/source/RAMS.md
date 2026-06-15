# RAMS Raporu - Post-transplantAR

## 1. Amaç ve Kapsam

Bu RAMS raporu, karaciğer nakli sonrası hastalara yönelik AR eğitim uygulamasının teknik ve kullanıcı güvenliği açısından değerlendirilmesi için hazırlanmıştır.

Kapsam:
- AR model yerleştirme ve etkileşim akışı
- Mobil cihazlarda çalışma sürekliliği
- Bakım ve sürüm yönetimi yaklaşımı
- Kullanıcı güvenliği ve yanlış yönlendirmeyi önleme önlemleri

Not: Bu uygulama tıbbi tanı veya tedavi kararı vermez; yalnızca eğitim ve farkındalık amaçlıdır.

## 2. R - Reliability (Güvenilirlik)

### Hedef
- AR eğitim deneyiminin tutarlı şekilde çalışması
- Kullanıcı etkileşimlerinin hatasız işlenmesi

### Ölçütler (KPI)
- Model yerleştirme başarı oranı: >= %95
- Uygulama çökme oranı (crash-free sessions): >= %99
- Kritik hata sayısı (release başına): 0

### Riskler
- Düşük ışık veya yetersiz yüzey algısında AR raycast başarısızlığı
- Cihaz performansı düşük olduğunda frame düşüşleri

### Azaltma Önlemleri
- Plane algılanmadan yerleştirme aksiyonunu pasifleştirme
- Kullanıcıya “daha iyi aydınlatma/yüzey” yönlendirmesi gösterme
- Düşük performansta model detay seviyesini azaltma (LOD)

## 3. A - Availability (Kullanılabilirlik)

### Hedef
- Uygulamanın hedef cihazlarda erişilebilir ve çalışır durumda olması

### Ölçütler (KPI)
- Uygulama açılış başarı oranı: >= %99
- Ortalama açılış süresi (cold start): <= 4 sn (orta segment cihaz)
- Kritik ekranlara erişim oranı: %100 (AR ekranı dahil)

### Riskler
- ARCore/ARKit uyumsuz cihazlarda deneyimin açılamaması
- Yanlış platform ayarları nedeniyle build sorunları

### Azaltma Önlemleri
- Uyumlu cihaz kontrolü ve desteklenmiyorsa bilgilendirici fallback ekranı
- Android/iOS için ayrı test checklist’i
- CI/CD veya release öncesi smoke test akışı

## 4. M - Maintainability (Bakım Yapılabilirlik)

### Hedef
- Kod tabanının hızlı güncellenebilir ve sürdürülebilir olması

### Ölçütler (KPI)
- Ortalama hata çözüm süresi (MTTR): <= 2 iş günü
- Küçük iyileştirme geliştirme süresi: <= 1 sprint
- Dokümantasyon güncelliği: her release sonrası güncelleme

### Riskler
- Modüller arası bağımlılığın artmasıyla değişiklik maliyetinin yükselmesi
- Unity/AR Foundation sürüm geçişlerinde kırılmalar

### Azaltma Önlemleri
- Modüler mimariyi koruma (AR, Interaction, UI, Bootstrap ayrımı)
- Kod inceleme standardı ve commit mesaj standardı
- Sürüm yükseltmelerini ayrı branch’te doğrulama

## 5. S - Safety (Emniyet/Güvenlik)

### Hedef
- Kullanıcının yanlış tıbbi yorum yapmasını önlemek
- Uygulama kullanımında fiziksel ve bilişsel güvenliği desteklemek

### Ölçütler (KPI)
- Güvenlik uyarısı görünürlüğü: %100 (ilk kullanım + kritik ekranlar)
- “Tanı aracı değildir” mesajının anlaşılma oranı (kullanıcı testi): >= %90
- Yanıltıcı tıbbi ifade sayısı: 0

### Riskler
- Kullanıcının uygulamayı tanı aracı olarak algılaması
- AR kullanımında dikkatin çevreden kopması (yürüme sırasında kullanım vb.)

### Azaltma Önlemleri
- Açık ve tekrar eden uyarı metinleri: “Bu uygulama tanı koymaz.”
- Kritik içeriklerde “Doktorunuza danışın” yönlendirmesi
- AR ekranında güvenli kullanım ipuçları (oturarak/sabit konumda kullanım)

## 6. Doğrulama ve Test Planı (Özet)

- Fonksiyonel test: model yerleştirme, yeniden konumlandırma, UI yönlendirme metinleri
- Cihaz testi: en az 3 Android + 2 iOS farklı performans sınıfı
- Kullanılabilirlik testi: teknik olmayan kullanıcı ile görev tamamlama
- Regresyon testi: her release öncesi temel AR akışı

## 7. Sonuç

Post-transplantAR için RAMS yaklaşımı, uygulamanın yalnızca AR eğitim hedefinde güvenilir, erişilebilir, sürdürülebilir ve kullanıcı açısından güvenli şekilde ilerlemesini destekler. Ürün kapsamı büyürse RAMS ölçütleri release bazında güncellenmelidir.
