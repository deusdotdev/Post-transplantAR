using UnityEngine;

namespace LiverAR.Modules.UI.Runtime.Theme
{
    /// <summary>Post-transplantAR — teal + lacivert eğitim arayüzü (yüksek kontrast).</summary>
    public static class UITheme
    {
        // Arka planlar
        public static readonly Color ViewportBackground = new Color(0.05f, 0.09f, 0.14f, 1f);
        public static readonly Color BackgroundDark = new Color(0.04f, 0.08f, 0.12f, 0.98f);
        public static readonly Color SheetBackground = new Color(0.06f, 0.11f, 0.18f, 0.99f);
        public static readonly Color Panel = new Color(0.10f, 0.16f, 0.24f, 1f);
        public static readonly Color PanelAccent = new Color(0.14f, 0.22f, 0.34f, 1f);
        public static readonly Color GlassPanel = new Color(0.07f, 0.13f, 0.20f, 0.98f);
        public static readonly Color Card = new Color(0.09f, 0.15f, 0.23f, 1f);
        /// <summary>AR üzerinde yalnızca metin; kamera görünsün.</summary>
        public static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);
        public static readonly Color OverlayBarTrack = new Color(0f, 0f, 0f, 0.35f);

        // Marka / aksiyon (belirgin teal + mercan vurgu)
        public static readonly Color Primary = new Color(0.15f, 0.78f, 0.72f, 1f);
        public static readonly Color PrimaryDark = new Color(0.08f, 0.52f, 0.50f, 1f);
        public static readonly Color AccentSecondary = new Color(0.95f, 0.48f, 0.32f, 1f);
        public static readonly Color Success = new Color(0.22f, 0.82f, 0.48f, 1f);
        public static readonly Color Warning = new Color(0.98f, 0.72f, 0.18f, 1f);
        public static readonly Color Danger = new Color(0.92f, 0.26f, 0.34f, 1f);

        // Metin
        public static readonly Color TextPrimary = new Color(0.97f, 0.99f, 1f, 1f);
        public static readonly Color TextSecondary = new Color(0.82f, 0.90f, 0.98f, 1f);
        public static readonly Color TextMuted = new Color(0.58f, 0.68f, 0.78f, 1f);
        public static readonly Color TextOnPrimary = new Color(0.02f, 0.08f, 0.10f, 1f);
        public static readonly Color TextOnDanger = new Color(1f, 1f, 1f, 1f);

        // Klinik
        public static readonly Color HealthGood = new Color(0.20f, 0.82f, 0.55f, 1f);
        public static readonly Color HealthMid = new Color(0.95f, 0.75f, 0.20f, 1f);
        public static readonly Color HealthBad = new Color(0.92f, 0.28f, 0.32f, 1f);
        public static readonly Color LabNormal = new Color(0.80f, 0.90f, 1f, 1f);
        public static readonly Color LabAlert = new Color(1f, 0.58f, 0.42f, 1f);
    }
}
