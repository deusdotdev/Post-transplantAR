using UnityEngine;

namespace LiverAR.Modules.UI.Runtime
{
    /// <summary>uGUI kardeş sırası: son çizilen öğe üstte görünür.</summary>
    public static class UiLayer
    {
        public static void BringToFront(Transform target)
        {
            if (target != null)
            {
                target.SetAsLastSibling();
            }
        }

        public static void BringToFront(GameObject target)
        {
            if (target != null)
            {
                BringToFront(target.transform);
            }
        }
    }
}
