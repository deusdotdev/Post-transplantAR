using UnityEngine;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>Ana ekran ve yolculuk görünümünde 3B karaciğeri UI ile hizalar.</summary>
    public static class HubCameraFraming
    {
        public const float RefScreenHeight = 1920f;

        private const float TitleH = 80f;
        private const float SubtitleH = 54f;
        private const float TitleSubtitleGap = 12f;
        private const float HeaderCardsGap = 36f;
        private const float CardH = 150f;
        private const float CardGap = 28f;
        private const int CardCount = 4;
        private const float SafetyReserve = 100f;

        public static float HomeCardsViewportCenterY => ComputeHomeCardsViewportCenterY();

        public static void FrameForHome(Camera cam, GameObject liver)
        {
            FrameToViewportY(cam, liver, HomeCardsViewportCenterY);
        }

        public static void FrameForJourney(Camera cam, GameObject liver)
        {
            // Üst dashboard (≈170–360px) ile alt metin şeridi (≈1360–1770px) arası orta.
            const float journeyCenterFromTop = 860f;
            var viewportY = 1f - journeyCenterFromTop / RefScreenHeight;
            FrameToViewportY(cam, liver, viewportY);
        }

        private static float ComputeHomeCardsViewportCenterY()
        {
            var blockHeight = TitleH + TitleSubtitleGap + SubtitleH + HeaderCardsGap
                              + CardCount * CardH + (CardCount - 1) * CardGap;
            var blockTop = -(RefScreenHeight * 0.5f - blockHeight * 0.5f - SafetyReserve * 0.25f);

            var headerHeight = TitleH + TitleSubtitleGap + SubtitleH + HeaderCardsGap;
            var cardsTop = blockTop - headerHeight;
            var cardsHeight = CardCount * CardH + (CardCount - 1) * CardGap;
            var cardsBottom = cardsTop - cardsHeight;
            var centerFromTop = (-cardsTop + -cardsBottom) * 0.5f;
            return 1f - centerFromTop / RefScreenHeight;
        }

        private static void FrameToViewportY(Camera cam, GameObject target, float targetViewportY)
        {
            if (cam == null || target == null)
            {
                return;
            }

            var bounds = GetRendererBounds(target);
            var maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z, 0.05f);
            var dist = maxDim * 1.9f + 0.35f;
            var focus = bounds.center;

            cam.transform.position = focus + new Vector3(0f, maxDim * 0.08f, -dist);

            var lookAtYOffset = maxDim * 0.18f;
            for (var i = 0; i < 10; i++)
            {
                cam.transform.LookAt(focus + new Vector3(0f, lookAtYOffset, 0f));
                var screenY = cam.WorldToViewportPoint(bounds.center).y;
                var error = screenY - targetViewportY;
                if (Mathf.Abs(error) < 0.008f)
                {
                    break;
                }

                lookAtYOffset += error * maxDim * 2.4f;
            }

            cam.nearClipPlane = Mathf.Max(0.01f, dist * 0.02f);
            cam.farClipPlane = Mathf.Max(100f, dist * 10f);
        }

        private static Bounds GetRendererBounds(GameObject target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(target.transform.position, Vector3.one * 0.4f);
            }

            var bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            return bounds;
        }
    }
}
