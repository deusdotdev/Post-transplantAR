using LiverAR.Modules.Simulation.Runtime.Data;
using UnityEngine;

namespace LiverAR.Modules.Visuals.Runtime
{
    /// <summary>
    /// Simülasyon verisini karaciğer modelinin görseline yansıtır:
    /// büyüme -> ölçek, bilirubin -> sararma (icterus), bağışıklık saldırısı -> şişme.
    /// Özel shader gerektirmez; renk + ölçek ile çalışır (built-in ve URP uyumlu).
    /// MaterialPropertyBlock kullanır, böylece materyal kopyalanmaz.
    /// </summary>
    public sealed class LiverVisualController : MonoBehaviour
    {
        [SerializeField] private SimulationState state;
        [SerializeField] private Renderer liverRenderer;
        [SerializeField] private Transform liverTransform;

        [Header("Ölçek (büyüme)")]
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 1f;
        [SerializeField] private float scaleLerpSpeed = 1.5f;

        [Header("Renk")]
        [SerializeField] private Color healthyColor = new Color(0.55f, 0.16f, 0.16f);
        [SerializeField] private Color jaundiceColor = new Color(0.85f, 0.78f, 0.2f);
        [SerializeField] private float colorLerpSpeed = 2f;

        [Header("Şişme (ödem)")]
        [SerializeField] private float swellingAmount = 0.12f;
        [SerializeField] private float swellingSpeed = 2.5f;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private MaterialPropertyBlock _propBlock;
        private Vector3 _baseScale = Vector3.one;
        private float _currentSwell;

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
        }

        private void Update()
        {
            if (state == null || liverTransform == null)
            {
                return;
            }

            ApplyScale();
            ApplyColor();
        }

        private void ApplyScale()
        {
            var growthScale = Mathf.Lerp(minScale, maxScale,
                Mathf.InverseLerp(0.3f, 1f, state.GrowthPercentage));

            var targetSwell = state.ImmuneAttack ? swellingAmount : 0f;
            _currentSwell = Mathf.Lerp(_currentSwell, targetSwell, Time.deltaTime * swellingSpeed);

            var pulse = state.ImmuneAttack
                ? Mathf.Sin(Time.time * swellingSpeed) * _currentSwell
                : 0f;

            var factor = growthScale + _currentSwell + pulse;
            var target = _baseScale * factor;
            liverTransform.localScale = Vector3.Lerp(liverTransform.localScale, target,
                Time.deltaTime * scaleLerpSpeed);
        }

        private void ApplyColor()
        {
            if (liverRenderer == null)
            {
                return;
            }

            var jaundice = Mathf.InverseLerp(1.2f, 8f, state.Bilirubin);
            var target = Color.Lerp(healthyColor, jaundiceColor, jaundice);

            liverRenderer.GetPropertyBlock(_propBlock);
            var current = _propBlock.GetColor(BaseColorId);
            if (current.a <= 0f)
            {
                current = healthyColor;
            }

            var next = Color.Lerp(current, target, Time.deltaTime * colorLerpSpeed);
            _propBlock.SetColor(ColorId, next);
            _propBlock.SetColor(BaseColorId, next);
            liverRenderer.SetPropertyBlock(_propBlock);
        }
    }
}
