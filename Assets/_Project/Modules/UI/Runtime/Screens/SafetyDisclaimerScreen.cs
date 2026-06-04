using System;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// İlk kullanımda gösterilen güvenlik/sorumluluk reddi ekranı.
    /// RAMS - Safety: kullanıcının uygulamayı tanı aracı sanmasını önlemek için
    /// açık uyarı ve onay gerektirir.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public sealed class SafetyDisclaimerScreen : MonoBehaviour
    {
        private const string AcknowledgedKey = "safety_disclaimer_acknowledged";

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private Button acknowledgeButton;

        [Tooltip("Her açılışta tekrar göster (true) veya yalnızca ilk kullanımda göster (false).")]
        [SerializeField] private bool showEveryLaunch = false;

        [TextArea]
        [SerializeField] private string disclaimerMessage =
            "Önemli Bilgilendirme\n\n" +
            "Bu uygulama karaciğer nakli sonrası süreci görsel olarak anlatan bir EĞİTİM aracıdır. " +
            "Tıbbi tanı koymaz, tedavi önermez ve doktor görüşünün yerine geçmez.\n\n" +
            "AR kullanımı sırasında çevrenize dikkat edin; tercihen oturarak ve sabit bir konumda kullanın.";

        /// <summary>Kullanıcı uyarıyı onayladığında tetiklenir.</summary>
        public event Action Acknowledged;

        private void Awake()
        {
            if (messageText != null)
            {
                messageText.text = disclaimerMessage;
            }

            if (acknowledgeButton != null)
            {
                acknowledgeButton.onClick.AddListener(OnAcknowledge);
            }
        }

        private void OnDestroy()
        {
            if (acknowledgeButton != null)
            {
                acknowledgeButton.onClick.RemoveListener(OnAcknowledge);
            }
        }

        private void Start()
        {
            var alreadyAccepted = !showEveryLaunch &&
                                  PlayerPrefs.GetInt(AcknowledgedKey, 0) == 1;

            if (alreadyAccepted)
            {
                SetVisible(false);
                Acknowledged?.Invoke();
            }
            else
            {
                SetVisible(true);
            }
        }

        private void OnAcknowledge()
        {
            PlayerPrefs.SetInt(AcknowledgedKey, 1);
            PlayerPrefs.Save();
            SetVisible(false);
            Acknowledged?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(visible);
            }
        }
    }
}
