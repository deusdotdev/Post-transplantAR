using LiverAR.Modules.AR.Runtime.Controllers;
using UnityEngine;

namespace LiverAR.Modules.Interaction.Runtime.Input
{
    /// <summary>
    /// Yerleştirilen modele dokunma jestleriyle döndürme ve ölçekleme uygular.
    /// Tek parmak yatay sürükleme = döndürme, iki parmak pinch = ölçekleme.
    /// README'deki "döndürme, ölçekleme" etkileşim hedefini karşılar.
    /// </summary>
    public sealed class ModelManipulator : MonoBehaviour
    {
        [SerializeField] private ARPlacementController placementController;

        [Header("Döndürme")]
        [SerializeField] private float rotationSpeed = 0.2f;

        [Header("Ölçekleme")]
        [SerializeField] private float scaleSpeed = 0.005f;
        [SerializeField] private float minScale = 0.2f;
        [SerializeField] private float maxScale = 3f;

        private float _previousPinchDistance;

        private void Update()
        {
            if (placementController == null || !placementController.HasModel)
            {
                return;
            }

            var target = placementController.SpawnedObject.transform;

            switch (UnityEngine.Input.touchCount)
            {
                case 1:
                    HandleRotation(target);
                    break;
                case 2:
                    HandleScale(target);
                    break;
                default:
                    _previousPinchDistance = 0f;
                    break;
            }
        }

        private void HandleRotation(Transform target)
        {
            var touch = UnityEngine.Input.GetTouch(0);
            if (touch.phase != TouchPhase.Moved)
            {
                return;
            }

            // Tek parmakla yapılan yerleştirme dokunuşuyla çakışmaması için sadece sürüklemede döndür.
            target.Rotate(Vector3.up, -touch.deltaPosition.x * rotationSpeed, Space.World);
        }

        private void HandleScale(Transform target)
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
    }
}
