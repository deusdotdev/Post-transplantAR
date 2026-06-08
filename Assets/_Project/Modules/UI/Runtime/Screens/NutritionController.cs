using System.Collections.Generic;
using LiverAR.Modules.Education.Runtime;
using LiverAR.Modules.UI.Runtime.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// Beslenme önerilerini açılır başlıklı (accordion) gruplar halinde gösterir.
    /// Başlığa tıklanınca alt maddeler hemen altındaki buton ile bir sonraki başlık arasında açılır.
    /// </summary>
    public sealed class NutritionController : MonoBehaviour
    {
        [SerializeField] private RectTransform contentContainer;

        private ScrollRect _scrollRect;
        private Font _font;
        private readonly List<GroupSlot> _slots = new List<GroupSlot>();
        private int _activeIndex = -1;

        private const float ContentWidth = 940f;
        private const float HeaderCollapsedHeight = 96f;
        private const float HeaderExpandedHeight = 114f;
        private const int HeaderCollapsedFontSize = 42;
        private const int HeaderExpandedFontSize = 45;
        private const int IndicatorFontSize = 39;
        private const float IndicatorWidth = 48f;
        private const float ContentTopPadding = 88f;
        private const float ItemMinHeight = 88f;
        private const float ItemSpacing = 10f;

        private sealed class GroupSlot
        {
            public Button HeaderButton;
            public Image HeaderImage;
            public LayoutElement HeaderLayout;
            public Text TitleText;
            public Text IndicatorText;
            public GameObject ItemsRoot;
            public LayoutElement ItemsLayout;
            public int Index;
        }

        private void Awake()
        {
            ResolveContentContainer();
            _font = GetFont();
        }

        private void OnEnable()
        {
            ResolveContentContainer();
            if (contentContainer == null)
            {
                Debug.LogWarning("[Nutrition] contentContainer bulunamadı; liste oluşturulamadı.");
                return;
            }

            BuildList();
            RefreshLayout();
        }

        private void ResolveContentContainer()
        {
            if (contentContainer != null)
            {
                _scrollRect = contentContainer.GetComponentInParent<ScrollRect>();
                return;
            }

            _scrollRect = GetComponentInChildren<ScrollRect>(true);
            if (_scrollRect != null)
            {
                contentContainer = _scrollRect.content;
            }
        }

        private void BuildList()
        {
            ClearContainer();
            _slots.Clear();
            _activeIndex = -1;
            ConfigureContentLayout();

            var groups = NutritionLibrary.GetGroups();
            for (var i = 0; i < groups.Length; i++)
            {
                CreateGroup(groups[i], i);
            }
        }

        private void ConfigureContentLayout()
        {
            var layout = contentContainer.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = contentContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.spacing = 18f;
            layout.padding = new RectOffset(20, 20, (int)ContentTopPadding, 36);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private void ClearContainer()
        {
            for (var i = contentContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(contentContainer.GetChild(i).gameObject);
            }
        }

        private void CreateGroup(NutritionGroup group, int index)
        {
            var slot = new GroupSlot { Index = index };

            slot.HeaderButton = CreateHeaderButton(group.Title, index, out slot.HeaderImage,
                out slot.HeaderLayout, out slot.TitleText, out slot.IndicatorText);
            slot.ItemsRoot = CreateItemsContainer(group.Items, out slot.ItemsLayout);
            slot.ItemsRoot.SetActive(false);
            slot.ItemsLayout.preferredHeight = 0f;

            _slots.Add(slot);
        }

        private Button CreateHeaderButton(string title, int index, out Image headerImage,
            out LayoutElement headerLayout, out Text titleText, out Text indicatorText)
        {
            var go = new GameObject("GroupHeader");
            go.transform.SetParent(contentContainer, false);

            headerLayout = go.AddComponent<LayoutElement>();
            headerLayout.minHeight = HeaderCollapsedHeight;
            headerLayout.preferredHeight = HeaderCollapsedHeight;
            headerLayout.preferredWidth = ContentWidth;
            headerLayout.minWidth = ContentWidth;

            headerImage = go.AddComponent<Image>();
            headerImage.color = UITheme.PanelAccent;
            headerImage.raycastTarget = true;

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(UITheme.PanelAccent, Color.white, 0.1f);
            colors.pressedColor = Color.Lerp(UITheme.PanelAccent, UITheme.Primary, 0.25f);
            button.colors = colors;
            button.onClick.AddListener(() => ToggleGroup(index));

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(go.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = Vector2.zero;
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = new Vector2(56f, 10f);
            titleRt.offsetMax = new Vector2(-56f, -10f);

            titleText = titleGo.AddComponent<Text>();
            titleText.font = _font;
            titleText.fontSize = HeaderCollapsedFontSize;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = UITheme.TextPrimary;
            titleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            titleText.verticalOverflow = VerticalWrapMode.Overflow;
            titleText.raycastTarget = false;
            titleText.supportRichText = false;
            titleText.text = title;

            var indicatorGo = new GameObject("Indicator");
            indicatorGo.transform.SetParent(go.transform, false);
            var indicatorRt = indicatorGo.AddComponent<RectTransform>();
            indicatorRt.anchorMin = new Vector2(0f, 0.5f);
            indicatorRt.anchorMax = new Vector2(0f, 0.5f);
            indicatorRt.pivot = new Vector2(0f, 0.5f);
            indicatorRt.anchoredPosition = new Vector2(22f, 0f);
            indicatorRt.sizeDelta = new Vector2(IndicatorWidth, HeaderCollapsedHeight - 16f);

            indicatorText = indicatorGo.AddComponent<Text>();
            indicatorText.font = _font;
            indicatorText.fontSize = IndicatorFontSize;
            indicatorText.fontStyle = FontStyle.Bold;
            indicatorText.alignment = TextAnchor.MiddleCenter;
            indicatorText.color = UITheme.Primary;
            indicatorText.horizontalOverflow = HorizontalWrapMode.Overflow;
            indicatorText.verticalOverflow = VerticalWrapMode.Overflow;
            indicatorText.raycastTarget = false;
            indicatorText.supportRichText = false;
            indicatorText.text = "▶";

            return button;
        }

        private GameObject CreateItemsContainer(NutritionItem[] items, out LayoutElement itemsLayout)
        {
            var go = new GameObject("GroupItems");
            go.transform.SetParent(contentContainer, false);

            itemsLayout = go.AddComponent<LayoutElement>();
            itemsLayout.flexibleWidth = 0f;
            itemsLayout.minHeight = 0f;
            itemsLayout.preferredWidth = ContentWidth;
            itemsLayout.minWidth = ContentWidth;

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 6, 10);
            layout.spacing = ItemSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            foreach (var item in items)
            {
                CreateItemRow(go.transform, item.Text, item.Recommended);
            }

            return go;
        }

        private void CreateItemRow(Transform parent, string content, bool recommended)
        {
            var go = new GameObject("Item");
            go.transform.SetParent(parent, false);

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = ItemMinHeight;
            le.flexibleWidth = 1f;

            var img = go.AddComponent<Image>();
            img.color = recommended ? UITheme.NutritionRecommendedBg : UITheme.NutritionAvoidBg;
            img.raycastTarget = false;

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 14, 14);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);

            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.flexibleWidth = 1f;

            var text = labelGo.AddComponent<Text>();
            text.font = _font;
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = recommended ? UITheme.NutritionRecommendedText : UITheme.NutritionAvoidText;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.supportRichText = false;
            text.text = (recommended ? "+ " : "- ") + content;
        }

        private Text CreateChildText(Transform parent, string name, string content, int fontSize,
            float preferredWidth, Color color, FontStyle style, TextAnchor align)
        {
            var labelGo = new GameObject(name);
            labelGo.transform.SetParent(parent, false);

            if (preferredWidth > 0f)
            {
                var le = labelGo.AddComponent<LayoutElement>();
                le.preferredWidth = preferredWidth;
                le.minWidth = preferredWidth;
            }

            var text = labelGo.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = align;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.supportRichText = false;
            text.text = content;
            return text;
        }

        private void ToggleGroup(int index)
        {
            if (index < 0 || index >= _slots.Count)
            {
                return;
            }

            if (_activeIndex == index)
            {
                CollapseGroup(_slots[index]);
                _activeIndex = -1;
            }
            else
            {
                if (_activeIndex >= 0)
                {
                    CollapseGroup(_slots[_activeIndex]);
                }

                ExpandGroup(_slots[index]);
                _activeIndex = index;
            }

            RefreshLayout();
        }

        private static void ExpandGroup(GroupSlot slot)
        {
            slot.ItemsRoot.SetActive(true);
            slot.ItemsLayout.preferredHeight = -1f;

            slot.HeaderLayout.preferredHeight = HeaderExpandedHeight;
            slot.HeaderLayout.minHeight = HeaderExpandedHeight;
            slot.HeaderImage.color = UITheme.Primary;
            slot.TitleText.fontSize = HeaderExpandedFontSize;
            slot.TitleText.color = UITheme.TextOnPrimary;
            slot.IndicatorText.text = "▼";
            slot.IndicatorText.color = UITheme.TextOnPrimary;
        }

        private static void CollapseGroup(GroupSlot slot)
        {
            slot.ItemsRoot.SetActive(false);
            slot.ItemsLayout.preferredHeight = 0f;

            slot.HeaderLayout.preferredHeight = HeaderCollapsedHeight;
            slot.HeaderLayout.minHeight = HeaderCollapsedHeight;
            slot.HeaderImage.color = UITheme.PanelAccent;
            slot.TitleText.fontSize = HeaderCollapsedFontSize;
            slot.TitleText.color = UITheme.TextPrimary;
            slot.IndicatorText.text = "▶";
            slot.IndicatorText.color = UITheme.Primary;
        }

        private void RefreshLayout()
        {
            if (contentContainer == null)
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
            Canvas.ForceUpdateCanvases();

            if (_scrollRect != null)
            {
                _scrollRect.movementType = ScrollRect.MovementType.Elastic;
                _scrollRect.scrollSensitivity = 40f;
            }
        }

        private static Font GetFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                   ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
