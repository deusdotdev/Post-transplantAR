using LiverAR.Modules.AR.Runtime.Controllers;
using LiverAR.Modules.UI.Runtime.Screens;
using UnityEngine;

namespace LiverAR.Bootstrap
{
    /// <summary>
    /// AR oturumu, düzlem algılama, yerleştirme ve UI bileşenlerini birbirine bağlar.
    /// Modüller arası tek bağlantı noktası olarak çalışır (RAMS - Maintainability:
    /// modüller doğrudan birbirini tanımaz, akış burada yönetilir).
    /// </summary>
    public sealed class ARExperienceCoordinator : MonoBehaviour
    {
        [Header("AR")]
        [SerializeField] private ARSessionController sessionController;
        [SerializeField] private PlaneDetectionMonitor planeMonitor;
        [SerializeField] private ARPlacementController placementController;

        [Header("UI")]
        [SerializeField] private ARSetupGuide setupGuide;
        [SerializeField] private SafetyDisclaimerScreen disclaimerScreen;

        private bool _disclaimerAccepted;

        private void OnEnable()
        {
            if (sessionController != null)
            {
                sessionController.StatusChanged += OnSessionStatusChanged;
            }

            if (planeMonitor != null)
            {
                planeMonitor.PlaneAvailabilityChanged += OnPlaneAvailabilityChanged;
            }

            if (placementController != null)
            {
                placementController.ModelPlaced += OnModelPlaced;
            }

            if (disclaimerScreen != null)
            {
                disclaimerScreen.Acknowledged += OnDisclaimerAcknowledged;
            }
        }

        private void OnDisable()
        {
            if (sessionController != null)
            {
                sessionController.StatusChanged -= OnSessionStatusChanged;
            }

            if (planeMonitor != null)
            {
                planeMonitor.PlaneAvailabilityChanged -= OnPlaneAvailabilityChanged;
            }

            if (placementController != null)
            {
                placementController.ModelPlaced -= OnModelPlaced;
            }

            if (disclaimerScreen != null)
            {
                disclaimerScreen.Acknowledged -= OnDisclaimerAcknowledged;
            }
        }

        private void Start()
        {
            if (setupGuide != null)
            {
                setupGuide.ShowState(ARSetupGuide.GuideState.CheckingDevice);
            }
        }

        private void OnDisclaimerAcknowledged()
        {
            _disclaimerAccepted = true;

            // Uyarı, düzlem bulunduktan sonra onaylanmış olabilir; durumu yeniden değerlendir.
            if (planeMonitor != null)
            {
                OnPlaneAvailabilityChanged(planeMonitor.HasUsablePlane);
            }
        }

        private void OnSessionStatusChanged(ARSessionController.SessionStatus status)
        {
            if (setupGuide == null)
            {
                return;
            }

            switch (status)
            {
                case ARSessionController.SessionStatus.CheckingAvailability:
                    setupGuide.ShowState(ARSetupGuide.GuideState.CheckingDevice);
                    break;
                case ARSessionController.SessionStatus.Unsupported:
                    setupGuide.ShowState(ARSetupGuide.GuideState.Unsupported);
                    break;
                case ARSessionController.SessionStatus.NeedsInstall:
                case ARSessionController.SessionStatus.Installing:
                case ARSessionController.SessionStatus.Initializing:
                    setupGuide.ShowState(ARSetupGuide.GuideState.Initializing);
                    break;
                case ARSessionController.SessionStatus.Ready:
                    setupGuide.ShowState(ARSetupGuide.GuideState.SearchingSurface);
                    break;
            }
        }

        private void OnPlaneAvailabilityChanged(bool hasPlane)
        {
            if (placementController != null)
            {
                placementController.SetPlacementEnabled(hasPlane && _disclaimerAccepted);
            }

            if (setupGuide == null || placementController == null || placementController.HasModel)
            {
                return;
            }

            setupGuide.ShowState(hasPlane
                ? ARSetupGuide.GuideState.ReadyToPlace
                : ARSetupGuide.GuideState.SearchingSurface);
        }

        private void OnModelPlaced(GameObject model)
        {
            if (setupGuide != null)
            {
                setupGuide.ShowState(ARSetupGuide.GuideState.ModelPlaced);
            }
        }
    }
}
