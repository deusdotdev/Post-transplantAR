using UnityEngine;

namespace LiverAR.Modules.UI.Runtime.Theme
{
    /// <summary>
    /// Post-transplantAR — yumuşak açık klinik tema (slate + teal + sıcak vurgular).
    /// </summary>
    public static class UITheme
    {
        // Arka planlar (açık, ferah)
        public static readonly Color ViewportBackground = new Color(0.90f, 0.94f, 0.97f, 1f);
        public static readonly Color BackgroundDark = new Color(0.96f, 0.98f, 0.99f, 0.98f);
        public static readonly Color SheetBackground = new Color(0.98f, 0.99f, 1f, 0.96f);
        public static readonly Color Panel = new Color(1f, 1f, 1f, 0.94f);
        public static readonly Color PanelAccent = new Color(0.88f, 0.92f, 0.96f, 1f);
        public static readonly Color GlassPanel = new Color(1f, 1f, 1f, 0.78f);
        public static readonly Color Card = new Color(0.94f, 0.97f, 0.99f, 1f);
        public static readonly Color HomeOverlay = new Color(0.97f, 0.98f, 0.99f, 0.55f);
        public static readonly Color JourneyBodyBackground = new Color(1f, 1f, 1f, 0.82f);
        public static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);
        public static readonly Color OverlayBarTrack = new Color(0.82f, 0.88f, 0.94f, 0.65f);
        public static readonly Color HomeBackButtonBg = new Color(0.10f, 0.12f, 0.16f, 0.88f);

        // Marka / aksiyon
        public static readonly Color Primary = new Color(0.28f, 0.62f, 0.58f, 1f);
        public static readonly Color PrimaryDark = new Color(0.22f, 0.50f, 0.56f, 1f);
        public static readonly Color PrimaryLight = new Color(0.45f, 0.76f, 0.71f, 1f);
        public static readonly Color AccentSecondary = new Color(0.82f, 0.55f, 0.45f, 1f);
        public static readonly Color AccentTertiary = new Color(0.48f, 0.62f, 0.82f, 1f);
        public static readonly Color Success = new Color(0.36f, 0.72f, 0.58f, 1f);
        public static readonly Color Warning = new Color(0.92f, 0.68f, 0.28f, 1f);
        public static readonly Color Danger = new Color(0.86f, 0.38f, 0.42f, 1f);

        // Beslenme listesi
        public static readonly Color NutritionRecommendedBg = new Color(0.88f, 0.96f, 0.91f, 1f);
        public static readonly Color NutritionAvoidBg = new Color(0.99f, 0.91f, 0.91f, 1f);
        public static readonly Color NutritionRecommendedText = new Color(0.12f, 0.38f, 0.30f, 1f);
        public static readonly Color NutritionAvoidText = new Color(0.45f, 0.18f, 0.20f, 1f);

        // Metin (açık zemin üzerinde koyu slate)
        public static readonly Color TextPrimary = new Color(0.14f, 0.20f, 0.28f, 1f);
        public static readonly Color TextSecondary = new Color(0.32f, 0.40f, 0.50f, 1f);
        public static readonly Color TextMuted = new Color(0.48f, 0.55f, 0.64f, 1f);
        public static readonly Color TextOnPrimary = new Color(1f, 1f, 1f, 1f);
        public static readonly Color TextOnPrimaryMuted = new Color(1f, 1f, 1f, 0.9f);
        public static readonly Color TextOnDanger = new Color(1f, 1f, 1f, 1f);

        // Klinik
        public static readonly Color HealthGood = new Color(0.28f, 0.68f, 0.52f, 1f);
        public static readonly Color HealthMid = new Color(0.90f, 0.68f, 0.28f, 1f);
        public static readonly Color HealthBad = new Color(0.86f, 0.38f, 0.42f, 1f);
        public static readonly Color LabNormal = new Color(0.22f, 0.32f, 0.44f, 1f);
        public static readonly Color LabAlert = new Color(0.78f, 0.36f, 0.28f, 1f);

        // Gölge (açık tema)
        public static readonly Color TextShadow = new Color(0f, 0f, 0f, 0.14f);
    }
}
