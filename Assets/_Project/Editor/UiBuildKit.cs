#if UNITY_EDITOR
using LiverAR.Modules.UI.Runtime.Theme;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LiverAR.EditorTools
{
    /// <summary>Sahne kurucuları için ortak uGUI yardımcıları (panel, metin, buton, scroll).</summary>
    internal static class UiBuildKit
    {
        public const float HomeBackButtonHeight = 52f;
        public const float HomeBackTopInset = 76f;
        public const float HomeBackButtonWidth = 228f;
        public const float HomeBackBarPadBottom = 6f;

        public static float HomeBackBarOccupiedHeight =>
            HomeBackTopInset + HomeBackButtonHeight + HomeBackBarPadBottom;

        public static GameObject CreateCanvas(string name = "UI Canvas")
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        public static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        public static Text CreateText(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            TextAnchor align, int size, string content, Color color, bool bold = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var text = go.AddComponent<Text>();
            text.font = GetFont();
            text.fontSize = size;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.alignment = align;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = content;
            return text;
        }

        public static GameObject CreateButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            Color color, UnityAction action, int fontSize = 28)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = UITheme.TextShadow;
            shadow.effectDistance = new Vector2(0f, -3f);

            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.highlightedColor = Color.Lerp(color, Color.white, 0.12f);
            cb.pressedColor = Color.Lerp(color, Color.black, 0.15f);
            btn.colors = cb;

            if (action != null)
            {
                UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
            }

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var lrt = labelGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(12f, 8f);
            lrt.offsetMax = new Vector2(-12f, -8f);

            var lbl = labelGo.AddComponent<Text>();
            lbl.font = GetFont();
            lbl.fontSize = fontSize;
            lbl.fontStyle = FontStyle.Bold;
            lbl.alignment = TextAnchor.MiddleCenter;
            lbl.color = UITheme.TextOnPrimary;
            lbl.horizontalOverflow = HorizontalWrapMode.Wrap;
            lbl.verticalOverflow = VerticalWrapMode.Overflow;
            lbl.text = label;
            return go;
        }

        /// <summary>Üstte kompakt koyu «Ana ekran» butonu (hub, yolculuk, beslenme, AR).</summary>
        public static GameObject CreateHomeBackBar(Transform parent, string buttonName, string label,
            UnityAction onHome, int fontSize = 24)
        {
            var button = CreateButton(parent, buttonName, label,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -(HomeBackTopInset + HomeBackButtonHeight)),
                new Vector2(20f + HomeBackButtonWidth, -HomeBackTopInset),
                UITheme.HomeBackButtonBg, onHome, fontSize);

            var shadow = button.GetComponent<Shadow>();
            if (shadow != null)
            {
                Object.DestroyImmediate(shadow);
            }

            var img = button.GetComponent<Image>();
            if (img != null)
            {
                img.color = UITheme.HomeBackButtonBg;
            }

            var btn = button.GetComponent<Button>();
            if (btn != null)
            {
                var colors = btn.colors;
                colors.normalColor = UITheme.HomeBackButtonBg;
                colors.highlightedColor = Color.Lerp(UITheme.HomeBackButtonBg, Color.white, 0.12f);
                colors.pressedColor = Color.Lerp(UITheme.HomeBackButtonBg, Color.black, 0.18f);
                btn.colors = colors;
            }

            var lbl = button.transform.Find("Label")?.GetComponent<Text>();
            if (lbl != null)
            {
                lbl.color = UITheme.TextOnPrimary;
            }

            button.transform.SetAsLastSibling();
            return button;
        }

        /// <summary>Dikey kaydırılabilir liste oluşturur; doldurulacak content RectTransform'u döndürür.</summary>
        public static RectTransform CreateVerticalScroll(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float spacing = 8f)
        {
            var viewport = new GameObject(name);
            viewport.transform.SetParent(parent, false);
            var vrt = viewport.AddComponent<RectTransform>();
            vrt.anchorMin = anchorMin;
            vrt.anchorMax = anchorMax;
            vrt.offsetMin = offsetMin;
            vrt.offsetMax = offsetMax;
            // Görünmez arka plan: ScrollRect'in parmakla kaydırma alması için raycast gerekir.
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            var scroll = viewport.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 24f;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var crt = content.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vrt;
            scroll.content = crt;
            return crt;
        }

        public static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogWarning($"[UiBuildKit] Alan yok: {field} ({target.GetType().Name})");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetString(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                return;
            }

            prop.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                return;
            }

            prop.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static Font GetFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                   ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
#endif
