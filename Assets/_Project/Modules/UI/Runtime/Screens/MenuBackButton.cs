using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>Geri butonu — serialized onClick kırılsa bile çalışır.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class MenuBackButton : MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            var nav = FindObjectOfType<MenuNavigationActions>();
            if (nav != null)
            {
                nav.BackToMainMenu();
                return;
            }

            FindObjectOfType<EducationMenuController>()?.ShowMainMenu();
        }
    }
}
