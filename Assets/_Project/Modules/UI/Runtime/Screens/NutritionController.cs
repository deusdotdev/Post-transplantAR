using LiverAR.Modules.Education.Runtime;
using LiverAR.Modules.UI.Runtime.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// Beslenme önerilerini gruplanmış, renk kodlu (yap/kaçın) bir liste olarak doldurur.
    /// İçerik kaydırılabilir bir kapsayıcıya çalışma zamanında eklenir.
    /// </summary>
    public sealed class NutritionController : MonoBehaviour
    {
        [SerializeField] private RectTransform contentContainer;

        private bool _built;

        private void OnEnable()
        {
            if (!_built)
            {
                BuildList();
                _built = true;
            }
        }

        private void BuildList()
        {
            if (contentContainer == null)
            {
                return;
            }

            foreach (var group in NutritionLibrary.GetGroups())
            {
                CreateHeader(group.Title);
                foreach (var item in group.Items)
                {
                    CreateItemRow(item.Text, item.Recommended);
                }
            }
        }

        private void CreateHeader(string title)
        {
            var go = new GameObject("GroupHeader");
            go.transform.SetParent(contentContainer, false);

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 52f;
            le.preferredHeight = 52f;

            var text = go.AddComponent<Text>();
            text.font = GetFont();
            text.fontSize = 26;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.LowerLeft;
            text.color = UITheme.Primary;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = title;
        }

        private void CreateItemRow(string content, bool recommended)
        {
            var go = new GameObject("Item");
            go.transform.SetParent(contentContainer, false);

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 64f;
            le.flexibleHeight = 0f;

            var img = go.AddComponent<Image>();
            img.color = recommended
                ? new Color(0.12f, 0.30f, 0.26f, 0.9f)
                : new Color(0.34f, 0.12f, 0.14f, 0.9f);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var rt = labelGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(16f, 6f);
            rt.offsetMax = new Vector2(-16f, -6f);

            var text = labelGo.AddComponent<Text>();
            text.font = GetFont();
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = UITheme.TextPrimary;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = (recommended ? "✓  " : "✕  ") + content;
        }

        private static Font GetFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                   ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
