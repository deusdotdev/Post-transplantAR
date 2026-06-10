namespace LiverAR.Modules.Education.Runtime
{
    /// <summary>
    /// Nakil sonrası iyileşme yolculuğunun adımları (eğitim amaçlı, tanı koymaz).
    /// Laboratuvar değerleri tipik post-transplant gidişatını yansıtan örnek aralıklardır.
    /// </summary>
    public sealed class JourneyStep
    {
        public string Stage;
        public string Title;
        public string Body;
        public float Growth;
        public float Health;
        public float Ast;
        public float Alt;
        public float Bilirubin;
        public bool Complication;

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
                    "Reperfüzyon ve erken greft fonksiyonu",
                    "Ameliyatın hemen ardından grefte kan akışı (reperfüzyon) yeniden başlar. Bu süreçte hepatositler " +
                    "oksijen stresine yanıt verir; serum AST ve ALT düzeyleri geçici olarak belirgin yükselir " +
                    "(iskemi-reperfüzyon yaralanması). Bilirubin de yükselmiş olabilir.\n\n" +
                    "Tipik bulgular: AST/ALT çoğu merkezde ilk günlerde referans üstü; total bilirubin 2–6 mg/dL " +
                    "aralığında görülebilir. Portal ven ve hepatik arter anastomozunun açık olduğu Doppler USG ile " +
                    "doğrulanır. Kalsinörin inhibitörü (ör. takrolimus) ve mikofenolat ile immünosupresyon başlatılır.",
                    growth: 0.58f, health: 68f, ast: 520f, alt: 480f, bilirubin: 4.8f),

                new JourneyStep(
                    "1. hafta",
                    "Hızlı transaminaz düşüşü",
                    "İlk 5–7 günde AST ve ALT genellikle her gün belirgin azalır; bu, hepatosit membranının onarıldığının " +
                    "laboratuvar göstergesidir. Total bilirubin de çoğu hastada gerilemeye başlar.\n\n" +
                    "Tipik bulgular: AST/ALT hâlâ yüksek olabilir ancak zirve geçmiştir; bilirubin 1,5–3 mg/dL " +
                    "civarına inebilir. Erken dönemde akut hücresel red (ACR) taraması için rutin kan örnekleri " +
                    "alınır. İlaç düzeyi (takrolimus/siklosporin trough) ve böbrek fonksiyonu yakından izlenir.",
                    growth: 0.74f, health: 78f, ast: 145f, alt: 160f, bilirubin: 2.4f),

                new JourneyStep(
                    "1. ay",
                    "Greftin metabolik işlevlerinin yerleşmesi",
                    "Karaciğer sentetik fonksiyonları (albumin, pıhtılaşma faktörleri) ve detoksifikasyon kapasitesi " +
                    "giderek normale yaklaşır. Transaminazlar çoğu hastada referans sınırına veya hafif üstüne iner.\n\n" +
                    "Tipik bulgular: AST 40–80 U/L, ALT 45–90 U/L, bilirubin genelde <2 mg/dL. İmmünosupresif dozlar " +
                    "tahlil sonuçlarına göre kademeli azaltılabilir. CMV profilaksisi, antibiyotik ve beslenme protokolü " +
                    "ekip tarafından sürdürülür. Ateş, safranın koyulaşması veya karın ağrısı erken bildirilmelidir.",
                    growth: 0.88f, health: 86f, ast: 58f, alt: 68f, bilirubin: 1.5f),

                new JourneyStep(
                    "3. ay",
                    "Stabil greft fonksiyonu",
                    "Üçüncü ayda çoğu hastada karaciğer fonksiyon testleri uzun süreli normal aralığa yakınsar. " +
                    "Erken dönem komplikasyon riski azalır; takip aralıkları genişletilebilir.\n\n" +
                    "Tipik bulgular: AST/ALT çoğunlukla <40 U/L; bilirubin <1,2 mg/dL. BK virüsü, CMV ve biliary " +
                    "komplikasyonlar için rutin tarama devam eder. Metabolik sendrom, hipertansiyon ve nefrotoksisite " +
                    "(kalsinörin inhibitörü kaynaklı) açısından izlem önemlidir.",
                    growth: 0.96f, health: 92f, ast: 32f, alt: 36f, bilirubin: 0.9f),

                new JourneyStep(
                    "6–12. ay",
                    "Uzun dönem greft sağkalımı",
                    "Altıncı–on ikinci ayda greft çoğu hastada kronik rejimde çalışır. İmmünosupresyon kişiselleştirilir; " +
                    "amaç redi önlerken enfeksiyon ve ilaç toksisitesi riskini dengelemektir.\n\n" +
                    "Tipik bulgular: AST 15–35 U/L, ALT 15–40 U/L, bilirubin 0,3–1,0 mg/dL. Geç red, kronik rejeksiyon " +
                    "ve malignite taraması uzun dönem takibin parçasıdır. Düzenli kontroller, ilaç uyumu ve enfeksiyondan " +
                    "korunma greft ömrünü uzatan temel faktörlerdir.",
                    growth: 1.0f, health: 97f, ast: 24f, alt: 28f, bilirubin: 0.7f)
            };
        }
    }
}
