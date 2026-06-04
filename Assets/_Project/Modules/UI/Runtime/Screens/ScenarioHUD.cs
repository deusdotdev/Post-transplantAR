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

            return $"Büyüme: %{Mathf.RoundToInt(state.GrowthPercentage * 100f)}\n" +
                   $"Sağlık: {Mathf.RoundToInt(state.HealthPoints)}\n" +
                   $"AST: {state.AST:F0} U/L   ALT: {state.ALT:F0} U/L\n" +
                   $"Bilirubin: {state.Bilirubin:F1} mg/dL";
        }
    }
}
