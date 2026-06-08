namespace LiverAR.Modules.Education.Runtime
{
    public sealed class NutritionItem
    {
        public string Text;
        public bool Recommended; // true: yap / öneri, false: kaçın

        public NutritionItem(string text, bool recommended)
        {
            Text = text;
            Recommended = recommended;
        }
    }

    public sealed class NutritionGroup
    {
        public string Title;
        public NutritionItem[] Items;

        public NutritionGroup(string title, NutritionItem[] items)
        {
            Title = title;
            Items = items;
        }
    }

    /// <summary>
    /// Nakil sonrası beslenme önerileri (eğitim amaçlı taslak; tanı/diyet kararı için ekibe danışın).
    /// </summary>
    public static class NutritionLibrary
    {
        public static NutritionGroup[] GetGroups()
        {
            return new[]
            {
                new NutritionGroup("Gıda güvenliği (bağışıklık baskılı dönem)", new[]
                {
                    new NutritionItem("Eti, tavuğu ve deniz ürünlerini iyice pişirin.", true),
                    new NutritionItem("Sebze ve meyveleri bol suyla yıkayın.", true),
                    new NutritionItem("Çiğ/az pişmiş et, çiğ yumurta ve çiğ deniz ürünlerinden kaçının.", false),
                    new NutritionItem("Pastörize edilmemiş süt ve peynirlerden kaçının.", false)
                }),

                new NutritionGroup("İlaç–besin etkileşimi", new[]
                {
                    new NutritionItem("Greyfurt ve greyfurt suyundan kaçının (ilaç düzeyini bozar).", false),
                    new NutritionItem("Bitkisel takviyeleri (özellikle sarı kantaron) ekibe sormadan kullanmayın.", false),
                    new NutritionItem("İlaçlarınızı her gün aynı saatte alın.", true)
                }),

                new NutritionGroup("Protein ve iyileşme", new[]
                {
                    new NutritionItem("İyileşme için yeterli protein alın (yumurta, tavuk, balık, baklagil).", true),
                    new NutritionItem("Öğünleri düzenli ve dengeli tutun.", true),
                    new NutritionItem("Aşırı protein takviyelerini ekibe sormadan kullanmayın.", false)
                }),

                new NutritionGroup("Sıvı, tuz ve şeker", new[]
                {
                    new NutritionItem("Ekibinizin önerdiği miktarda su için.", true),
                    new NutritionItem("Aşırı tuzdan kaçının (tansiyon/ödem riski).", false),
                    new NutritionItem("Kortizon kullanırken şeker ve kiloyu takip edin.", true)
                }),

                new NutritionGroup("Kaçınılması gerekenler", new[]
                {
                    new NutritionItem("Alkol tüketmeyin.", false),
                    new NutritionItem("İşlenmiş, çok yağlı ve aşırı şekerli gıdaları sınırlayın.", false),
                    new NutritionItem("Açık büfelerden ve uzun süre oda sıcaklığında bekleyen yiyeceklerden kaçının.", false)
                }),

                new NutritionGroup("Lif, sindirim ve kabızlık", new[]
                {
                    new NutritionItem("Tam tahıllar, sebze ve meyve ile yeterli lif alın.", true),
                    new NutritionItem("Günde düzenli hafif hareket sindirime yardımcı olur.", true),
                    new NutritionItem("Kabızlık için yüksek doz lif takviyesine kendi başınıza başlamayın.", false)
                }),

                new NutritionGroup("Kemik sağlığı ve vitaminler", new[]
                {
                    new NutritionItem("Kalsiyum ve D vitamini düzeylerinizi ekibinizle takip edin.", true),
                    new NutritionItem("Süt, yoğurt ve peynir gibi kalsiyum kaynaklarını planlı tüketin.", true),
                    new NutritionItem("Yüksek doz vitamin/mineral kombinasyonlarını onaysız kullanmayın.", false)
                }),

                new NutritionGroup("Öğün düzeni ve porsiyon", new[]
                {
                    new NutritionItem("Günde 4–6 küçük öğünle kan şekerini daha dengeli tutun.", true),
                    new NutritionItem("Porsiyonları küçük tabakla kontrol edin.", true),
                    new NutritionItem("Gece geç saatlerde ağır ve yağlı tek öğünlerden kaçının.", false)
                })
            };
        }
    }
}
