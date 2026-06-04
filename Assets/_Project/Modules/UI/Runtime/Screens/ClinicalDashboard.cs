using LiverAR.Modules.Simulation.Runtime;
using LiverAR.Modules.Simulation.Runtime.Data;
using LiverAR.Modules.UI.Runtime;
using LiverAR.Modules.UI.Runtime.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>Klinik değerler, sağlık çubuğu ve uyarı bandı (referans DashboardUI fikri, RAMS Safety).</summary>
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
        [SerializeField] private GameObject rejectionDetailRoot;
        [SerializeField] private Text symptomText;
        [SerializeField] private Text clinicalDetailText;
        [SerializeField] private Text actionText;
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
            // Yalnızca kullanıcı toggle'a dokunduğunda simülasyonu güncelle.
            // Refresh içinden OnMedicationToggle çağrılırsa sonsuz döngü → Unity çöker.
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
                ? "Bugün immünosupresif ilacını aldım"
                : "İlacı atladım — risk artar";
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
            UpdateRejectionDetail(state);
            UpdateMedicationReminder(state);
        }

        private void SetPanelActive(bool active)
        {
            if (healthFill != null && healthFill.transform.parent != null)
            {
                healthFill.transform.parent.gameObject.SetActive(active);
            }
        }

        private void UpdateHealth(SimulationState state)
        {
            var t = Mathf.Clamp01(state.HealthPoints / 100f);
            if (healthFill != null)
            {
                healthFill.fillAmount = t;
                healthFill.color = Color.Lerp(UITheme.HealthBad, UITheme.HealthGood, t);
            }

            if (healthLabel != null)
            {
                healthLabel.text = $"Organ sağlığı %{Mathf.RoundToInt(state.HealthPoints)}";
            }
        }

        private void UpdateLabs(SimulationState state)
        {
            SetLab(astText, $"AST {state.AST:F0}", state.AST > 100f);
            SetLab(altText, $"ALT {state.ALT:F0}", state.ALT > 100f);
            SetLab(bilirubinText, $"Bili {state.Bilirubin:F1}", state.Bilirubin > 2f);
        }

        private static void SetLab(Text target, string value, bool critical)
        {
            if (target == null)
            {
                return;
            }

            target.text = value;
            target.color = critical ? UITheme.Danger : UITheme.TextMuted;
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
                message = "Bağışıklık yanıtı — doktorunuza danışın.";
            }
            else if (state.HealthPoints < 30f)
            {
                message = "Kritik düşük organ sağlığı — acil tıbbi destek gerekebilir.";
            }
            else if (state.IsRejecting && state.RejectionStage >= 2)
            {
                message = "Akut red bulguları — transplant ekibinizi arayın.";
            }

            var show = !string.IsNullOrEmpty(message);
            warningRoot.SetActive(show);
            if (show)
            {
                warningText.text = message;
                UiLayer.BringToFront(warningRoot);
            }
        }

        private void UpdateRejectionDetail(SimulationState state)
        {
            if (rejectionDetailRoot == null)
            {
                return;
            }

            var show = state.CurrentScenario == ScenarioType.Rejection;
            rejectionDetailRoot.SetActive(show);
            if (show)
            {
                UiLayer.BringToFront(rejectionDetailRoot);
            }

            if (!show || controller == null)
            {
                return;
            }

            controller.GetRejectionPanels(
                out var symptom,
                out var clinical,
                out var action);

            if (symptomText != null)
            {
                symptomText.text = symptom;
            }

            if (clinicalDetailText != null)
            {
                clinicalDetailText.text = clinical;
            }

            if (actionText != null)
            {
                actionText.text = action;
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
