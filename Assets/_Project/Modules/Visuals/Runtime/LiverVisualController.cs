using LiverAR.Modules.Simulation.Runtime.Data;
using UnityEngine;

namespace LiverAR.Modules.Visuals.Runtime
{
    /// <summary>
    /// Simülasyon verisini karaciğer modelinin görseline yansıtır:
    /// büyüme -> ölçek, bilirubin -> sararma (icterus), bağışıklık saldırısı -> şişme.
    /// </summary>
    public sealed class LiverVisualController : MonoBehaviour
    {
        [SerializeField] private SimulationState state;
        [SerializeField] private Renderer liverRenderer;
        [SerializeField] private Transform liverTransform;

        [Header("Ölçek (iyileşme senaryosu)")]
        [SerializeField] private float minScale = 0.88f;
        [SerializeField] private float maxScale = 1.08f;

        [Header("Ölçek (ilaç senaryosu)")]
        [SerializeField] private float medicationStableScale = 0.94f;
        [SerializeField] private float medicationRiskScale = 1.14f;

        [SerializeField] private float scaleLerpSpeed = 5f;

        [Header("Renk")]
        [SerializeField] private Color healthyColor = new Color(0.55f, 0.16f, 0.16f);
        [SerializeField] private Color jaundiceColor = new Color(0.85f, 0.78f, 0.2f);
        [SerializeField] private float colorLerpSpeed = 2f;

        [Header("Şişme (red / ilaç atlama)")]
        [SerializeField] private float swellingAmount = 0.2f;
        [SerializeField] private float swellingSpeed = 3.5f;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private MaterialPropertyBlock _propBlock;
        private Vector3 _baseScale = Vector3.one;
        private float _currentSwell;
        private bool _useSimulationPalette;
        private bool _lastImmuneAttack;
        private bool _lastAdherent;
        private ScenarioType _lastScenario;

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();

            if (liverRenderer == null)
            {
                liverRenderer = GetComponentInChildren<Renderer>();
            }

            if (liverTransform == null)
            {
                liverTransform = transform;
            }

            _baseScale = liverTransform.localScale;
            _useSimulationPalette = GetComponentInChildren<LiverMeshGenerator>(true) != null;
            if (liverRenderer != null)
            {
                LiverRenderBootstrap.EnsureVisible(gameObject);
            }
        }

        private void Update()
        {
            if (state == null || liverTransform == null)
            {
                return;
            }

            DetectStateTransition();
            ApplyScale();
            ApplyColor();
        }

        private void DetectStateTransition()
        {
            var immune = state.ImmuneAttack;
            var adherent = state.IsAdherent;
            var scenario = state.CurrentScenario;

            if (immune == _lastImmuneAttack && adherent == _lastAdherent && scenario == _lastScenario)
            {
                return;
            }

            _lastImmuneAttack = immune;
            _lastAdherent = adherent;
            _lastScenario = scenario;

            if (!immune)
            {
                _currentSwell = 0f;
            }
            else
            {
                _currentSwell = swellingAmount * 0.35f;
            }
        }

        private void ApplyScale()
        {
            var growthScale = ResolveGrowthScale();

            var targetSwell = state.ImmuneAttack ? swellingAmount : 0f;
            _currentSwell = Mathf.Lerp(_currentSwell, targetSwell, Time.deltaTime * swellingSpeed);

            var pulse = state.ImmuneAttack
                ? Mathf.Sin(Time.time * swellingSpeed * 1.2f) * _currentSwell * 0.45f
                : 0f;

            var factor = growthScale + _currentSwell + pulse;
            var target = _baseScale * factor;
            liverTransform.localScale = Vector3.Lerp(liverTransform.localScale, target,
                Time.deltaTime * scaleLerpSpeed);
        }

        private float ResolveGrowthScale()
        {
            if (state.CurrentScenario == ScenarioType.Medication)
            {
                var healthFactor = Mathf.Clamp01(state.HealthPoints / 100f);
                if (state.IsAdherent && !state.ImmuneAttack)
                {
                    return Mathf.Lerp(medicationRiskScale, medicationStableScale, healthFactor);
                }

                var riskT = 1f - healthFactor;
                var growthT = Mathf.InverseLerp(0.82f, 1.15f, state.GrowthPercentage);
                return Mathf.Lerp(medicationStableScale, medicationRiskScale,
                    Mathf.Max(riskT, growthT * 0.85f));
            }

            return Mathf.Lerp(minScale, maxScale,
                Mathf.InverseLerp(0.3f, 1f, state.GrowthPercentage));
        }

        private void ApplyColor()
        {
            if (liverRenderer == null)
            {
                return;
            }

            var jaundice = Mathf.InverseLerp(1.2f, 8f, state.Bilirubin);

            liverRenderer.GetPropertyBlock(_propBlock);
            Color target;
            Color current;

            if (_useSimulationPalette)
            {
                target = Color.Lerp(healthyColor, jaundiceColor, jaundice);
                current = _propBlock.GetColor(BaseColorId);
                if (current.a <= 0f)
                {
                    current = healthyColor;
                }
            }
            else
            {
                target = Color.Lerp(Color.white, jaundiceColor, jaundice * 0.85f);
                current = _propBlock.GetColor(BaseColorId);
                if (current.a <= 0f)
                {
                    current = Color.white;
                }
            }

            var next = Color.Lerp(current, target, Time.deltaTime * colorLerpSpeed);
            _propBlock.SetColor(ColorId, next);
            _propBlock.SetColor(BaseColorId, next);
            liverRenderer.SetPropertyBlock(_propBlock);
        }
    }
}
