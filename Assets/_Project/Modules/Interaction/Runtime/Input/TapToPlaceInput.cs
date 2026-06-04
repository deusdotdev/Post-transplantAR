using LiverAR.Modules.AR.Runtime.Controllers;
using UnityEngine;

namespace LiverAR.Modules.Interaction.Runtime.Input
{
    public sealed class TapToPlaceInput : MonoBehaviour
    {
        [SerializeField] private ARPlacementController placementController;

        private void Update()
        {
            // Sadece tek parmak dokunuşunda yerleştir; iki parmak jesti ölçekleme içindir.
            if (placementController == null || Input.touchCount != 1)
            {
                return;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began)
            {
                return;
            }

            placementController.TryPlaceFromScreenTap(touch.position);
        }
    }
}
