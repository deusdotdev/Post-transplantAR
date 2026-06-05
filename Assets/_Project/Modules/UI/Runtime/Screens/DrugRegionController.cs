using System.Collections.Generic;
using LiverAR.Modules.Education.Runtime;
using LiverAR.Modules.UI.Runtime.Theme;
using LiverAR.Modules.Visuals.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>
    /// AR sahnesinde ilaç -> bölge etkisini (ok + etiket + parlama) veya anatomi keşfini gösterir.
    /// Mod, <see cref="ARLaunchContext"/> üzerinden gelir. Model yerleştirilince panel açılır.
    /// </summary>
    public sealed class DrugRegionController : MonoBehaviour
    {
        private sealed class TopicItem
        {
            public LiverRegionId Region;
            public string Text;
            public bool Positive;
        }

        private sealed class Topic
        {
            public string Title;
            public string Detail;
            public readonly List<TopicItem> Items = new List<TopicItem>();
        }

        [SerializeField] private ARLaunchContext launchContext;
        [SerializeField] private Camera viewCamera;

        [Header("UI")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private RectTransform topicButtonContainer;
        [SerializeField] private Text titleText;
        [SerializeField] private Text detailText;

        [Header("Ok ölçeği (model boyutuna göre, metre)")]
        [SerializeField] private float arrowReferenceSize = 0.18f;

        private readonly List<Topic> _topics = new List<Topic>();
        private readonly List<RegionAnnotationArrow> _arrows = new List<RegionAnnotationArrow>();
        private readonly Dictionary<LiverRegionId, LiverRegionMarker> _markers =
            new Dictionary<LiverRegionId, LiverRegionMarker>();

        private bool _ready;
        private float _pollTimer;

        private void Awake()
        {
            BuildTopics();
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void Update()
        {
            if (_ready)
            {
                return;
            }

            _pollTimer -= Time.deltaTime;
            if (_pollTimer > 0f)
            {
                return;
            }

            _pollTimer = 0.4f;
            TryActivate();
        }

        private void TryActivate()
        {
            var found = FindObjectsOfType<LiverRegionMarker>();
            if (found == null || found.Length == 0)
            {
                return;
            }

            _markers.Clear();
            foreach (var marker in found)
            {
                _markers[marker.Region] = marker;
            }

            EnsureArrows();
            BuildButtons();

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            _ready = true;
            SelectTopic(0);
        }

        private void BuildTopics()
        {
            _topics.Clear();
            var mode = launchContext != null ? launchContext.CurrentMode : ARLaunchContext.Mode.ExploreAnatomy;

            if (mode == ARLaunchContext.Mode.DrugRegion)
            {
                foreach (var drug in DrugRegionLibrary.GetDrugs())
                {
                    var topic = new Topic { Title = drug.Name, Detail = drug.Summary };
                    foreach (var effect in drug.Effects)
                    {
                        topic.Items.Add(new TopicItem
                        {
                            Region = effect.Region,
                            Text = effect.EffectText,
                            Positive = effect.Positive
                        });
                    }

                    _topics.Add(topic);
                }
            }
            else
            {
                foreach (var region in DrugRegionLibrary.GetRegions())
                {
                    var topic = new Topic { Title = region.Name, Detail = region.Text };
                    topic.Items.Add(new TopicItem
                    {
                        Region = region.Region,
                        Text = region.Text,
                        Positive = true
                    });
                    _topics.Add(topic);
                }
            }
        }

        private void EnsureArrows()
        {
            if (_arrows.Count > 0)
            {
                return;
            }

            // Oklar dünya-uzayında çizilir; Canvas altında değil, sahne kökünde tutulur.
            var root = new GameObject("RegionArrows");

            // Aynı anda gösterilebilecek en fazla ok sayısı = bölge sayısı kadar yeterli.
            var count = Mathf.Max(2, _markers.Count);
            for (var i = 0; i < count; i++)
            {
                var go = new GameObject($"RegionArrow_{i}");
                go.transform.SetParent(root.transform, false);
                var arrow = go.AddComponent<RegionAnnotationArrow>();
                arrow.Initialize(arrowReferenceSize);
                _arrows.Add(arrow);
            }
        }

        private void BuildButtons()
        {
            if (topicButtonContainer == null)
            {
                return;
            }

            for (var i = topicButtonContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(topicButtonContainer.GetChild(i).gameObject);
            }

            for (var i = 0; i < _topics.Count; i++)
            {
                var index = i;
                CreateTopicButton(_topics[i].Title, () => SelectTopic(index));
            }
        }

        private void CreateTopicButton(string label, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject("TopicButton");
            go.transform.SetParent(topicButtonContainer, false);

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 76f;
            le.preferredHeight = 88f;
            le.minWidth = 120f;
            le.preferredWidth = 180f;
            le.flexibleWidth = 1f;

            var img = go.AddComponent<Image>();
            img.color = UITheme.Primary;
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(action);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var rt = labelGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12f, 0f);
            rt.offsetMax = new Vector2(-12f, 0f);

            var text = labelGo.AddComponent<Text>();
            text.font = GetFont();
            text.fontSize = 26;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UITheme.TextOnPrimary;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = label;
        }

        private void SelectTopic(int index)
        {
            if (index < 0 || index >= _topics.Count)
            {
                return;
            }

            ClearVisuals();

            var topic = _topics[index];

            if (titleText != null)
            {
                titleText.text = topic.Title;
            }

            if (detailText != null)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append(topic.Detail);
                foreach (var item in topic.Items)
                {
                    sb.Append("\n\n• ");
                    sb.Append(item.Text);
                }

                detailText.text = sb.ToString();
            }

            var arrowIndex = 0;
            foreach (var item in topic.Items)
            {
                if (!_markers.TryGetValue(item.Region, out var marker) || marker == null)
                {
                    continue;
                }

                var color = item.Positive ? UITheme.Primary : UITheme.AccentSecondary;
                marker.SetHighlightColor(color);
                marker.SetHighlighted(true);

                if (arrowIndex < _arrows.Count)
                {
                    _arrows[arrowIndex].Show(marker, marker.DisplayName, color, ResolveCamera());
                    arrowIndex++;
                }
            }
        }

        private void ClearVisuals()
        {
            foreach (var marker in _markers.Values)
            {
                if (marker != null)
                {
                    marker.SetHighlighted(false);
                }
            }

            foreach (var arrow in _arrows)
            {
                arrow.Hide();
            }
        }

        private Camera ResolveCamera()
        {
            return viewCamera != null ? viewCamera : Camera.main;
        }

        private static Font GetFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                   ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
