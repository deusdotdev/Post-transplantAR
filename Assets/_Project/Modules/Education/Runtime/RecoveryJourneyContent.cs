namespace LiverAR.Modules.Education.Runtime
{
    /// <summary>
    /// Nakil sonrası iyileşme yolculuğunun adımları (eğitim amaçlı, tanı koymaz).
    /// Klinik değerler yalnızca tipik bir gidişatı görselleştirmek için örnektir.
    /// </summary>
    public sealed class JourneyStep
    {
        public string Stage;        // örn. "0. gün"
        public string Title;        // kısa başlık
        public string Body;         // hasta dostu anlatım
        public float Growth;        // 0.3..1 (doku/işlev toparlanması)
        public float Health;        // 0..100
        public float Ast;           // U/L
        public float Alt;           // U/L
        public float Bilirubin;     // mg/dL
        public bool Complication;   // görsel "dikkat" vurgusu

        public JourneyStep(string stage, string title, string body,
            float growth, float health, float ast, float alt, float bilirubin,
            bool complication = false)
        {
            Stage = stage;
            Title = title;
            Body = body;
            Growth = growth;
            Health = health;
            Ast = ast;
            Alt = alt;
            Bilirubin = bilirubin;
            Complication = complication;
        }
    }

    public static class RecoveryJourneyContent
    {
        public static JourneyStep[] GetSteps()
        {
            return new[]
            {
                new JourneyStep(
                    "0. gün",
                    "Ameliyat sonrası ilk saatler",
                    "Yeni karaciğerin yerine bağlandı ve hemen çalışmaya başladı. Bu dönemde doku biraz " +
                    "ödemli olabilir; kan testlerinde AST/ALT ve bilirubin yüksek çıkar. Bu, organın strese " +
                    "verdiği normal bir tepkidir.\n\n" +
                    "Vücudunda ne oluyor: Damar ve safra bağlantıları yeni kurulduğu için karaciğer henüz tam " +
                    "verimde değil. Ekip seni yakından izler ve bağışıklığı baskılayan ilaçlar başlar.",
                    growth: 0.92f, health: 70f, ast: 360f, alt: 300f, bilirubin: 4.2f),

                new JourneyStep(
                    "1. hafta",
                    "Erken toparlanma",
                    "Karaciğer hücreleri kendini onarmaya başlar, dokuya kan taşıyan damarlar düzene girer. " +
                    "Kan değerleri hızla düşmeye başlar; sararma (sarılık) azalır.\n\n" +
                    "Vücudunda ne oluyor: İştahın ve enerjin yavaş yavaş geri gelir. İlaçların düzenli " +
                    "alınması bu noktada en kritik konudur; doz saatlerini kaçırma.",
                    growth: 0.95f, health: 80f, ast: 130f, alt: 140f, bilirubin: 2.6f),

                new JourneyStep(
                    "1. ay",
                    "İşlevin yerleşmesi",
                    "Karaciğer günlük görevlerinin çoğunu üstlenir: kanı süzme, protein üretimi, safra salgısı. " +
                    "Değerler normale yaklaşır.\n\n" +
                    "Vücudunda ne oluyor: Kontrol randevuları ve kan testleri sıklaşır. İlaç dozların " +
                    "tahlillere göre ekibince ayarlanabilir. Ateş, karın ağrısı veya yeniden sararma olursa bildir.",
                    growth: 0.98f, health: 88f, ast: 55f, alt: 62f, bilirubin: 1.4f),

                new JourneyStep(
                    "3. ay",
                    "Dengeye yaklaşma",
                    "Doku düzeni oturur, klinik değerler büyük ölçüde normal aralığa gelir. Karaciğer artık " +
                    "kararlı biçimde çalışır.\n\n" +
                    "Vücudunda ne oluyor: Günlük yaşamına daha rahat dönersin. Yine de bağışıklık baskılayıcı " +
                    "ilaçlar ömür boyu sürebilir; bu yüzden enfeksiyonlara karşı temizlik ve beslenme önemlidir.",
                    growth: 1.0f, health: 94f, ast: 34f, alt: 40f, bilirubin: 1.0f),

                new JourneyStep(
                    "6–12. ay",
                    "Uzun dönem takip",
                    "Metabolik denge yerleşmiştir. Düzenli ilaç ve takip ile karaciğer uzun yıllar sağlıklı " +
                    "çalışabilir.\n\n" +
                    "Vücudunda ne oluyor: Artık \"yeni normal\"in bu. İlaç disiplini, dengeli beslenme ve " +
                    "kontrollerin aksamaması en büyük koruman. Sorularını her zaman ekibine sor.",
                    growth: 1.0f, health: 98f, ast: 26f, alt: 30f, bilirubin: 0.8f)
            };
        }
    }
}
