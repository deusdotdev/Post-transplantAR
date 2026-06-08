using LiverAR.Modules.Education.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// Kart bazlı ana ekranın yönlendiricisi. AR'sız kartlar hub içi panel açar;
    /// AR kartları modu yazıp AR sahnesini yükler. AR isteğe bağlıdır (doğrudan açılmaz).
    /// </summary>
    public sealed class HomeHubController : MonoBehaviour
    {
        [Header("Yönlendirme")]
        [SerializeField] private ARLaunchContext arLaunchContext;
        [SerializeField] private string arSceneName = "ARMain";

        [Header("Hub panelleri")]
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject journeyPanel;
        [SerializeField] private GameObject nutritionPanel;

        [Header("3B sahne")]
        [SerializeField] private GameObject liverStage;
        [SerializeField] private GameObject liverModel;
        [SerializeField] private Camera hubCamera;

        private void Start()
        {
            if (hubCamera == null)
            {
                hubCamera = Camera.main;
            }

            ShowHome();
        }

        public void ShowHome()
        {
            SetPanel(homePanel, true);
            SetPanel(journeyPanel, false);
            SetPanel(nutritionPanel, false);
            SetStage(true);
            FrameLiverForHome();
        }

        public void ShowJourney()
        {
            SetPanel(homePanel, false);
            SetPanel(journeyPanel, true);
            SetPanel(nutritionPanel, false);
            SetStage(true);
            FrameLiverForJourney();
        }

        public void ShowNutrition()
        {
            SetPanel(homePanel, false);
            SetPanel(journeyPanel, false);
            SetPanel(nutritionPanel, true);
            SetStage(false);
        }

        public void OpenDrugRegionAR()
        {
            LoadAr(ARLaunchContext.Mode.DrugRegion);
        }

        public void OpenExploreAR()
        {
            LoadAr(ARLaunchContext.Mode.ExploreAnatomy);
        }

        private void LoadAr(ARLaunchContext.Mode mode)
        {
            if (arLaunchContext != null)
            {
                arLaunchContext.CurrentMode = mode;
            }

            if (!string.IsNullOrEmpty(arSceneName))
            {
                SceneManager.LoadScene(arSceneName);
            }
        }

        private void SetStage(bool active)
        {
            if (liverStage != null && liverStage.activeSelf != active)
            {
                liverStage.SetActive(active);
            }
        }

        private void FrameLiverForHome()
        {
            var liver = ResolveLiverModel();
            if (hubCamera != null && liver != null)
            {
                HubCameraFraming.FrameForHome(hubCamera, liver);
            }
        }

        private void FrameLiverForJourney()
        {
            var liver = ResolveLiverModel();
            if (hubCamera != null && liver != null)
            {
                HubCameraFraming.FrameForJourney(hubCamera, liver);
            }
        }

        private GameObject ResolveLiverModel()
        {
            if (liverModel != null)
            {
                return liverModel;
            }

            if (liverStage == null)
            {
                return null;
            }

            return liverStage.transform.childCount > 0
                ? liverStage.transform.GetChild(0).gameObject
                : liverStage;
        }

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null && panel.activeSelf != active)
            {
                panel.SetActive(active);
            }
        }
    }
}
