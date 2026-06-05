using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>Buton onClick'lerinden adı verilen sahneyi yükler (örn. AR -> ana ekran).</summary>
    public sealed class SceneNavigator : MonoBehaviour
    {
        [SerializeField] private string homeSceneName = "EducationHub";

        public void GoHome()
        {
            LoadScene(homeSceneName);
        }

        public void LoadScene(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}
