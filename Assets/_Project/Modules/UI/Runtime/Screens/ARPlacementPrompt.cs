using LiverAR.Modules.AR.Runtime.Controllers;
using UnityEngine;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// Karaciğer yerleştirilene kadar kamera üstünde kısa yönlendirme + yerleştir butonu gösterir.
    /// </summary>
    public sealed class ARPlacementPrompt : MonoBehaviour
    {
        [SerializeField] private ARPlacementController placementController;
        [SerializeField] private EducationFlowController educationFlow;
        [SerializeField] private GameObject promptRoot;

        private void OnEnable()
        {
            if (placementController != null)
            {
                placementController.ModelPlaced += OnModelPlaced;
            }

            if (educationFlow != null)
            {
                educationFlow.FlowComplete += RefreshVisibility;
            }

            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (placementController != null)
            {
                placementController.ModelPlaced -= OnModelPlaced;
            }

            if (educationFlow != null)
            {
                educationFlow.FlowComplete -= RefreshVisibility;
            }
        }

        private void OnModelPlaced(GameObject _)
        {
            RefreshVisibility();
        }

        public void OnPlaceButtonClicked()
        {
            placementController?.TryPlaceAtViewportCenter(allowFallbackInFrontOfCamera: true);
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            if (promptRoot == null)
            {
                return;
            }

            var flowReady = educationFlow == null || educationFlow.IsFlowComplete;
            var needsPlace = placementController == null || !placementController.HasModel;
            promptRoot.SetActive(flowReady && needsPlace);
        }
    }
}
