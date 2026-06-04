using LiverAR.Modules.UI.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>README bilgi noktaları: lob ve işlev kartları (üst overlay katmanında).</summary>
    public sealed class LiverAnatomyInfoPanel : MonoBehaviour
    {
        [SerializeField] private GameObject overlayRoot;
        [SerializeField] private GameObject cardRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            Hide();
        }

        public void ShowRightLobe()
        {
            Show("Sağ lob",
                "Daha büyük bölümdür; detoksifikasyon ve metabolizmanın önemli kısmı burada yürür. " +
                "Nakil sonrası bu bölgenin kanlanması iyileşme için kritiktir.");
        }

        public void ShowLeftLobe()
        {
            Show("Sol lob",
                "Sağ loba göre daha küçüktür. Nakil parçası genelde sağ lobdan alınır; " +
                "kalan dokunun rejenerasyon kapasitesi yüksektir.");
        }

        public void ShowBileDuct()
        {
            Show("Safra yolları",
                "Safra sindirime yardımcı olur. Nakil sonrası safra akımında sorun olursa " +
                "sararma (sarılık) görülebilir — ekibinize bildirin.");
        }

        private void Show(string title, string body)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (bodyText != null)
            {
                bodyText.text = body;
            }

            if (overlayRoot != null)
            {
                overlayRoot.SetActive(true);
                UiLayer.BringToFront(overlayRoot);
            }

            if (cardRoot != null)
            {
                cardRoot.SetActive(true);
            }
        }

        public void Hide()
        {
            if (cardRoot != null)
            {
                cardRoot.SetActive(false);
            }

            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }
        }
    }
}
