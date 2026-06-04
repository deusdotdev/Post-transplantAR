using LiverAR.Modules.AR.Runtime.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiverAR.Modules.Interaction.Runtime.Input
{
    /// <summary>
    /// Karaciğer modelini sürekli yatay eksende döndürür; sürükleyerek manuel döndürme de mümkündür.
    /// İki parmak pinch ile ölçekleme yalnızca AR yerleştirme modunda.
    /// </summary>
    public sealed class ModelManipulator : MonoBehaviour
    {
        [SerializeField] private ARPlacementController placementController;
        [Tooltip("Önizleme sahnesinde döndürülecek model (AR'de yerleştirilen nesne kullanılır).")]
        [SerializeField] private Transform previewTarget;

        [Header("Döndürme")]
        [SerializeField] private float rotationSpeed = 0.95f;
        [SerializeField] private float mouseRotationSpeed = 10f;
        [SerializeField] private bool enableMouseDrag = true;

        [Header("Otomatik dönüş")]
        [SerializeField] private bool enableAutoRotate = true;
        [Tooltip("Saniyede derece (yatay eksen, turntable).")]
        [SerializeField] private float autoRotateDegreesPerSecond = 22f;
        [Tooltip("Elle döndürdükten sonra otomatik dönüşün yeniden başlaması için bekleme (sn).")]
        [SerializeField] private float autoRotateResumeDelay = 0.45f;

        [Tooltip("Açıksa yalnızca ekranın üst bölgesinde sürükleme döndürür (alt menüye dokunulmaz).")]
        [SerializeField] private bool limitToUpperViewport = true;
        [SerializeField] [Range(0f, 1f)] private float viewportMinYNormalized = 0.36f;

        [Header("Ölçekleme (AR, iki parmak)")]
        [SerializeField] private float scaleSpeed = 0.005f;
        [SerializeField] private float minScale = 0.2f;
        [SerializeField] private float maxScale = 3f;

        private float _previousPinchDistance;
        private float _lastManualRotateTime = -999f;

        private void Update()
        {
            var target = ResolveTarget();
            if (target == null)
            {
                return;
            }

            var userManipulating = false;

            if (placementController != null && placementController.HasModel &&
                UnityEngine.Input.touchCount == 2)
            {
                userManipulating = true;
                HandlePinchScale(target);
            }
            else if (UnityEngine.Input.touchCount == 1)
            {
                userManipulating = HandleTouchRotation(target);
            }
            else if (enableMouseDrag && UnityEngine.Input.GetMouseButton(0))
            {
                userManipulating = HandleMouseRotation(target);
            }
            else
            {
                _previousPinchDistance = 0f;
            }

            if (userManipulating)
            {
                _lastManualRotateTime = Time.time;
            }
            else if (enableAutoRotate && Time.time - _lastManualRotateTime >= autoRotateResumeDelay)
            {
                target.Rotate(Vector3.up, autoRotateDegreesPerSecond * Time.deltaTime, Space.World);
            }
        }

        private Transform ResolveTarget()
        {
            if (placementController != null && placementController.HasModel)
            {
                return placementController.SpawnedObject.transform;
            }

            return previewTarget;
        }

        private bool HandleTouchRotation(Transform target)
        {
            var touch = UnityEngine.Input.GetTouch(0);
            if (!IsInRotateZone(touch.position) || IsOverUi(touch.fingerId))
            {
                return false;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                ApplyRotationDelta(target, touch.deltaPosition.x, touch.deltaPosition.y, rotationSpeed);
                return true;
            }

            return touch.phase is TouchPhase.Began or TouchPhase.Stationary;
        }

        private bool HandleMouseRotation(Transform target)
        {
            if (!IsInRotateZone(UnityEngine.Input.mousePosition) || IsOverUi())
            {
                return false;
            }

            ApplyRotationDelta(target,
                UnityEngine.Input.GetAxis("Mouse X"),
                UnityEngine.Input.GetAxis("Mouse Y"),
                mouseRotationSpeed);
            return true;
        }

        private static void ApplyRotationDelta(Transform target, float deltaX, float deltaY, float speed)
        {
            target.Rotate(Vector3.up, -deltaX * speed, Space.World);
            target.Rotate(Vector3.right, deltaY * speed * 0.65f, Space.World);
        }

        private void HandlePinchScale(Transform target)
        {
            var touch0 = UnityEngine.Input.GetTouch(0);
            var touch1 = UnityEngine.Input.GetTouch(1);
            var currentDistance = Vector2.Distance(touch0.position, touch1.position);

            if (touch1.phase == TouchPhase.Began || _previousPinchDistance <= 0f)
            {
                _previousPinchDistance = currentDistance;
                return;
            }

            var delta = currentDistance - _previousPinchDistance;
            _previousPinchDistance = currentDistance;

            var uniform = target.localScale.x + delta * scaleSpeed;
            uniform = Mathf.Clamp(uniform, minScale, maxScale);
            target.localScale = new Vector3(uniform, uniform, uniform);
        }

        private bool IsInRotateZone(Vector2 screenPosition)
        {
            if (!limitToUpperViewport)
            {
                return true;
            }

            return screenPosition.y >= Screen.height * viewportMinYNormalized;
        }

        private static bool IsOverUi(int fingerId = -1)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return fingerId >= 0
                ? EventSystem.current.IsPointerOverGameObject(fingerId)
                : EventSystem.current.IsPointerOverGameObject();
        }
    }
}
