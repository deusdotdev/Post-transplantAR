using UnityEngine;

namespace LiverAR.Modules.Visuals.Runtime
{
    /// <summary>
    /// Bir bölge işaretçisine doğru leader-line ok çizer ve kameraya dönük yüzen etiket gösterir.
    /// Çalışma zamanında LineRenderer + TextMesh çocukları oluşturur (özel shader gerektirmez).
    /// </summary>
    public sealed class RegionAnnotationArrow : MonoBehaviour
    {
        private LineRenderer _line;
        private TextMesh _label;
        private Transform _labelTransform;
        private Camera _camera;

        private LiverRegionMarker _target;
        private float _lineWidth = 0.004f;
        private float _headLength = 0.02f;
        private float _headWidth = 0.012f;

        public void Initialize(float referenceSize)
        {
            // Çizgi/ok ölçüsünü modelin büyüklüğüne göre ayarla.
            _lineWidth = Mathf.Max(0.0015f, referenceSize * 0.02f);
            _headLength = Mathf.Max(0.008f, referenceSize * 0.10f);
            _headWidth = Mathf.Max(0.005f, referenceSize * 0.06f);

            EnsureLine();
            EnsureLabel(referenceSize);
            Hide();
        }

        private void EnsureLine()
        {
            if (_line != null)
            {
                return;
            }

            _line = gameObject.AddComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.positionCount = 5;
            _line.numCornerVertices = 2;
            _line.numCapVertices = 2;
            _line.startWidth = _lineWidth;
            _line.endWidth = _lineWidth;
            _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _line.receiveShadows = false;

            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Sprites/Default");
            _line.material = new Material(shader) { name = "ArrowLineMat" };
        }

        private void EnsureLabel(float referenceSize)
        {
            if (_label != null)
            {
                return;
            }

            var labelGo = new GameObject("ArrowLabel");
            _labelTransform = labelGo.transform;
            _labelTransform.SetParent(transform, false);

            _label = labelGo.AddComponent<TextMesh>();
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.fontSize = 80;
            _label.characterSize = Mathf.Max(0.01f, referenceSize * 0.06f);
            _label.color = Color.white;
            _label.richText = false;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                       ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null)
            {
                _label.font = font;
            }

            var mr = labelGo.GetComponent<MeshRenderer>();
            if (font != null)
            {
                mr.sharedMaterial = font.material;
            }

            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        public void Show(LiverRegionMarker target, string text, Color color, Camera viewCamera)
        {
            _target = target;
            _camera = viewCamera != null ? viewCamera : Camera.main;

            if (_line != null)
            {
                _line.startColor = color;
                _line.endColor = color;
                _line.enabled = true;
            }

            if (_label != null)
            {
                _label.text = text;
                _label.color = color;
                _label.gameObject.SetActive(true);
            }

            gameObject.SetActive(true);
            UpdateGeometry();
        }

        public void Hide()
        {
            _target = null;
            if (_line != null)
            {
                _line.enabled = false;
            }

            if (_label != null)
            {
                _label.gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            UpdateGeometry();
        }

        private void UpdateGeometry()
        {
            if (_target == null || _line == null)
            {
                return;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            var tip = _target.TipWorld;
            var labelAnchor = _target.LabelAnchorWorld;

            // Etiketi konumla + kameraya döndür (billboard).
            if (_labelTransform != null)
            {
                _labelTransform.position = labelAnchor;
                if (_camera != null)
                {
                    _labelTransform.rotation = Quaternion.LookRotation(
                        _labelTransform.position - _camera.transform.position, _camera.transform.up);
                }
            }

            // Ok ucunu (V) kameraya göre düzlemde hesapla.
            var dir = (tip - labelAnchor);
            var dist = dir.magnitude;
            if (dist < 1e-4f)
            {
                _line.SetPosition(0, labelAnchor);
                _line.SetPosition(1, tip);
                _line.SetPosition(2, tip);
                _line.SetPosition(3, tip);
                _line.SetPosition(4, tip);
                return;
            }

            dir /= dist;
            var camForward = _camera != null ? _camera.transform.forward : Vector3.forward;
            var right = Vector3.Cross(dir, camForward).normalized;
            if (right.sqrMagnitude < 1e-4f)
            {
                right = Vector3.Cross(dir, Vector3.up).normalized;
            }

            var headBase = tip - dir * _headLength;
            var headLeft = headBase + right * _headWidth;
            var headRight = headBase - right * _headWidth;

            _line.SetPosition(0, labelAnchor);
            _line.SetPosition(1, tip);
            _line.SetPosition(2, headLeft);
            _line.SetPosition(3, headRight);
            _line.SetPosition(4, tip);
        }
    }
}
