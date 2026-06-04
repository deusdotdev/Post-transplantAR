using UnityEngine;

namespace LiverAR.Bootstrap
{
    /// <summary>
    /// Sahne açılışında temel performans ve AR oturum ayarlarını uygular.
    /// </summary>
    public sealed class SceneBootstrap : MonoBehaviour
    {
        [Tooltip("Hedef kare hızı. AR için 60 önerilir; düşük segment cihazlarda 30'a düşürülebilir.")]
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;

            // AR oturumu sırasında ekranın uyumaması kritik (uzun süreli görüntüleme).
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
