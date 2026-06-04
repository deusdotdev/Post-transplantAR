using System;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// README vaadi: karaciğer anatomisi / işlev girişi. Güvenlik uyarısından sonra gösterilir.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public sealed class AnatomyIntroScreen : MonoBehaviour
    {
        private const string IntroSeenKey = "anatomy_intro_seen";

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private Button continueButton;

        [SerializeField] private bool showEveryLaunch;

        [TextArea]
        [SerializeField] private string introMessage =
            "Karaciğerimizi tanıyalım\n\n" +
            "Karaciğer; kanı temizleme, protein üretimi ve safra oluşturma gibi hayati işleri yapar.\n\n" +
            "Bu uygulama, nakil sonrası iyileşme ve ilaç uyumunu 3B model üzerinde görmeniz için " +
            "hazırlanmış bir eğitim aracıdır. Tanı veya tedavi önermez.\n\n" +
            "Şikâyetiniz varsa her zaman transplant ekibinize danışın.";

        public event Action Continued;

        private void Awake()
        {
            if (messageText != null)
            {
                messageText.text = introMessage;
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinue);
            }
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinue);
            }
        }

        private void Start()
        {
            var skip = !showEveryLaunch && PlayerPrefs.GetInt(IntroSeenKey, 0) == 1;
            if (skip)
            {
                SetVisible(false);
                Continued?.Invoke();
            }
            else
            {
                SetVisible(false);
            }
        }

        public void Show()
        {
            SetVisible(true);
        }

        private void OnContinue()
        {
            PlayerPrefs.SetInt(IntroSeenKey, 1);
            PlayerPrefs.Save();
            SetVisible(false);
            Continued?.Invoke();
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
