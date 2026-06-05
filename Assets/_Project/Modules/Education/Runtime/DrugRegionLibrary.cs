using LiverAR.Modules.Visuals.Runtime;

namespace LiverAR.Modules.Education.Runtime
{
    /// <summary>Bir ilacın belirli bir karaciğer bölgesine etkisi (eğitim amaçlı, tanı koymaz).</summary>
    public sealed class DrugRegionEffect
    {
        public LiverRegionId Region;
        public string EffectText;
        public bool Positive;

        public DrugRegionEffect(LiverRegionId region, string effectText, bool positive = true)
        {
            Region = region;
            EffectText = effectText;
            Positive = positive;
        }
    }

    public sealed class DrugInfo
    {
        public string Name;
        public string Summary;
        public DrugRegionEffect[] Effects;

        public DrugInfo(string name, string summary, DrugRegionEffect[] effects)
        {
            Name = name;
            Summary = summary;
            Effects = effects;
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
                    "Bağışıklık baskılayıcı. Vücudun nakledilen karaciğere saldırmasını (reddi) önler. " +
                    "Saatinde ve düzenli alınması en kritik ilaçtır.",
                    new[]
                    {
                        new DrugRegionEffect(LiverRegionId.RightLobe,
                            "Greft dokusunu bağışıklık saldırısından korur."),
                        new DrugRegionEffect(LiverRegionId.LeftLobe,
                            "Red riskini azaltarak işlevin sürmesini sağlar.")
                    }),

                new DrugInfo(
                    "Mikofenolat",
                    "İkinci bir bağışıklık baskılayıcı. Takrolimus ile birlikte reddi önlemeye yardım eder.",
                    new[]
                    {
                        new DrugRegionEffect(LiverRegionId.LeftLobe,
                            "Bağışıklık yanıtını baskılayarak greft dokusunu destekler.")
                    }),

                new DrugInfo(
                    "Kortikosteroid",
                    "Erken dönemde iltihabı azaltır; doz zamanla ekibince düşürülür.",
                    new[]
                    {
                        new DrugRegionEffect(LiverRegionId.RightLobe,
                            "Nakil sonrası iltihaplanmayı baskılar."),
                        new DrugRegionEffect(LiverRegionId.LeftLobe,
                            "Erken red ataklarını yatıştırmaya yardımcı olur.")
                    }),

                new DrugInfo(
                    "Ursodeoksikolik asit",
                    "Safra akışını kolaylaştırır ve safra yollarını korur.",
                    new[]
                    {
                        new DrugRegionEffect(LiverRegionId.BileDuct,
                            "Safranın akışını rahatlatır, tıkanma/sarılık riskini azaltır.")
                    }),

                new DrugInfo(
                    "Pıhtı önleyici",
                    "Damar bağlantısında (anastomoz) pıhtı oluşmasını önleyerek kan akışını sürdürür.",
                    new[]
                    {
                        new DrugRegionEffect(LiverRegionId.VesselInlet,
                            "Damar girişinde pıhtılaşmayı önler, dokunun kanlanmasını korur.")
                    })
            };
        }

        public static RegionInfo[] GetRegions()
        {
            return new[]
            {
                new RegionInfo(LiverRegionId.RightLobe, "Sağ lob",
                    "Karaciğerin yaklaşık dörtte üçü. Kanı süzer, zararlı maddeleri parçalar, enerji depolar."),
                new RegionInfo(LiverRegionId.LeftLobe, "Sol lob",
                    "Daha küçük bölüm. Nakilde greftin önemli bir kısmı buradan gelebilir; zamanla yenilenir."),
                new RegionInfo(LiverRegionId.BileDuct, "Safra yolları",
                    "Safrayı bağırsağa taşır. Burada darlık/tıkanma sarılığa yol açabilir."),
                new RegionInfo(LiverRegionId.VesselInlet, "Damar girişi",
                    "Karaciğere kan getiren damarların ve anastomozun bulunduğu bölge; kanlanma için kritiktir.")
            };
        }
    }
}
