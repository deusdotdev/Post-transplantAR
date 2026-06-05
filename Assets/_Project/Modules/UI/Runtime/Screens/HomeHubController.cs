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

        [Header("3B sahne (yolculukta görünür)")]
        [SerializeField] private GameObject liverStage;

        private void Start()
        {
            ShowHome();
        }

        public void ShowHome()
        {
            SetPanel(homePanel, true);
            SetPanel(journeyPanel, false);
            SetPanel(nutritionPanel, false);
            SetStage(true);
        }

        public void ShowJourney()
        {
            SetPanel(homePanel, false);
            SetPanel(journeyPanel, true);
            SetPanel(nutritionPanel, false);
            SetStage(true);
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

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null && panel.activeSelf != active)
            {
                panel.SetActive(active);
            }
        }
    }
}
