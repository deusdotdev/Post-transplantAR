using LiverAR.Modules.Simulation.Runtime;
using LiverAR.Modules.Simulation.Runtime.Data;
using UnityEngine;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// Referans FlowManager yaklaşımı: aynı anda tek panel (Ana Menü / Senaryo / Anatomi).
    /// Tüm butonlar bir ekranda değil; menüden senaryoya girilir.
    /// </summary>
    public sealed class EducationMenuController : MonoBehaviour
    {
        [SerializeField] private SimulationController controller;
        [SerializeField] private ScenarioHUD scenarioHud;

        [Header("Paneller")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject scenarioPanel;
        [SerializeField] private GameObject anatomyPanel;
        [SerializeField] private LiverAnatomyInfoPanel anatomyInfo;

        [Header("Senaryo aksiyon grupları")]
        [SerializeField] private GameObject recoveryActions;
        [SerializeField] private GameObject medicationActions;
        [SerializeField] private GameObject rejectionActions;
        [SerializeField] private GameObject lifestyleActions;

        private void Start()
        {
            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            controller?.ExitScenario();
            anatomyInfo?.Hide();
            SetPanel(mainMenuPanel, true);
            SetPanel(scenarioPanel, false);
            SetPanel(anatomyPanel, false);
        }

        public void ShowAnatomyMenu()
        {
            SetPanel(mainMenuPanel, false);
            SetPanel(scenarioPanel, false);
            SetPanel(anatomyPanel, true);
        }

        public void OpenRecovery()
        {
            scenarioHud?.OnRecoverySelected();
            ShowScenarioPanel(ScenarioType.Recovery);
        }

        public void OpenMedication()
        {
            scenarioHud?.OnMedicationSelected();
            ShowScenarioPanel(ScenarioType.Medication);
        }

        public void OpenRejection()
        {
            scenarioHud?.OnRejectionSelected();
            ShowScenarioPanel(ScenarioType.Rejection);
        }

        public void OpenLifestyle()
        {
            scenarioHud?.OnLifestyleSelected();
            ShowScenarioPanel(ScenarioType.Lifestyle);
        }

        private void ShowScenarioPanel(ScenarioType type)
        {
            SetPanel(mainMenuPanel, false);
            SetPanel(anatomyPanel, false);
            SetPanel(scenarioPanel, true);

            if (recoveryActions != null)
            {
                recoveryActions.SetActive(type == ScenarioType.Recovery);
            }

            if (medicationActions != null)
            {
                medicationActions.SetActive(type == ScenarioType.Medication);
            }

            if (rejectionActions != null)
            {
                rejectionActions.SetActive(type == ScenarioType.Rejection);
            }

            if (lifestyleActions != null)
            {
                lifestyleActions.SetActive(type == ScenarioType.Lifestyle);
            }
        }

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
    }
}
