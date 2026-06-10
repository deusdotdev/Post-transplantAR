using LiverAR.Modules.Education.Runtime;
using LiverAR.Modules.Simulation.Runtime;
using LiverAR.Modules.Simulation.Runtime.Data;
using LiverAR.Modules.Visuals.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// Nakil sonrası iyileşme yolculuğu: AR'sız, rehberli zaman çizelgesi.
    /// Her adımda SimulationState'i güncelleyip görsel + klinik dashboard'u yeniler.
    /// </summary>
    public sealed class RecoveryJourneyController : MonoBehaviour
    {
        [SerializeField] private SimulationController controller;
        [SerializeField] private SimulationState state;
        [SerializeField] private ClinicalDashboard dashboard;
        [SerializeField] private LiverVisualController liverVisual;

        [Header("Metinler")]
        [SerializeField] private Text stageText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text progressText;

        [Header("Gezinme")]
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;

        private JourneyStep[] _steps;
        private int _index = -1;

        private void Awake()
        {
            _steps = RecoveryJourneyContent.GetSteps();

            if (liverVisual == null)
            {
                liverVisual = FindObjectOfType<LiverVisualController>();
            }
        }

        private void OnEnable()
        {
            if (_steps == null || _steps.Length == 0)
            {
                _steps = RecoveryJourneyContent.GetSteps();
            }

            // Yolculuk ekranı her açıldığında baştan başlat.
            ApplyStep(0);
        }

        public void OnNext()
        {
            if (_steps == null)
            {
                return;
            }

            ApplyStep(Mathf.Min(_index + 1, _steps.Length - 1));
        }

        public void OnPrevious()
        {
            if (_steps == null)
            {
                return;
            }

            ApplyStep(Mathf.Max(_index - 1, 0));
        }

        private void ApplyStep(int index)
        {
            if (_steps == null || _steps.Length == 0 || index < 0 || index >= _steps.Length)
            {
                return;
            }

            _index = index;
            var step = _steps[index];

            if (state != null)
            {
                state.CurrentScenario = ScenarioType.Recovery;
                state.SimulationWeek = index + 1;
                state.IsAdherent = true;
                state.ImmuneAttack = step.Complication;
                state.GrowthPercentage = step.Growth;
                state.HealthPoints = step.Health;
                state.AST = step.Ast;
                state.ALT = step.Alt;
                state.Bilirubin = step.Bilirubin;
            }

            if (stageText != null)
            {
                stageText.text = step.Stage;
            }

            if (titleText != null)
            {
                titleText.text = step.Title;
            }

            if (bodyText != null)
            {
                bodyText.text = step.Body;
            }

            if (progressText != null)
            {
                progressText.text = $"{index + 1} / {_steps.Length}";
            }

            if (previousButton != null)
            {
                previousButton.interactable = index > 0;
            }

            if (nextButton != null)
            {
                nextButton.interactable = index < _steps.Length - 1;
            }

            controller?.NotifyStateChanged();
            liverVisual?.ApplyStateNow();

            // Dashboard kapanmış olabilir (senaryo None iken); doğrudan yenile -> yeniden etkinleşir.
            if (dashboard != null)
            {
                dashboard.Refresh();
            }
        }
    }
}
