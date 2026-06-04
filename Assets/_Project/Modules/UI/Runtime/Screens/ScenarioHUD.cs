using LiverAR.Modules.Simulation.Runtime;
using LiverAR.Modules.Simulation.Runtime.Data;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// Senaryo başlığı/açıklaması ve klinik değerleri gösterir; buton aksiyonlarını
    /// SimulationController'a iletir. Yalnızca StateChanged event'inde güncellenir.
    /// </summary>
    public sealed class ScenarioHUD : MonoBehaviour
    {
        [SerializeField] private SimulationController controller;

        [Header("Metinler")]
        [SerializeField] private Text headerText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text clinicalText;
        [SerializeField] private ClinicalDashboard clinicalDashboard;

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.StateChanged += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.StateChanged -= Refresh;
            }
        }

        // --- Buton aksiyonları (Inspector onClick ile bağlanır) ---

        public void OnRecoverySelected()
        {
            controller?.StartScenario(ScenarioType.Recovery);
        }

        public void OnMedicationSelected()
        {
            controller?.StartScenario(ScenarioType.Medication);
        }

        public void OnNextWeek()
        {
            controller?.AdvanceWeek();
        }

        public void OnTakeMedication()
        {
            controller?.SetMedicationAdherence(true);
        }

        public void OnSkipMedication()
        {
            controller?.SetMedicationAdherence(false);
        }

        public void OnRejectionSelected()
        {
            controller?.StartScenario(ScenarioType.Rejection);
        }

        public void OnAdvanceRejection()
        {
            controller?.AdvanceRejection();
        }

        public void OnLifestyleSelected()
        {
            controller?.StartScenario(ScenarioType.Lifestyle);
        }

        public void OnHealthyLifestyle()
        {
            controller?.SetLifestyle(true);
        }

        public void OnFattyDiet()
        {
            controller?.SetLifestyle(false);
        }

        private void Refresh()
        {
            if (controller == null)
            {
                return;
            }

            if (headerText != null)
            {
                headerText.text = controller.GetHeader();
            }

            if (descriptionText != null)
            {
                descriptionText.text = controller.GetDescription();
            }

            if (clinicalDashboard != null)
            {
                clinicalDashboard.Refresh();
                if (clinicalText != null)
                {
                    clinicalText.gameObject.SetActive(false);
                }
            }
            else if (clinicalText != null)
            {
                clinicalText.gameObject.SetActive(true);
                clinicalText.text = BuildClinicalText(controller.State);
            }
        }

        private static string BuildClinicalText(SimulationState state)
        {
            if (state == null || state.CurrentScenario == ScenarioType.None)
            {
                return string.Empty;
            }

            var text = $"Büyüme: %{Mathf.RoundToInt(state.GrowthPercentage * 100f)}\n" +
                       $"Sağlık: {Mathf.RoundToInt(state.HealthPoints)}\n" +
                       $"AST: {state.AST:F0} U/L   ALT: {state.ALT:F0} U/L\n" +
                       $"Bilirubin: {state.Bilirubin:F1} mg/dL";

            if (state.CurrentScenario == ScenarioType.Rejection)
            {
                text += $"\nDamar tıkanıklığı: %{Mathf.RoundToInt(state.VascularOcclusion * 100f)}" +
                        $"\nFibrozis: %{Mathf.RoundToInt(state.FibrosisFactor * 100f)}";
            }
            else if (state.CurrentScenario == ScenarioType.Lifestyle)
            {
                text += $"\nBeslenme x{state.NutritionMultiplier:F1}   Egzersiz x{state.ExerciseMultiplier:F1}";
            }

            return text;
        }
    }
}
