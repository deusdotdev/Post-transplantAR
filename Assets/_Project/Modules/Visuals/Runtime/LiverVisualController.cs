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
        [SerializeField] private float recoveryMinScale = 0.80f;
        [SerializeField] private float recoveryMaxScale = 1.06f;

        [Header("Ölçek (ilaç senaryosu)")]
        [SerializeField] private float medicationStableScale = 0.94f;
        [SerializeField] private float medicationRiskScale = 1.14f;

        [SerializeField] private float scaleLerpSpeed = 5f;

        [Header("Renk")]
        [SerializeField] private Color healthyColor = new Color(0.55f, 0.16f, 0.16f);
        [SerializeField] private Color jaundiceColor = new Color(0.85f, 0.78f, 0.2f);
        [SerializeField] private Color inflamedColor = new Color(0.42f, 0.14f, 0.12f);
        [SerializeField] private Color importedHealthyColor = new Color(0.62f, 0.22f, 0.18f);
        [SerializeField] private float colorLerpSpeed = 2f;
        [SerializeField] private float stepChangeColorSpeed = 9f;

        [Header("Şişme (red / ilaç atlama)")]
        [SerializeField] private float swellingAmount = 0.2f;
        [SerializeField] private float swellingSpeed = 3.5f;
        [SerializeField] private float recoveryEdemaAmount = 0.1f;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private MaterialPropertyBlock _propBlock;
        private Renderer[] _liverRenderers;
        private Vector3 _baseScale = Vector3.one;
        private float _currentSwell;
        private bool _useSimulationPalette;
        private bool _lastImmuneAttack;
        private bool _lastAdherent;
        private ScenarioType _lastScenario;
        private float _lastBilirubin = -1f;
        private float _lastGrowth = -1f;
        private float _lastHealth = -1f;
        private float _activeColorSpeed;
        private Color _currentTint = Color.white;
        private bool _drugAdherenceVisualActive;
        private bool _drugAdherent = true;

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();
            CacheRenderers();

            if (liverTransform == null)
            {
                liverTransform = transform;
            }

            _baseScale = liverTransform.localScale;
            _useSimulationPalette = GetComponentInChildren<LiverMeshGenerator>(true) != null;
            _activeColorSpeed = colorLerpSpeed;
            _currentTint = ResolveHealthyBaseColor();

            if (_liverRenderers.Length > 0)
            {
                LiverRenderBootstrap.EnsureVisible(gameObject);
            }
        }

        /// <summary>İlaç AR modunda düzenli / atlandı görünümü (ölçeği bozmaz, yalnızca renk).</summary>
        public void ApplyDrugAdherenceVisual(bool adherent)
        {
            _drugAdherenceVisualActive = true;
            _drugAdherent = adherent;
            _activeColorSpeed = stepChangeColorSpeed;
        }

        public void ClearDrugAdherenceVisual()
        {
            _drugAdherenceVisualActive = false;
            _activeColorSpeed = colorLerpSpeed;
        }

        /// <summary>AR yerleştirme veya pinch sonrası referans ölçeği günceller.</summary>
        public void CaptureBaseScale()
        {
            if (liverTransform != null)
            {
                _baseScale = liverTransform.localScale;
            }
        }

        /// <summary>Yolculuk adımı değişince görseli anında günceller.</summary>
        public void ApplyStateNow()
        {
            if (state == null || liverTransform == null)
            {
                return;
            }

            DetectMetricTransition(force: true);
            var growthScale = ResolveGrowthScale();
            var targetSwell = ResolveTargetSwell();
            _currentSwell = targetSwell;

            if (ShouldDriveScale())
            {
                liverTransform.localScale = _baseScale * (growthScale + targetSwell);
            }

            _currentTint = ResolveTargetColor();
            ApplyTintToRenderers(_currentTint);
        }

        private void CacheRenderers()
        {
            if (liverRenderer == null)
            {
                liverRenderer = GetComponentInChildren<Renderer>();
            }

            _liverRenderers = GetComponentsInChildren<Renderer>(true);
            if (_liverRenderers.Length == 0 && liverRenderer != null)
            {
                _liverRenderers = new[] { liverRenderer };
            }
        }

        private void Update()
        {
            if (state == null || liverTransform == null)
            {
                return;
            }

            DetectStateTransition();
            DetectMetricTransition(force: false);
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
                _currentSwell = Mathf.Min(_currentSwell, swellingAmount * 0.35f);
            }
            else
            {
                _currentSwell = swellingAmount * 0.35f;
            }
        }

        private void DetectMetricTransition(bool force)
        {
            var metricsChanged = !Mathf.Approximately(state.Bilirubin, _lastBilirubin)
                                 || !Mathf.Approximately(state.GrowthPercentage, _lastGrowth)
                                 || !Mathf.Approximately(state.HealthPoints, _lastHealth);

            if (!force && !metricsChanged)
            {
                return;
            }

            _lastBilirubin = state.Bilirubin;
            _lastGrowth = state.GrowthPercentage;
            _lastHealth = state.HealthPoints;
            _activeColorSpeed = force ? stepChangeColorSpeed : colorLerpSpeed;
        }

        private static bool ShouldDriveScale(ScenarioType scenario)
        {
            return scenario == ScenarioType.Recovery || scenario == ScenarioType.Medication;
        }

        private bool ShouldDriveScale()
        {
            return state != null && ShouldDriveScale(state.CurrentScenario);
        }

        private void ApplyScale()
        {
            if (!ShouldDriveScale())
            {
                return;
            }

            var growthScale = ResolveGrowthScale();
            var targetSwell = ResolveTargetSwell();
            _currentSwell = Mathf.Lerp(_currentSwell, targetSwell, Time.deltaTime * swellingSpeed);

            var pulse = state.ImmuneAttack
                ? Mathf.Sin(Time.time * swellingSpeed * 1.2f) * _currentSwell * 0.45f
                : 0f;

            var factor = growthScale + _currentSwell + pulse;
            var target = _baseScale * factor;
            liverTransform.localScale = Vector3.Lerp(liverTransform.localScale, target,
                Time.deltaTime * scaleLerpSpeed);
        }

        private float ResolveTargetSwell()
        {
            if (state.ImmuneAttack)
            {
                return swellingAmount;
            }

            if (state.CurrentScenario == ScenarioType.Recovery)
            {
                var stress = 1f - Mathf.InverseLerp(65f, 95f, state.HealthPoints);
                return recoveryEdemaAmount * stress;
            }

            return 0f;
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

            if (state.CurrentScenario == ScenarioType.Recovery)
            {
                var t = Mathf.InverseLerp(0.5f, 1f, state.GrowthPercentage);
                return Mathf.Lerp(recoveryMinScale, recoveryMaxScale, t);
            }

            return Mathf.Lerp(minScale, maxScale,
                Mathf.InverseLerp(0.3f, 1f, state.GrowthPercentage));
        }

        private void ApplyColor()
        {
            if (_liverRenderers == null || _liverRenderers.Length == 0)
            {
                return;
            }

            var target = ResolveTargetColor();
            _currentTint = Color.Lerp(_currentTint, target, Time.deltaTime * _activeColorSpeed);
            ApplyTintToRenderers(_currentTint);

            if (_activeColorSpeed > colorLerpSpeed
                && ColorDistance(_currentTint, target) < 0.01f)
            {
                _activeColorSpeed = colorLerpSpeed;
            }
        }

        private void ApplyTintToRenderers(Color color)
        {
            for (var i = 0; i < _liverRenderers.Length; i++)
            {
                var renderer = _liverRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(ColorId, color);
                _propBlock.SetColor(BaseColorId, color);
                renderer.SetPropertyBlock(_propBlock);
            }
        }

        private Color ResolveTargetColor()
        {
            var healthyBase = ResolveHealthyBaseColor();

            if (_drugAdherenceVisualActive)
            {
                if (_drugAdherent)
                {
                    return healthyBase;
                }

                var stressed = Color.Lerp(inflamedColor, jaundiceColor, 0.42f);
                return Color.Lerp(healthyBase, stressed, 0.72f);
            }

            var jaundice = Mathf.InverseLerp(0.8f, 5f, state.Bilirubin);
            var healthT = Mathf.InverseLerp(62f, 98f, state.HealthPoints);
            var tinted = Color.Lerp(healthyBase, jaundiceColor, jaundice);

            if (state.CurrentScenario == ScenarioType.Recovery)
            {
                tinted = Color.Lerp(inflamedColor, tinted, Mathf.Lerp(0.35f, 1f, healthT));
            }

            return tinted;
        }

        private Color ResolveHealthyBaseColor()
        {
            return _useSimulationPalette ? healthyColor : importedHealthyColor;
        }

        private static float ColorDistance(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
        }
    }
}
