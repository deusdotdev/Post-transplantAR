using UnityEngine;

namespace LiverAR.Modules.Visuals.Runtime
{
    public enum LiverRegionId
    {
        RightLobe,
        LeftLobe,
        BileDuct,
        VesselInlet
    }

    /// <summary>
    /// Karaciğer üzerinde adlandırılmış bir bölge çapası. Tek mesh üzerinde alt-mesh ayrımı
    /// gerektirmez; vurgulandığında parlayan bir işaret küresi gösterir.
    /// Boyutlar dünya-uzayında (metre) tanımlıdır; model ölçeğinden bağımsızdır.
    /// </summary>
    public sealed class LiverRegionMarker : MonoBehaviour
    {
        [SerializeField] private LiverRegionId region = LiverRegionId.RightLobe;
        [SerializeField] private string displayName = "Sağ lob";

        [Tooltip("Etiketin bölgeye göre yönü (yerel uzayda, birim vektör gibi).")]
        [SerializeField] private Vector3 labelDirectionLocal = new Vector3(0.7f, 0.7f, 0f);

        [Tooltip("Etiketin bölgeden uzaklığı (metre).")]
        [SerializeField] private float labelWorldDistance = 0.08f;

        [Tooltip("İşaret küresinin dünya yarıçapı (metre).")]
        [SerializeField] private float markerWorldRadius = 0.01f;

        [Tooltip("Dokunma çarpışma yarıçapının görünür yarıçapa oranı (kolay dokunmak için).")]
        [SerializeField] private float tapRadiusMultiplier = 3f;

        [SerializeField] private Color highlightColor = new Color(0.28f, 0.62f, 0.58f, 1f);

        private Transform _markerDot;
        private Renderer _markerRenderer;
        private SphereCollider _tapCollider;
        private MaterialPropertyBlock _propBlock;
        private bool _highlighted;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public LiverRegionId Region => region;
        public string DisplayName => displayName;

        public Vector3 TipWorld => transform.position;

        public Vector3 LabelAnchorWorld
        {
            get
            {
                var dir = labelDirectionLocal.sqrMagnitude > 1e-5f
                    ? labelDirectionLocal.normalized
                    : Vector3.up;
                return transform.position + transform.rotation * (dir * labelWorldDistance);
            }
        }

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();
            EnsureMarkerDot();
            EnsureTapCollider();
            SetHighlighted(false);
        }

        /// <summary>
        /// Bölge gizli olsa bile dokunulabilsin diye, görünür küreden daha geniş bir
        /// çarpışma küresi ekler. Physics.Raycast bu collider'ı yakalar.
        /// </summary>
        private void EnsureTapCollider()
        {
            _tapCollider = GetComponent<SphereCollider>();
            if (_tapCollider == null)
            {
                _tapCollider = gameObject.AddComponent<SphereCollider>();
            }

            _tapCollider.isTrigger = false;
            _tapCollider.center = Vector3.zero;

            var lossy = transform.lossyScale;
            var uniform = Mathf.Max(1e-4f, Mathf.Abs(lossy.x));
            var worldTapRadius = Mathf.Max(markerWorldRadius * Mathf.Max(1f, tapRadiusMultiplier), 0.02f);
            _tapCollider.radius = worldTapRadius / uniform;
        }

        private void EnsureMarkerDot()
        {
            if (_markerDot != null)
            {
                return;
            }

            var dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name = "RegionDot";
            var col = dot.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }

            dot.transform.SetParent(transform, false);
            dot.transform.localPosition = Vector3.zero;

            _markerRenderer = dot.GetComponent<Renderer>();
            _markerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _markerRenderer.receiveShadows = false;
            _markerRenderer.sharedMaterial = CreateMarkerMaterial();
            _markerDot = dot.transform;
            ApplyDotScale(1f);
        }

        private static Material CreateMarkerMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Sprites/Default");
            return new Material(shader) { name = "RegionMarkerMat" };
        }

        public void SetHighlighted(bool value)
        {
            _highlighted = value;
            if (_markerDot != null)
            {
                _markerDot.gameObject.SetActive(value);
            }
        }

        public void SetHighlightColor(Color color)
        {
            highlightColor = color;
        }

        private void Update()
        {
            if (!_highlighted || _markerRenderer == null)
            {
                return;
            }

            var pulse = 0.65f + 0.35f * Mathf.Sin(Time.time * 4f);
            var c = highlightColor;
            c.a = pulse;

            _markerRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(ColorId, c);
            _propBlock.SetColor(BaseColorId, c);
            _markerRenderer.SetPropertyBlock(_propBlock);

            ApplyDotScale(0.9f + 0.15f * Mathf.Sin(Time.time * 4f));
        }

#if UNITY_EDITOR
        /// <summary>Editor'de (Play'e basmadan) bölgeyi ve etiket yönünü görünür kılar.</summary>
        private void OnDrawGizmos()
        {
            var c = highlightColor;
            c.a = 1f;
            Gizmos.color = c;
            Gizmos.DrawSphere(transform.position, markerWorldRadius);

            var anchor = LabelAnchorWorld;
            Gizmos.color = new Color(c.r, c.g, c.b, 0.6f);
            Gizmos.DrawLine(transform.position, anchor);
            Gizmos.DrawWireSphere(anchor, markerWorldRadius * 0.6f);
        }
#endif

        /// <summary>Küreyi parent ölçeğinden bağımsız, sabit dünya boyutunda tutar.</summary>
        private void ApplyDotScale(float pulse)
        {
            if (_markerDot == null)
            {
                return;
            }

            var lossy = transform.lossyScale;
            var world = markerWorldRadius * 2f * pulse;
            _markerDot.localScale = new Vector3(
                world / Mathf.Max(1e-4f, Mathf.Abs(lossy.x)),
                world / Mathf.Max(1e-4f, Mathf.Abs(lossy.y)),
                world / Mathf.Max(1e-4f, Mathf.Abs(lossy.z)));
        }
    }
}
