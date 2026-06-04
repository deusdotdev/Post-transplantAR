using System;
using System.Collections;
using LiverAR.Modules.Simulation.Runtime.Data;
using UnityEngine;

namespace LiverAR.Modules.Simulation.Runtime
{
    /// <summary>
    /// Senaryo mantığını yürütür. Her frame yerine yalnızca anlamlı bir değişiklik
    /// olduğunda <see cref="StateChanged"/> tetikler (event-driven tasarım).
    /// </summary>
    public sealed class SimulationController : MonoBehaviour
    {
        [SerializeField] private SimulationState state;

        [Tooltip("İlaç uyumsuzluğunda kötüleşmenin saniyelik tik aralığı.")]
        [SerializeField] private float declineTickSeconds = 0.85f;

        [Tooltip("İlacı alınca iyileşme tik aralığı.")]
        [SerializeField] private float recoveryTickSeconds = 0.85f;

        /// <summary>Durumda anlamlı bir değişiklik olduğunda tetiklenir.</summary>
        public event Action StateChanged;

        public SimulationState State => state;

        private Coroutine _declineRoutine;
        private Coroutine _recoveryRoutine;

        public void StartScenario(ScenarioType scenario)
        {
            if (state == null)
            {
                return;
            }

            state.ResetToDefault();
            state.CurrentScenario = scenario;

            if (scenario == ScenarioType.Medication)
            {
                ApplyMedicationTargets(adherent: true);
            }
            else
            {
                RecalculateRecoveryTargets();
            }

            RestartSimulationRoutines();
            RaiseChanged();
        }

        /// <summary>Ana menüye dönüldüğünde senaryo durumunu kapatır.</summary>
        public void ExitScenario()
        {
            if (state == null)
            {
                return;
            }

            state.CurrentScenario = ScenarioType.None;
            RestartSimulationRoutines();
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
            if (state == null || state.CurrentScenario != ScenarioType.Medication)
            {
                return;
            }

            if (state.IsAdherent == adherent)
            {
                return;
            }

            ApplyMedicationTargets(adherent);
            RestartSimulationRoutines();
            RaiseChanged();
        }

        public string GetHeader() => ScenarioNarrative.GetHeader(state);

        public string GetDescription() => ScenarioNarrative.GetDescription(state);

        private void ApplyMedicationTargets(bool adherent)
        {
            state.IsAdherent = adherent;
            state.ImmuneAttack = !adherent;

            if (adherent)
            {
                state.GrowthPercentage = Mathf.Max(0.82f, state.GrowthPercentage);
            }
            else
            {
                state.GrowthPercentage = Mathf.Min(1.12f, state.GrowthPercentage + 0.08f);
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
                case 5:
                case 6:
                case 7: state.GrowthPercentage = 0.92f; break;
                default: state.GrowthPercentage = 1f; break;
            }

            state.Bilirubin = Mathf.Lerp(2.5f, 0.8f, Mathf.InverseLerp(1, 8, state.SimulationWeek));
        }

        private void RestartSimulationRoutines()
        {
            if (_declineRoutine != null)
            {
                StopCoroutine(_declineRoutine);
                _declineRoutine = null;
            }

            if (_recoveryRoutine != null)
            {
                StopCoroutine(_recoveryRoutine);
                _recoveryRoutine = null;
            }

            if (!isActiveAndEnabled || state == null)
            {
                return;
            }

            if (state.CurrentScenario == ScenarioType.Medication && !state.IsAdherent)
            {
                _declineRoutine = StartCoroutine(DeclineRoutine());
            }
            else if (state.CurrentScenario == ScenarioType.Medication && state.IsAdherent &&
                     NeedsRecovery())
            {
                _recoveryRoutine = StartCoroutine(RecoveryRoutine());
            }
        }

        private bool NeedsRecovery()
        {
            return state.HealthPoints < 99.5f
                   || state.Bilirubin > 1.2f
                   || state.AST > 45f
                   || state.ALT > 50f
                   || state.GrowthPercentage > 0.9f;
        }

        private IEnumerator DeclineRoutine()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.1f, declineTickSeconds));

            while (state != null && state.HealthPoints > 0f && !state.IsAdherent)
            {
                yield return wait;

                state.HealthPoints = Mathf.Max(0f, state.HealthPoints - 6f);
                state.Bilirubin = Mathf.Min(8f, state.Bilirubin + 0.45f);
                state.AST = Mathf.Min(400f, state.AST + 22f);
                state.ALT = Mathf.Min(400f, state.ALT + 20f);
                state.GrowthPercentage = Mathf.Min(1.15f, state.GrowthPercentage + 0.04f);
                RaiseChanged();
            }
        }

        private IEnumerator RecoveryRoutine()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.1f, recoveryTickSeconds));

            while (state != null && state.IsAdherent && NeedsRecovery())
            {
                yield return wait;

                state.HealthPoints = Mathf.Min(100f, state.HealthPoints + 6f);
                state.Bilirubin = Mathf.Max(0.8f, state.Bilirubin - 0.35f);
                state.AST = Mathf.Max(25f, state.AST - 18f);
                state.ALT = Mathf.Max(30f, state.ALT - 16f);
                state.GrowthPercentage = Mathf.Max(0.82f, state.GrowthPercentage - 0.04f);
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

            if (_recoveryRoutine != null)
            {
                StopCoroutine(_recoveryRoutine);
                _recoveryRoutine = null;
            }
        }

        private void RaiseChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
