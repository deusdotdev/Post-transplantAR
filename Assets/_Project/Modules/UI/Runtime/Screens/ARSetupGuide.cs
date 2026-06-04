using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// Kullanıcıya AR kurulum/yerleştirme yönlendirmesi ve kalıcı güvenlik uyarısı gösterir.
    /// RAMS - Safety: "Bu uygulama tanı koymaz." mesajı kritik ekranda sürekli görünür.
    /// </summary>
    public sealed class ARSetupGuide : MonoBehaviour
    {
        public enum GuideState
        {
            CheckingDevice,
            Unsupported,
            Initializing,
            SearchingSurface,
            ReadyToPlace,
            ModelPlaced
        }

        [Header("Metin Alanları")]
        [SerializeField] private Text statusText;
        [SerializeField] private Text safetyText;

        [Header("Güvenlik Mesajı")]
        [TextArea]
        [SerializeField] private string safetyMessage =
            "Bu uygulama tanı koymaz; yalnızca eğitim amaçlıdır. Sorularınız için doktorunuza danışın.";

        private void Awake()
        {
            if (safetyText != null)
            {
                safetyText.text = safetyMessage;
            }
        }

        public void SetStatus(string value)
        {
            if (statusText != null)
            {
                statusText.text = value;
            }
        }

        public void ShowState(GuideState state)
        {
            SetStatus(GetMessage(state));
        }

        private static string GetMessage(GuideState state)
        {
            switch (state)
            {
                case GuideState.CheckingDevice:
                    return "Cihaz uyumluluğu kontrol ediliyor...";
                case GuideState.Unsupported:
                    return "Bu cihaz AR deneyimini desteklemiyor. İçeriği AR olmadan inceleyebilirsiniz.";
                case GuideState.Initializing:
                    return "AR başlatılıyor, lütfen cihazı sabit tutun.";
                case GuideState.SearchingSurface:
                    return "Düz bir yüzeye yavaşça doğru hareket edin. İyi aydınlatma yerleştirmeyi kolaylaştırır.";
                case GuideState.ReadyToPlace:
                    return "Yüzey bulundu. Modeli yerleştirmek için ekrana dokunun.";
                case GuideState.ModelPlaced:
                    return "Model yerleştirildi. Tek parmakla döndürün, iki parmakla boyutlandırın.";
                default:
                    return string.Empty;
            }
        }
    }
}
