using UnityEngine;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// Geri butonları için yedek: menü kontrolcüsü başka panelde/inactive olsa bile bulur.
    /// </summary>
    public sealed class MenuNavigationActions : MonoBehaviour
    {
        [SerializeField] private EducationMenuController menu;

        public void BackToMainMenu()
        {
            if (menu == null)
            {
                menu = FindObjectOfType<EducationMenuController>();
            }

            menu?.ShowMainMenu();
        }
    }
}
