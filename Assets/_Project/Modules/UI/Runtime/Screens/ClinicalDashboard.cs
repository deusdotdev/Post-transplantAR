using LiverAR.Modules.Simulation.Runtime;
using LiverAR.Modules.Simulation.Runtime.Data;
using LiverAR.Modules.UI.Runtime;
using LiverAR.Modules.UI.Runtime.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>Klinik değerler, sağlık çubuğu ve uyarı bandı.</summary>
    public sealed class ClinicalDashboard : MonoBehaviour
    {
        [SerializeField] private SimulationController controller;
        [SerializeField] private Image healthFill;
        [SerializeField] private Text healthLabel;
        [SerializeField] private Text astText;
        [SerializeField] private Text altText;
        [SerializeField] private Text bilirubinText;
        [SerializeField] private GameObject warningRoot;
        [SerializeField] private Text warningText;
        [SerializeField] private GameObject medicationReminderRoot;
        [SerializeField] private Text medicationReminderLabel;
        [SerializeField] private Toggle medicationTakenToggle;

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.StateChanged += Refresh;
            }

            if (medicationTakenToggle != null)
            {
                medicationTakenToggle.onValueChanged.AddListener(OnMedicationToggle);
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.StateChanged -= Refresh;
            }

            if (medicationTakenToggle != null)
            {
                medicationTakenToggle.onValueChanged.RemoveListener(OnMedicationToggle);
            }
        }

        private void OnMedicationToggle(bool taken)
        {
            controller?.SetMedicationAdherence(taken);
            ApplyMedicationLabel(taken);
        }

        private void ApplyMedicationLabel(bool taken)
        {
            if (medicationReminderLabel == null)
            {
                return;
            }

            medicationReminderLabel.text = taken
                ? "Bugün immünosupresif ilacımı aldım"
                : "İlacı atladım / geciktirdim";
            medicationReminderLabel.color = taken ? UITheme.TextPrimary : UITheme.Warning;
        }

        public void Refresh()
        {
            var state = controller?.State;
            if (state == null || state.CurrentScenario == ScenarioType.None)
            {
                SetPanelActive(false);
                return;
            }

            SetPanelActive(true);
            UpdateHealth(state);
            UpdateLabs(state);
            UpdateWarning(state);
            UpdateMedicationReminder(state);
        }

        private void SetPanelActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void UpdateHealth(SimulationState state)
        {
            var t = Mathf.Clamp01(state.HealthPoints / 100f);
            if (healthFill != null)
            {
                healthFill.fillAmount = t;
                healthFill.color = t > 0.6f
                    ? UITheme.HealthGood
                    : t > 0.3f
                        ? UITheme.HealthMid
                        : UITheme.HealthBad;
            }

            if (healthLabel != null)
            {
                healthLabel.text = $"Genel durum: %{Mathf.RoundToInt(state.HealthPoints)}";
                healthLabel.color = UITheme.TextPrimary;
            }
        }

        private void UpdateLabs(SimulationState state)
        {
            SetLab(astText, "AST", state.AST, "U/L", state.AST > 80f);
            SetLab(altText, "ALT", state.ALT, "U/L", state.ALT > 80f);
            SetLab(bilirubinText, "Bilirubin", state.Bilirubin, "mg/dL", state.Bilirubin > 2f);
        }

        private static void SetLab(Text target, string name, float value, string unit, bool alert)
        {
            if (target == null)
            {
                return;
            }

            target.text = $"{name} {value:F1} {unit}";
            target.color = alert ? UITheme.LabAlert : UITheme.LabNormal;
        }

        private void UpdateWarning(SimulationState state)
        {
            if (warningRoot == null || warningText == null)
            {
                return;
            }

            string message = null;
            if (state.ImmuneAttack)
            {
                message = "Olası red (bağışıklık saldırısı) — ilaç planınızı ekibinizle gözden geçirin.";
            }
            else if (state.HealthPoints < 30f)
            {
                message = "Genel durum çok düşük — acil tıbbi değerlendirme gerekebilir.";
            }
            else if (state.Bilirubin > 4f)
            {
                message = "Bilirubin yüksek — sarılık riski; doktorunuza bildirin.";
            }

            var show = !string.IsNullOrEmpty(message);
            warningRoot.SetActive(show);
            if (show)
            {
                warningText.text = message;
                warningText.color = UITheme.TextOnDanger;
                UiLayer.BringToFront(warningRoot);
            }
        }

        private void UpdateMedicationReminder(SimulationState state)
        {
            if (medicationReminderRoot == null)
            {
                return;
            }

            var show = state.CurrentScenario == ScenarioType.Medication;
            medicationReminderRoot.SetActive(show);
            if (!show || medicationTakenToggle == null)
            {
                return;
            }

            medicationTakenToggle.SetIsOnWithoutNotify(state.IsAdherent);
            ApplyMedicationLabel(state.IsAdherent);
        }
    }
}
