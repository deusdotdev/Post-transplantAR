using LiverAR.Modules.UI.Runtime.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>AR sahnelerinde üstte kalan eski beyaz şeridi kaldırır; ana ekran butonunu öne alır.</summary>
    public static class ArTopBarCleanup
    {
        public static void Run(Canvas canvas = null)
        {
            if (canvas == null)
            {
                canvas = Object.FindObjectOfType<Canvas>();
            }

            if (canvas == null)
            {
                return;
            }

            var legacy = canvas.transform.Find("HomeBackBarBg");
            if (legacy != null)
            {
                Object.Destroy(legacy.gameObject);
            }

            var home = canvas.transform.Find("BtnHome");
            if (home != null)
            {
                home.SetAsLastSibling();
            }
        }

        public static void EnsureExplorePanelTransparent(GameObject panelRoot)
        {
            if (panelRoot == null)
            {
                return;
            }

            var img = panelRoot.GetComponent<Image>();
            if (img != null)
            {
                img.color = UITheme.Transparent;
                img.raycastTarget = false;
            }

            var sheet = panelRoot.transform.Find("DrugSheetBg");
            if (sheet != null)
            {
                sheet.gameObject.SetActive(false);
            }
        }
    }
}
