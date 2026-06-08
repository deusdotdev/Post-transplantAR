using LiverAR.Modules.AR.Runtime.Controllers;
using LiverAR.Modules.Visuals.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiverAR.Modules.Interaction.Runtime.Input
{
    public sealed class TapToPlaceInput : MonoBehaviour
    {
        [SerializeField] private ARPlacementController placementController;

        private void Update()
        {
            // Sadece tek parmak dokunuşunda yerleştir; iki parmak jesti ölçekleme içindir.
            // UnityEngine.Input tam yol: namespace'imiz "...Runtime.Input" ile bittiği için
            // çıplak "Input" yanlış çözümlenir.
            if (placementController == null || UnityEngine.Input.touchCount != 1)
            {
                return;
            }

            var touch = UnityEngine.Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began)
            {
                return;
            }

            if (IsOverUi(touch.fingerId))
            {
                return;
            }

            // Bir bölgeye dokunulduysa modeli taşıma; dokunmayı keşif/bölge seçimine bırak.
            if (HitsRegionMarker(touch.position))
            {
                return;
            }

            if (!placementController.TryPlaceFromScreenTap(touch.position))
            {
                // Üst kamera alanında düzlem yoksa yine de önüne yerleştirmeyi dene.
                placementController.TryPlaceFromScreenTap(touch.position, allowFallbackInFrontOfCamera: true);
            }
        }

        private static bool IsOverUi(int fingerId)
        {
            return EventSystem.current != null &&
                   EventSystem.current.IsPointerOverGameObject(fingerId);
        }

        private static bool HitsRegionMarker(Vector2 screenPosition)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return false;
            }

            var ray = cam.ScreenPointToRay(screenPosition);
            return Physics.Raycast(ray, out var hit, 100f) &&
                   hit.collider.GetComponentInParent<LiverRegionMarker>() != null;
        }
    }
}
