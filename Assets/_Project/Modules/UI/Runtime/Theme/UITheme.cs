using UnityEngine;

namespace LiverAR.Modules.UI.Runtime.Theme
{
    /// <summary>Post-transplantAR eğitim arayüzü renk paleti.</summary>
    public static class UITheme
    {
        public static readonly Color BackgroundDark = new Color(0.06f, 0.08f, 0.12f, 0.92f);
        public static readonly Color Panel = new Color(0.11f, 0.14f, 0.2f, 0.94f);
        public static readonly Color PanelAccent = new Color(0.15f, 0.22f, 0.32f, 0.98f);
        public static readonly Color Primary = new Color(0.2f, 0.55f, 0.95f, 1f);
        public static readonly Color PrimaryDark = new Color(0.12f, 0.38f, 0.72f, 1f);
        public static readonly Color Success = new Color(0.18f, 0.72f, 0.52f, 1f);
        public static readonly Color Warning = new Color(0.95f, 0.55f, 0.15f, 1f);
        public static readonly Color Danger = new Color(0.9f, 0.28f, 0.32f, 1f);
        public static readonly Color TextPrimary = new Color(0.95f, 0.97f, 1f, 1f);
        public static readonly Color TextMuted = new Color(0.65f, 0.72f, 0.82f, 1f);
        public static readonly Color HealthGood = new Color(0.2f, 0.78f, 0.45f, 1f);
        public static readonly Color HealthBad = new Color(0.92f, 0.25f, 0.3f, 1f);

        /// <summary>Yarı saydam panel — AR'de model görünür kalır (referans glass efekti).</summary>
        public static readonly Color GlassPanel = new Color(0.1f, 0.12f, 0.2f, 0.72f);
    }
}
