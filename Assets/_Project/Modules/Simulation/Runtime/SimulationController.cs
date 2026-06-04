using System;
using System.Collections;
using LiverAR.Modules.Simulation.Runtime.Data;
using UnityEngine;

namespace LiverAR.Modules.Simulation.Runtime
{
    /// <summary>
    /// Senaryo mantığını yürütür. Her frame yerine yalnızca anlamlı bir değişiklik
    /// olduğunda <see cref="StateChanged"/> tetikler (yaprakasln örneğindeki
    /// her-frame string üretimi maliyetinden kaçınmak için event-driven tasarım).
    /// </summary>
    public sealed class SimulationController : MonoBehaviour
    {
        [SerializeField] private SimulationState state;

        [Tooltip("İlaç uyumsuzluğunda kötüleşmenin saniyelik tik aralığı.")]
        [SerializeField] private float declineTickSeconds = 1f;

        /// <summary>Durumda anlamlı bir değişiklik olduğunda tetiklenir.</summary>
        public event Action StateChanged;

        public SimulationState State => state;

        private Coroutine _declineRoutine;

        public void StartScenario(ScenarioType scenario)
        {
            if (state == null)
            {
                return;
            }

            state.ResetToDefault();
            state.CurrentScenario = scenario;
            RecalculateRecoveryTargets();
            RestartDeclineRoutine();
            RaiseChanged();
        }

        public void AdvanceWeek()
        {
            if (state == null || state.CurrentScenario == ScenarioType.None)
            {
                return;
            }

            state.SimulationWeek = Mathf.Min(state.SimulationWeek + 1, 8);
            RecalculateRecoveryTargets();
            RaiseChanged();
        }

        public void SetMedicationAdherence(bool adherent)
        {
            if (state == null)
            {
                return;
            }

            state.IsAdherent = adherent;
            state.ImmuneAttack = !adherent;
            RestartDeclineRoutine();
            RaiseChanged();
        }

        /// <summary>Red senaryosunda bir sonraki evreye geçer (erken -> akut -> kronik).</summary>
        public void AdvanceRejection()
        {
            if (state == null || state.CurrentScenario != ScenarioType.Rejection)
            {
                return;
            }

            state.RejectionStage = Mathf.Min(state.RejectionStage + 1, 3);
            state.IsRejecting = state.RejectionStage > 0;
            state.ImmuneAttack = state.RejectionStage >= 2;

            var t = state.RejectionStage / 3f;
            state.VascularOcclusion = t;
            state.FibrosisFactor = Mathf.Clamp01(t * 0.9f);
            state.Bilirubin = Mathf.Lerp(0.8f, 7.5f, t);
            state.AST = Mathf.Lerp(25f, 350f, t);
            state.ALT = Mathf.Lerp(30f, 320f, t);
            state.HealthPoints = Mathf.Lerp(100f, 20f, t);

            RaiseChanged();
        }

        /// <summary>Yaşam tarzı senaryosunda sağlıklı/yağlı tercihini uygular.</summary>
        public void SetLifestyle(bool healthy)
        {
            if (state == null)
            {
                return;
            }

            state.IsFattyDiet = !healthy;
            state.NutritionMultiplier = healthy ? 1.2f : 0.8f;
            state.ExerciseMultiplier = healthy ? 1.2f : 0.7f;

            if (healthy)
            {
                state.HealthPoints = Mathf.Min(100f, state.HealthPoints + 10f);
                state.GrowthPercentage = Mathf.Min(1f, state.GrowthPercentage + 0.05f);
                state.Bilirubin = Mathf.Max(0.8f, state.Bilirubin - 0.3f);
            }
            else
            {
                state.HealthPoints = Mathf.Max(0f, state.HealthPoints - 8f);
                state.Bilirubin = Mathf.Min(8f, state.Bilirubin + 0.4f);
            }

            RaiseChanged();
        }

        public string GetHeader()
        {
            if (state == null)
            {
                return string.Empty;
            }

            switch (state.CurrentScenario)
            {
                case ScenarioType.Recovery:
                    return $"Onarım Süreci - Hafta {state.SimulationWeek}";
                case ScenarioType.Medication:
                    return state.IsAdherent ? "İlaç Uyumu - Düzenli" : "İlaç Uyumu - Aksatıldı";
                case ScenarioType.Rejection:
                    return $"Red / Rejeksiyon - {GetRejectionStageName(state.RejectionStage)}";
                case ScenarioType.Lifestyle:
                    return state.IsFattyDiet ? "Yaşam Tarzı - Yağlı Diyet" : "Yaşam Tarzı - Sağlıklı";
                default:
                    return "Senaryo Seçimi";
            }
        }

        private static string GetRejectionStageName(int stage)
        {
            switch (stage)
            {
                case 0: return "Stabil";
                case 1: return "Erken";
                case 2: return "Akut";
                default: return "Kronik";
            }
        }

        public string GetDescription()
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
                        ? "İlaçlar düzenli alındığında bağışıklık baskılanır, yeni karaciğer reddedilmez ve iyileşme sürer."
                        : "DİKKAT: İlaç aksatıldığında bağışıklık sistemi organa saldırır; sararma ve fonksiyon kaybı başlar.";
                case ScenarioType.Rejection:
                    return GetRejectionDescription(state.RejectionStage);
                case ScenarioType.Lifestyle:
                    return state.IsFattyDiet
                        ? "Yağlı diyet ve hareketsizlik karaciğerde yağlanmaya (steatoz) ve fonksiyon kaybına yol açar."
                        : "Dengeli beslenme ve düzenli egzersiz damar akışını ve doku sağlığını destekler.";
                default:
                    return "İncelemek istediğiniz nakil sonrası senaryoyu seçin.";
            }
        }

        private static string GetRejectionDescription(int stage)
        {
            switch (stage)
            {
                case 0:
                    return "Organ stabil. 'Reddi İlerlet' ile rejeksiyon sürecini aşama aşama gözlemleyin.";
                case 1:
                    return "Erken rejeksiyon: bağışıklık hücreleri damar çevresinde toplanmaya başlar (hafif ödem).";
                case 2:
                    return "Akut rejeksiyon: belirgin sararma (icterus), damar tıkanıklığı ve doku şişmesi görülür.";
                default:
                    return "Kronik rejeksiyon: fibrozis ve kalıcı damar hasarı; organ fonksiyonu ciddi düşer.";
            }
        }

        private static string GetRecoveryDescription(int week)
        {
            switch (week)
            {
                case 1:
                    return "Hafta 1: Hepatositler agresif bölünme (mitoz) döngüsüne girer. Kritik kütle artışı başlar.";
                case 2:
                    return "Hafta 2: Anjiyogenez (yeni damar oluşumu) doku beslenmesini artırır. Albumin sentezi normalleşir.";
                case 3:
                case 4:
                    return "Hafta 4: Segmental matürasyon; doku mimarisi düzenlenir, detoksifikasyon kapasitesi %90'a ulaşır.";
                default:
                    return "Hafta 8+: Tam metabolik stabilizasyon. Karaciğer fizyolojik boyutuna ulaşır.";
            }
        }

        private void RecalculateRecoveryTargets()
        {
            if (state.CurrentScenario != ScenarioType.Recovery)
            {
                return;
            }

            switch (state.SimulationWeek)
            {
                case 1: state.GrowthPercentage = 0.45f; break;
                case 2: state.GrowthPercentage = 0.65f; break;
                case 3:
                case 4: state.GrowthPercentage = 0.85f; break;
                default: state.GrowthPercentage = 1f; break;
            }

            // Klinik değerler iyileşmeyle normale yaklaşır.
            state.Bilirubin = Mathf.Lerp(2.5f, 0.8f, Mathf.InverseLerp(1, 8, state.SimulationWeek));
        }

        private void RestartDeclineRoutine()
        {
            if (_declineRoutine != null)
            {
                StopCoroutine(_declineRoutine);
                _declineRoutine = null;
            }

            var shouldDecline = state.CurrentScenario == ScenarioType.Medication && !state.IsAdherent;
            if (shouldDecline && isActiveAndEnabled)
            {
                _declineRoutine = StartCoroutine(DeclineRoutine());
            }
        }

        private IEnumerator DeclineRoutine()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.1f, declineTickSeconds));

            while (state.HealthPoints > 0f)
            {
                yield return wait;
                state.HealthPoints = Mathf.Max(0f, state.HealthPoints - 5f);
                state.Bilirubin = Mathf.Min(8f, state.Bilirubin + 0.4f);
                state.AST = Mathf.Min(400f, state.AST + 20f);
                state.ALT = Mathf.Min(400f, state.ALT + 18f);
                RaiseChanged();
            }
        }

        private void OnDisable()
        {
            if (_declineRoutine != null)
            {
                StopCoroutine(_declineRoutine);
                _declineRoutine = null;
            }
        }

        private void RaiseChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
