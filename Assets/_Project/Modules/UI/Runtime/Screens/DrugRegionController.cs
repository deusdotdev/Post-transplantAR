using System.Collections.Generic;
using LiverAR.Modules.AR.Runtime.Controllers;
using LiverAR.Modules.Education.Runtime;
using LiverAR.Modules.Interaction.Runtime.Input;
using LiverAR.Modules.UI.Runtime.Theme;
using LiverAR.Modules.Visuals.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
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
        [SerializeField] private GameObject exploreHintRoot;

        [Header("İlaç modu")]
        [Tooltip("İlaç modunda model yerleştirilince uygulanan ölçek (0.5 = yarı boyut).")]
        [SerializeField] private float drugModelScaleFactor = 0.325f;
        [SerializeField] private float drugArrowReferenceSize = 0.065f;
        [Tooltip("Ok uzunluğu; etiket mesafesine göre kısaltma oranı.")]
        [SerializeField] private float drugArrowLengthScale = 0.36f;
        [Tooltip("Ok ucundaki 3B etiket yazı boyutu çarpanı.")]
        [SerializeField] private float drugArrowLabelScale = 0.33f;

        [Header("Keşfet modu")]
        [Tooltip("Keşfet modunda model yerleştirilince uygulanan ölçek (0.48 ≈ %52 küçük).")]
        [SerializeField] private float exploreModelScaleFactor = 0.48f;
        [SerializeField] private float exploreArrowReferenceSize = 0.10f;
        [SerializeField] private float exploreArrowLengthScale = 0.55f;
        [SerializeField] private float exploreArrowLabelScale = 0.65f;

        private readonly List<Topic> _topics = new List<Topic>();
        private readonly List<RegionAnnotationArrow> _arrows = new List<RegionAnnotationArrow>();
        private readonly Dictionary<LiverRegionId, LiverRegionMarker> _markers =
            new Dictionary<LiverRegionId, LiverRegionMarker>();

        private bool _ready;
        private bool _explore;
        private bool _modelScaled;
        private float _pollTimer;

        // Keşfet modunda seçili olmayan bölgelerin sönük rengi.
        private static readonly Color ExploreBaseColor = new Color(0.85f, 0.88f, 0.95f, 1f);

        private void Awake()
        {
            BuildTopics();
            ArTopBarCleanup.Run();
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (exploreHintRoot != null)
            {
                exploreHintRoot.SetActive(false);
            }
        }

        private void Start()
        {
            StartCoroutine(CleanupTopBarAfterFrame());
        }

        private System.Collections.IEnumerator CleanupTopBarAfterFrame()
        {
            yield return null;
            ArTopBarCleanup.Run();
        }

        private void Update()
        {
            if (!_ready)
            {
                _pollTimer -= Time.deltaTime;
                if (_pollTimer <= 0f)
                {
                    _pollTimer = 0.4f;
                    TryActivate();
                }

                return;
            }

            HandleRegionTap();
        }

        /// <summary>
        /// Modeldeki bir bölgeye dokununca ilgili konuyu seçer (Keşfet modunun kalbi).
        /// UI üzerindeki dokunuşlar ve model yerleştirme jesti ile çakışmaz.
        /// </summary>
        private void HandleRegionTap()
        {
            if (_markers.Count == 0)
            {
                return;
            }

            Vector2 screenPos;
            var fingerId = -1;

            if (UnityEngine.Input.touchCount > 0)
            {
                var touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase != TouchPhase.Began)
                {
                    return;
                }

                screenPos = touch.position;
                fingerId = touch.fingerId;
            }
            else if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                screenPos = UnityEngine.Input.mousePosition;
            }
            else
            {
                return;
            }

            if (IsOverUi(fingerId))
            {
                return;
            }

            var cam = ResolveCamera();
            if (cam == null)
            {
                return;
            }

            var ray = cam.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out var hit, 100f))
            {
                return;
            }

            var marker = hit.collider.GetComponentInParent<LiverRegionMarker>();
            if (marker == null)
            {
                return;
            }

            var index = FindTopicIndexForRegion(marker.Region);
            if (index >= 0)
            {
                SelectTopic(index);
            }
        }

        private int FindTopicIndexForRegion(LiverRegionId region)
        {
            for (var i = 0; i < _topics.Count; i++)
            {
                foreach (var item in _topics[i].Items)
                {
                    if (item.Region == region)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static bool IsOverUi(int fingerId)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return fingerId >= 0
                ? EventSystem.current.IsPointerOverGameObject(fingerId)
                : EventSystem.current.IsPointerOverGameObject();
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

            // Bölgeleri incelerken model kendi kendine dönmesin; kullanıcı elle çevirip baksın.
            foreach (var manipulator in FindObjectsOfType<ModelManipulator>())
            {
                manipulator.SetAutoRotate(false);
            }

            ApplyPlacementModelScale();
            ArTopBarCleanup.Run();

            if (_explore)
            {
                if (topicButtonContainer != null)
                {
                    topicButtonContainer.gameObject.SetActive(false);
                }

                ApplyExplorePanelStyle();
                if (panelRoot != null)
                {
                    panelRoot.SetActive(false);
                }

                _ready = true;
                ShowExploreIdleState();
            }
            else
            {
                ApplyDrugPanelStyle();
                BuildButtons();
                if (panelRoot != null)
                {
                    panelRoot.SetActive(true);
                }

                _ready = true;
                SelectTopic(0);
            }

            ArTopBarCleanup.Run();
        }

        private void ApplyDrugPanelStyle()
        {
            if (panelRoot == null)
            {
                return;
            }

            var img = panelRoot.GetComponent<Image>();
            if (img != null)
            {
                img.color = UITheme.Transparent;
                img.raycastTarget = false;
            }

            var sheet = panelRoot.transform.Find("DrugSheetBg");
            if (sheet != null)
            {
                sheet.gameObject.SetActive(true);
                var sheetImg = sheet.GetComponent<Image>();
                if (sheetImg != null)
                {
                    sheetImg.color = UITheme.SheetBackground;
                    sheetImg.raycastTarget = true;
                }
            }

            if (titleText != null)
            {
                titleText.fontSize = 36;
            }

            if (detailText != null)
            {
                detailText.fontSize = 28;
                detailText.color = UITheme.TextPrimary;
            }
        }

        private void ApplyExplorePanelStyle()
        {
            ArTopBarCleanup.EnsureExplorePanelTransparent(panelRoot);

            if (titleText != null)
            {
                titleText.fontSize = 38;
            }

            if (detailText != null)
            {
                detailText.fontSize = 30;
                detailText.color = UITheme.TextPrimary;
            }
        }

        private void ApplyPlacementModelScale()
        {
            if (_modelScaled)
            {
                return;
            }

            var factor = _explore ? exploreModelScaleFactor : drugModelScaleFactor;
            if (factor <= 0f)
            {
                return;
            }

            var placement = FindObjectOfType<ARPlacementController>();
            if (placement == null || !placement.HasModel)
            {
                return;
            }

            placement.SpawnedObject.transform.localScale *= factor;
            _modelScaled = true;
        }

        /// <summary>Keşfet modu başlangıcı: bölge noktaları görünür, bilgi paneli kapalı.</summary>
        private void ShowExploreIdleState()
        {
            ClearVisuals();

            foreach (var pair in _markers)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                pair.Value.SetHighlightColor(ExploreBaseColor);
                pair.Value.SetHighlighted(true);
            }

            if (titleText != null)
            {
                titleText.text = string.Empty;
            }

            if (detailText != null)
            {
                detailText.text = string.Empty;
            }

            SetExploreHintVisible(true);
            ArTopBarCleanup.Run();
        }

        private void SetExploreHintVisible(bool visible)
        {
            if (!_explore || exploreHintRoot == null)
            {
                return;
            }

            exploreHintRoot.SetActive(visible);
        }

        private void BuildTopics()
        {
            _topics.Clear();
            var mode = launchContext != null ? launchContext.CurrentMode : ARLaunchContext.Mode.ExploreAnatomy;
            _explore = mode == ARLaunchContext.Mode.ExploreAnatomy;

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
            var refSize = _explore ? exploreArrowReferenceSize : drugArrowReferenceSize;
            var lengthScale = _explore ? exploreArrowLengthScale : drugArrowLengthScale;
            var labelScale = _explore ? exploreArrowLabelScale : drugArrowLabelScale;
            for (var i = 0; i < count; i++)
            {
                var go = new GameObject($"RegionArrow_{i}");
                go.transform.SetParent(root.transform, false);
                var arrow = go.AddComponent<RegionAnnotationArrow>();
                arrow.Initialize(refSize, lengthScale, labelScale);
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
            text.fontSize = 28;
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

            if (_explore)
            {
                SetExploreHintVisible(false);
                ApplyExplorePanelStyle();

                if (panelRoot != null)
                {
                    panelRoot.SetActive(true);
                }

                if (titleText != null)
                {
                    titleText.text = topic.Title;
                }

                if (detailText != null)
                {
                    detailText.text = topic.Detail;
                }

                foreach (var pair in _markers)
                {
                    if (pair.Value == null)
                    {
                        continue;
                    }

                    var isSelected = topic.Items.Exists(item => item.Region == pair.Key);
                    pair.Value.SetHighlightColor(isSelected ? UITheme.Primary : ExploreBaseColor);
                    pair.Value.SetHighlighted(true);
                }

                foreach (var item in topic.Items)
                {
                    if (!_markers.TryGetValue(item.Region, out var marker) || marker == null)
                    {
                        continue;
                    }

                    if (_arrows.Count > 0)
                    {
                        _arrows[0].Show(marker, marker.DisplayName, UITheme.Primary, ResolveCamera());
                    }

                    break;
                }

                ArTopBarCleanup.Run();
                return;
            }

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
            // Keşfet modunda noktalar hep görünür kalır; sadece okları temizleriz.
            if (!_explore)
            {
                foreach (var marker in _markers.Values)
                {
                    if (marker != null)
                    {
                        marker.SetHighlighted(false);
                    }
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
