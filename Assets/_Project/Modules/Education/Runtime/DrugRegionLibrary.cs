using LiverAR.Modules.Visuals.Runtime;

namespace LiverAR.Modules.Education.Runtime
{
    /// <summary>Bir ilacın belirli bir karaciğer bölgesine etkisi (eğitim amaçlı, tanı koymaz).</summary>
    public sealed class DrugRegionEffect
    {
        public LiverRegionId Region;
        public string EffectText;
        public string ShortLabel;
        public bool Positive;

        public DrugRegionEffect(LiverRegionId region, string effectText, bool positive = true,
            string shortLabel = null)
        {
            Region = region;
            EffectText = effectText;
            ShortLabel = shortLabel;
            Positive = positive;
        }
    }

    public sealed class DrugInfo
    {
        public string Name;
        public string Summary;
        public string MissedSummary;
        public DrugRegionEffect[] Effects;
        public DrugRegionEffect[] MissedEffects;

        public DrugInfo(string name, string summary, DrugRegionEffect[] effects,
            string missedSummary = null, DrugRegionEffect[] missedEffects = null)
        {
            Name = name;
            Summary = summary;
            Effects = effects;
            MissedSummary = missedSummary;
            MissedEffects = missedEffects;
        }
    }

    public sealed class RegionInfo
    {
        public LiverRegionId Region;
        public string Name;
        public string Text;

        public RegionInfo(LiverRegionId region, string name, string text)
        {
            Region = region;
            Name = name;
            Text = text;
        }
    }

    /// <summary>
    /// İlaç -> bölge eşleşmeleri ve bölge anatomi metinleri. İçerik eğitim amaçlı taslaktır;
    /// gerçek doz/karar için transplant ekibine danışılmalıdır.
    /// </summary>
    public static class DrugRegionLibrary
    {
        public static DrugInfo[] GetDrugs()
        {
            return new[]
            {
                new DrugInfo(
                    "Takrolimus",
                    "Düzenli alındığında: bağışıklık baskılayıcı olarak grefti korur. " +
                    "Saatinde ve düzenli alınması en kritik ilaçtır.",
                    new[]
                    {
                        new DrugRegionEffect(LiverRegionId.RightLobe,
                            "Greft dokusunu bağışıklık saldırısından korur."),
                        new DrugRegionEffect(LiverRegionId.LeftLobe,
                            "Red riskini azaltarak işlevin sürmesini sağlar.")
                    },
                    missedSummary: "Atlanırsa: kan düzeyi düşebilir; akut red riski artabilir. " +
                                   "Ateş, halsizlik veya karaciğer testlerinde yükselme olursa ekibinize bildirin. " +
                                   "(Eğitim amaçlı senaryo; tanı koymaz.)",
                    missedEffects: new[]
                    {
                        new DrugRegionEffect(LiverRegionId.RightLobe,
                            "Bağışıklık bu bölgeye saldırabilir; akut red riski artar.",
                            positive: false, shortLabel: "Red riski ↑"),
                        new DrugRegionEffect(LiverRegionId.LeftLobe,
                            "İşlev düşüşü ve hastaneye yatış gerekebilir.",
                            positive: false, shortLabel: "İşlev ↓")
                    }),

                new DrugInfo(
                    "Mikofenolat",
                    "Düzenli alındığında: takrolimus ile birlikte bağışıklık yanıtını baskılayarak reddi önlemeye yardım eder.",
                    new[]
                    {
                        new DrugRegionEffect(LiverRegionId.LeftLobe,
                            "Bağışıklık yanıtını baskılayarak greft dokusunu destekler.")
                    },
                    missedSummary: "Atlanırsa: immün baskılama zayıflar; red ataklarına zemin hazırlanabilir. " +
                                   "Doz atlamalarını ekibinize bildirin.",
                    missedEffects: new[]
                    {
                        new DrugRegionEffect(LiverRegionId.LeftLobe,
                            "Greft dokusunda iltihaplanma ve red riski artabilir.",
                            positive: false, shortLabel: "İltihap riski")
                    }),

                new DrugInfo(
                    "Kortikosteroid",
                    "Düzenli alındığında: erken dönemde iltihabı azaltır; doz zamanla ekibince düşürülür.",
                    new[]
                    {
                        new DrugRegionEffect(LiverRegionId.RightLobe,
                            "Nakil sonrası iltihaplanmayı baskılar."),
                        new DrugRegionEffect(LiverRegionId.LeftLobe,
                            "Erken red ataklarını yatıştırmaya yardımcı olur.")
                    },
                    missedSummary: "Atlanırsa (ekip onayı olmadan): iltihap kontrolü zayıflayabilir; " +
                                   "red belirtileri alevlenebilir. Doz değişikliğini kendi başınıza yapmayın.",
                    missedEffects: new[]
                    {
                        new DrugRegionEffect(LiverRegionId.RightLobe,
                            "İltihap baskısı azalır; bölgede hassasiyet artabilir.",
                            positive: false, shortLabel: "İltihap ↑"),
                        new DrugRegionEffect(LiverRegionId.LeftLobe,
                            "Erken red bulguları belirginleşebilir.",
                            positive: false, shortLabel: "Red riski")
                    }),

                new DrugInfo(
                    "Ursodeoksikolik asit",
                    "Düzenli alındığında: safra akışını kolaylaştırır ve safra yollarını korur.",
                    new[]
                    {
                        new DrugRegionEffect(LiverRegionId.BileDuct,
                            "Safranın akışını rahatlatır, tıkanma/sarılık riskini azaltır.")
                    },
                    missedSummary: "Atlanırsa: safra akışı zorlaşabilir; sarılık ve kaşıntı riski artabilir.",
                    missedEffects: new[]
                    {
                        new DrugRegionEffect(LiverRegionId.BileDuct,
                            "Safra birikimi ve sarılık riski artabilir.",
                            positive: false, shortLabel: "Sarılık riski")
                    }),

                new DrugInfo(
                    "Pıhtı önleyici",
                    "Düzenli alındığında: damar bağlantısında pıhtı oluşmasını önleyerek kan akışını sürdürür.",
                    new[]
                    {
                        new DrugRegionEffect(LiverRegionId.VesselInlet,
                            "Damar girişinde pıhtılaşmayı önler, dokunun kanlanmasını korur.")
                    },
                    missedSummary: "Atlanırsa: damar girişinde pıhtı riski artabilir; greft kanlanması bozulabilir. " +
                                   "Kanama veya morarma fark ederseniz acil ekibinize ulaşın.",
                    missedEffects: new[]
                    {
                        new DrugRegionEffect(LiverRegionId.VesselInlet,
                            "Portal/hepatik akım azalabilir; greft iskemisi riski artar.",
                            positive: false, shortLabel: "Kan akışı ↓")
                    })
            };
        }

        public static RegionInfo[] GetRegions()
        {
            return new[]
            {
                new RegionInfo(LiverRegionId.RightLobe, "Sağ lob",
                    "Karaciğerin en büyük bölümüdür ve gövdenin sağ tarafında yer alır. Kanı süzerek toksinleri " +
                    "temizler, ilaçları parçalar, protein sentezler ve enerji depolar. Nakil sonrası bu bölgenin " +
                    "düzenli çalışması, günlük yaşam kaliten ve ilaçların etkili olması için temel bir gerekliliktir."),
                new RegionInfo(LiverRegionId.LeftLobe, "Sol lob",
                    "Sağ loba göre daha küçük olsa da metabolik işlevlere önemli katkı sağlar. Nakilde greftin " +
                    "önemli bir kısmı buradan gelir ve zamanla yenilenir. Ekip, AST/ALT gibi kan değerleriyle " +
                    "bu bölgenin iyileşmesini düzenli olarak takip eder."),
                new RegionInfo(LiverRegionId.BileDuct, "Safra yolları",
                    "Karaciğerden üretilen safra, bu bölgedeki kanallar aracılığıyla safra kesesi ve ince bağırsağa " +
                    "taşınır. Darlık veya tıkanma olduğunda ciltte sararma, koyu renkli idrar ve kaşıntı görülebilir. " +
                    "Nakil sonrası safra yolları yakından izlenir; erken müdahale sarılık riskini azaltmaya yardımcı olur."),
                new RegionInfo(LiverRegionId.VesselInlet, "Damar girişi (porta hepatis)",
                    "Portal ven ve hepatik arter gibi hayati damarlar karaciğere bu giriş bölgesinden ulaşır. " +
                    "Nakilde damarların bağlandığı nokta (anastomoz) greftin kanlanması için kritiktir. Bu bölgede " +
                    "kan akımında sorun olursa karaciğer fonksiyonları hızla etkilenebilir; bu yüzden erken uyarı " +
                    "belirtileri ekip tarafından dikkatle izlenir.")
            };
        }
    }
}
