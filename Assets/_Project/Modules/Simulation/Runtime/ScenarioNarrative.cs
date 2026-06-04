using LiverAR.Modules.Simulation.Runtime.Data;

namespace LiverAR.Modules.Simulation.Runtime
{
    /// <summary>Senaryo başlık ve açıklama metinleri (hasta dostu, eğitim amaçlı).</summary>
    public static class ScenarioNarrative
    {
        public static string GetHeader(SimulationState state)
        {
            if (state == null)
            {
                return string.Empty;
            }

            switch (state.CurrentScenario)
            {
                case ScenarioType.Recovery:
                    return $"İyileşme süreci — {state.SimulationWeek}. hafta";
                case ScenarioType.Medication:
                    return state.IsAdherent
                        ? "İlaç uyumu — düzenli kullanım"
                        : "İlaç uyumu — atlama / gecikme";
                default:
                    return "Eğitim menüsü";
            }
        }

        public static string GetDescription(SimulationState state)
        {
            if (state == null)
            {
                return string.Empty;
            }

            switch (state.CurrentScenario)
            {
                case ScenarioType.Recovery:
                    return GetRecoveryDescription(state.SimulationWeek);
                case ScenarioType.Medication:
                    return state.IsAdherent
                        ? "İmmünosupresif ilacınızı saatinde almak, organın reddedilmesini önlemeye yardımcı olur. " +
                          "Yan etki veya doz sorularınızı ekibinize sorun; ilacı kendi başınıza bırakmayın."
                        : "İlaç atlandığında veya geciktiğinde bağışıklık sistemi nakledilen organa saldırabilir. " +
                          "Sarılık ve kan testlerinde bozulma görülebilir — hemen transplant ekibinize ulaşın.";
                default:
                    return "Nakil sonrası iyileşme veya ilaç uyumu senaryosunu seçin.";
            }
        }

        private static string GetRecoveryDescription(int week)
        {
            switch (week)
            {
                case 1:
                    return "1. hafta: Karaciğer hücreleri yenilenmeye başlar; organ boyutu kademeli artabilir. " +
                           "Bu, erken dönemde beklenen bir iyileşme bulgusudur.";
                case 2:
                    return "2. hafta: Dokuya kan taşıyan yeni damarlar oluşur. Enerji ve iştah yavaş yavaş düzelebilir.";
                case 3:
                case 4:
                    return "3–4. hafta: Doku düzeni oturur; karaciğer günlük işlevlerinin çoğunu üstlenir. " +
                           "Takip randevularınızı ve tahlillerinizi aksatmayın.";
                case 5:
                case 6:
                case 7:
                    return "5–7. hafta: İyileşme devam eder; ilaç dozları ekibinizce ayarlanabilir. " +
                           "Ateş, karın ağrısı veya sararma fark ederseniz bildirin.";
                default:
                    return "8. hafta ve sonrası: Metabolik denge yerleşir. Uzun dönem takip ve ilaç disiplini önemlidir.";
            }
        }
    }
}
