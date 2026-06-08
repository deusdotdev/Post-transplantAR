#if UNITY_EDITOR
using System.IO;
using LiverAR.Modules.Education.Runtime;
using LiverAR.Modules.Simulation.Runtime;
using LiverAR.Modules.Simulation.Runtime.Data;
using LiverAR.Modules.UI.Runtime.Screens;
using LiverAR.Modules.UI.Runtime.Theme;
using LiverAR.Modules.Visuals.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LiverAR.EditorTools
{
    /// <summary>
    /// Kart bazlı ana ekranı (EducationHub) kurar: 4 kart, AR'sız iyileşme yolculuğu ve
    /// beslenme paneli. AR sadece ilgili karttan açılır. Menü: Post-transplantAR > Build Home Hub
    /// </summary>
    public static class HomeHubBuilder
    {
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string ScenePath = SceneFolder + "/EducationHub.unity";
        private const string ArScenePath = SceneFolder + "/ARMain.unity";
        private const string StateAssetPath = "Assets/_Project/SimulationState.asset";
        private const string LaunchAssetPath = "Assets/_Project/ARLaunchContext.asset";
        private const string LiverPrefabPath = "Assets/_Project/Prefabs/LiverModel.prefab";
        private const string LiverMaterialPath = "Assets/_Project/Materials/LiverMaterial.mat";

        [MenuItem("Post-transplantAR/Build Home Hub")]
        public static void BuildHub()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Play modunu kapat",
                    "Sahne kurulumu Play modunda yapılamaz. Önce ▶ ile Play modundan çık.", "Tamam");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var state = LoadOrCreateState();
            state.ResetToDefault();
            state.CurrentScenario = ScenarioType.None;
            EditorUtility.SetDirty(state);

            var launch = LoadOrCreateLaunchContext();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Kamera + ışık ---
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = UITheme.ViewportBackground;
            camGo.AddComponent<AudioListener>();

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // --- 3B karaciğer sahnesi (yolculukta görünür) ---
            var stage = new GameObject("LiverStage");
            var liver = CreateLiver(state);
            liver.transform.SetParent(stage.transform, false);
            liver.transform.position = new Vector3(0f, 0.08f, 0f);
            liver.transform.localScale *= 1.54f; // 2.2'nin %70'i (yatayda ~%30 küçültme)
            LiverRenderBootstrap.EnsureVisible(liver);

            var manipulator = liver.AddComponent<LiverAR.Modules.Interaction.Runtime.Input.ModelManipulator>();
            UiBuildKit.SetRef(manipulator, "previewTarget", liver.transform);
            SetBool(manipulator, "enableMouseDrag", true);
            SetBool(manipulator, "enableAutoRotate", true);
            UiBuildKit.SetFloat(manipulator, "autoRotateDegreesPerSecond", 20f);
            UiBuildKit.SetFloat(manipulator, "autoRotateResumeDelay", 0.5f);

            HubCameraFraming.FrameForHome(cam, liver);

            // --- Simülasyon kontrolcüsü (yolculuk + dashboard ortak) ---
            var simGo = new GameObject("Simulation");
            var controller = simGo.AddComponent<SimulationController>();
            UiBuildKit.SetRef(controller, "state", state);

            // --- Canvas ---
            var canvasGo = UiBuildKit.CreateCanvas();
            var canvas = canvasGo.transform;

            var hubGo = new GameObject("Hub Controller");
            var hub = hubGo.AddComponent<HomeHubController>();

            var homePanel = BuildHomePanel(canvas, hub);
            var journeyPanel = BuildJourneyPanel(canvas, hub, controller, state);
            var nutritionPanel = BuildNutritionPanel(canvas, hub);

            UiBuildKit.SetRef(hub, "arLaunchContext", launch);
            UiBuildKit.SetString(hub, "arSceneName", "ARMain");
            UiBuildKit.SetRef(hub, "homePanel", homePanel);
            UiBuildKit.SetRef(hub, "journeyPanel", journeyPanel);
            UiBuildKit.SetRef(hub, "nutritionPanel", nutritionPanel);
            UiBuildKit.SetRef(hub, "liverStage", stage);
            UiBuildKit.SetRef(hub, "liverModel", liver);
            UiBuildKit.SetRef(hub, "hubCamera", cam);

            EnsureEventSystem();
            SaveScene(scene);
            EnsureBuildSettings();

            EditorUtility.DisplayDialog("Ana ekran kuruldu",
                "EducationHub sahnesi 4 kart + yolculuk + beslenme ile kuruldu ve giriş sahnesi yapıldı.\n\n" +
                "AR kartları için: Post-transplantAR > Build AR Scene.",
                "Tamam");
            Debug.Log("[HomeHubBuilder] Ana ekran kuruldu: " + ScenePath);
        }

        private static GameObject BuildHomePanel(Transform canvas, HomeHubController hub)
        {
            // Tam ekran saydam: ortadaki karaciğer boşluğunda 3B model görünsün.
            var panel = UiBuildKit.CreatePanel(canvas, "HomePanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                UITheme.Transparent);
            panel.GetComponent<Image>().raycastTarget = false;

            const float refHeight = HubCameraFraming.RefScreenHeight;
            const float titleH = 80f;
            const float subtitleH = 54f;
            const float titleSubtitleGap = 12f;
            const float headerCardsGap = 36f;
            const float cardH = 150f;
            const float cardGap = 28f;
            const int cardCount = 4;
            const float safetyReserve = 100f;

            var blockHeight = titleH + titleSubtitleGap + subtitleH + headerCardsGap
                              + cardCount * cardH + (cardCount - 1) * cardGap;
            var blockTop = -(refHeight * 0.5f - blockHeight * 0.5f - safetyReserve * 0.25f);

            var y = blockTop;

            var headerBg = UiBuildKit.CreatePanel(panel.transform, "HeaderBg",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, y - titleH - titleSubtitleGap - subtitleH - 24f), new Vector2(0f, 0f),
                UITheme.HomeOverlay);
            headerBg.GetComponent<Image>().raycastTarget = false;
            headerBg.transform.SetAsFirstSibling();

            var title = UiBuildKit.CreateText(panel.transform, "Title",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(40f, y - titleH), new Vector2(-40f, y),
                TextAnchor.MiddleCenter, 44, "Senaryolar", UITheme.TextPrimary, bold: true);
            AddShadow(title.gameObject);
            y -= titleH + titleSubtitleGap;

            var subtitle = UiBuildKit.CreateText(panel.transform, "Subtitle",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(40f, y - subtitleH), new Vector2(-40f, y),
                TextAnchor.MiddleCenter, 24, "Nakil sonrası yolculuğunu keşfet. Bir kart seç.",
                UITheme.TextSecondary);
            AddShadow(subtitle.gameObject);
            y -= subtitleH + headerCardsGap;

            CreateCard(panel.transform, "CardJourney", "Nakil sonrası yolculuğum",
                "Karaciğerin zamanla nasıl iyileşiyor?", y, cardH, UITheme.Primary, hub.ShowJourney);
            y -= cardH + cardGap;
            CreateCard(panel.transform, "CardDrug", "İlaçlarım nereye etki ediyor? (AR)",
                "Bölgelerden çıkan oklarla ilaç etkisi.", y, cardH, UITheme.PrimaryDark, hub.OpenDrugRegionAR);
            y -= cardH + cardGap;
            CreateCard(panel.transform, "CardNutrition", "Beslenme önerilerim",
                "Yapılması ve kaçınılması gerekenler.", y, cardH, UITheme.AccentSecondary, hub.ShowNutrition);
            y -= cardH + cardGap;
            CreateCard(panel.transform, "CardExplore", "Karaciğeri keşfet (AR)",
                "Bölgeleri AR'da yakından incele.", y, cardH, UITheme.AccentTertiary, hub.OpenExploreAR);

            var safety = UiBuildKit.CreateText(panel.transform, "Safety",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(28f, 52f), new Vector2(-28f, 128f),
                TextAnchor.LowerCenter, 28,
                "Bu uygulama tanı koymaz; yalnızca eğitim amaçlıdır. Sorularınız için doktorunuza danışın.",
                UITheme.TextMuted);
            AddShadow(safety.gameObject);

            return panel;
        }

        private static void CreateCard(Transform parent, string name, string title, string subtitle,
            float top, float height, Color color, UnityEngine.Events.UnityAction action)
        {
            var card = UiBuildKit.CreateButton(parent, name, "",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-470f, top - height), new Vector2(470f, top),
                color, action);

            // Buton etiketini kart başlık/alt başlık olarak yeniden düzenle.
            var label = card.transform.Find("Label");
            if (label != null)
            {
                Object.DestroyImmediate(label.gameObject);
            }

            UiBuildKit.CreateText(card.transform, "CardTitle",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -64f), new Vector2(-28f, -12f),
                TextAnchor.LowerLeft, 30, title, UITheme.TextOnPrimary, bold: true);
            UiBuildKit.CreateText(card.transform, "CardSubtitle",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(28f, 16f), new Vector2(-28f, 64f),
                TextAnchor.UpperLeft, 22, subtitle, UITheme.TextOnPrimaryMuted);
        }

        private static GameObject BuildJourneyPanel(Transform canvas, HomeHubController hub,
            SimulationController controller, SimulationState state)
        {
            // Saydam: arkadaki 3B karaciğer görünsün.
            var panel = UiBuildKit.CreatePanel(canvas, "JourneyPanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, UITheme.Transparent);
            panel.GetComponent<Image>().raycastTarget = false;
            panel.SetActive(false);

            CreateBackBar(panel.transform, hub);

            var jSafety = UiBuildKit.CreateText(panel.transform, "JourneySafety",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -168f), new Vector2(-24f, -122f),
                TextAnchor.MiddleCenter, 18, "Eğitim amaçlıdır; tanı koymaz.", UITheme.TextMuted);
            AddShadow(jSafety.gameObject);

            var stage = UiBuildKit.CreateText(panel.transform, "StageText",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -236f), new Vector2(-24f, -182f),
                TextAnchor.MiddleCenter, 34, "0. gün", UITheme.Primary, bold: true);
            AddShadow(stage.gameObject);

            // Klinik dashboard bandı (üst).
            var dashHolder = UiBuildKit.CreatePanel(panel.transform, "DashHolder",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -360f), new Vector2(-20f, -170f),
                UITheme.Transparent);
            dashHolder.GetComponent<Image>().raycastTarget = false;
            var dashboard = EducationUIBuilder.BuildDashboard(dashHolder.transform, controller, compact: true);

            // Açıklama için yarı saydam okunaklı şerit (alt).
            var bodyBg = UiBuildKit.CreatePanel(panel.transform, "BodyBg",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 150f), new Vector2(-16f, 560f),
                UITheme.JourneyBodyBackground);
            bodyBg.GetComponent<Image>().raycastTarget = false;

            var title = UiBuildKit.CreateText(bodyBg.transform, "JourneyTitle",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -64f), new Vector2(-20f, -12f),
                TextAnchor.MiddleLeft, 30, "", UITheme.TextPrimary, bold: true);
            var body = UiBuildKit.CreateText(bodyBg.transform, "JourneyBody",
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 16f), new Vector2(-20f, -72f),
                TextAnchor.UpperLeft, 25, "", UITheme.TextSecondary);

            var progress = UiBuildKit.CreateText(panel.transform, "Progress",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 96f), new Vector2(120f, 142f),
                TextAnchor.MiddleCenter, 24, "1 / 5", UITheme.TextSecondary);
            AddShadow(progress.gameObject);

            var journey = panel.AddComponent<RecoveryJourneyController>();

            var prevBtn = UiBuildKit.CreateButton(panel.transform, "BtnPrev", "← Geri",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 28f), new Vector2(300f, 128f),
                UITheme.PanelAccent, journey.OnPrevious);
            var nextBtn = UiBuildKit.CreateButton(panel.transform, "BtnNext", "İleri →",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-300f, 28f), new Vector2(-24f, 128f),
                UITheme.Primary, journey.OnNext);

            UiBuildKit.SetRef(journey, "controller", controller);
            UiBuildKit.SetRef(journey, "state", state);
            UiBuildKit.SetRef(journey, "dashboard", dashboard);
            UiBuildKit.SetRef(journey, "stageText", stage);
            UiBuildKit.SetRef(journey, "titleText", title);
            UiBuildKit.SetRef(journey, "bodyText", body);
            UiBuildKit.SetRef(journey, "progressText", progress);
            UiBuildKit.SetRef(journey, "previousButton", prevBtn.GetComponent<Button>());
            UiBuildKit.SetRef(journey, "nextButton", nextBtn.GetComponent<Button>());

            return panel;
        }

        private static GameObject BuildNutritionPanel(Transform canvas, HomeHubController hub)
        {
            var panel = UiBuildKit.CreatePanel(canvas, "NutritionPanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                UITheme.BackgroundDark);
            panel.SetActive(false);

            CreateBackBar(panel.transform, hub);

            UiBuildKit.CreateText(panel.transform, "NutritionTitle",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -236f), new Vector2(-24f, -182f),
                TextAnchor.MiddleCenter, 34, "Beslenme önerileri", UITheme.TextPrimary, bold: true);

            // Alt uyarı şeridinin üstünde kalsın; dokunma çakışması olmasın.
            var content = UiBuildKit.CreateVerticalScroll(panel.transform, "NutritionScroll",
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(12f, 120f), new Vector2(-12f, -256f), 12f);

            var nutrition = panel.AddComponent<NutritionController>();
            UiBuildKit.SetRef(nutrition, "contentContainer", content);

            var safety = UiBuildKit.CreateText(panel.transform, "NutritionSafety",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(24f, 24f), new Vector2(-24f, 100f),
                TextAnchor.LowerCenter, 18,
                "Bu öneriler eğitim amaçlıdır; kişisel diyetiniz için ekibinize danışın.", UITheme.TextMuted);
            safety.raycastTarget = false;

            return panel;
        }

        private static void CreateBackBar(Transform panel, HomeHubController hub)
        {
            UiBuildKit.CreateHomeBackBar(panel, "BtnHome", "← Ana ekran", hub.ShowHome, 24);
        }

        private static void AddShadow(GameObject go)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = UITheme.TextShadow;
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        private static GameObject CreateLiver(SimulationState state)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LiverPrefabPath);
            if (prefab != null)
            {
                return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            }

            var liver = new GameObject("Liver (Procedural)");
            liver.AddComponent<MeshFilter>();
            var liverRenderer = liver.AddComponent<MeshRenderer>();
            liverRenderer.sharedMaterial = CreateLiverMaterial();
            liver.AddComponent<LiverMeshGenerator>();
            var visual = liver.AddComponent<LiverVisualController>();
            UiBuildKit.SetRef(visual, "state", state);
            UiBuildKit.SetRef(visual, "liverRenderer", liverRenderer);
            UiBuildKit.SetRef(visual, "liverTransform", liver.transform);
            return liver;
        }

        private static Material CreateLiverMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(LiverMaterialPath);
            if (existing != null)
            {
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = "LiverMaterial" };
            var color = new Color(0.55f, 0.16f, 0.16f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            var dir = Path.GetDirectoryName(LiverMaterialPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(mat, LiverMaterialPath);
            AssetDatabase.SaveAssets();
            return mat;
        }

        private static SimulationState LoadOrCreateState()
        {
            var state = AssetDatabase.LoadAssetAtPath<SimulationState>(StateAssetPath);
            if (state == null)
            {
                state = ScriptableObject.CreateInstance<SimulationState>();
                EnsureFolder(Path.GetDirectoryName(StateAssetPath));
                AssetDatabase.CreateAsset(state, StateAssetPath);
                AssetDatabase.SaveAssets();
            }
            return state;
        }

        private static ARLaunchContext LoadOrCreateLaunchContext()
        {
            var ctx = AssetDatabase.LoadAssetAtPath<ARLaunchContext>(LaunchAssetPath);
            if (ctx == null)
            {
                ctx = ScriptableObject.CreateInstance<ARLaunchContext>();
                EnsureFolder(Path.GetDirectoryName(LaunchAssetPath));
                AssetDatabase.CreateAsset(ctx, LaunchAssetPath);
                AssetDatabase.SaveAssets();
            }
            return ctx;
        }

        private static void EnsureBuildSettings()
        {
            var hub = new EditorBuildSettingsScene(ScenePath, true);
            if (File.Exists(ArScenePath))
            {
                var ar = new EditorBuildSettingsScene(ArScenePath, true);
                EditorBuildSettings.scenes = new[] { hub, ar };
            }
            else
            {
                EditorBuildSettings.scenes = new[] { hub };
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }
        }

        private static void SaveScene(Scene scene)
        {
            if (!Directory.Exists(SceneFolder))
            {
                Directory.CreateDirectory(SceneFolder);
                AssetDatabase.Refresh();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void EnsureFolder(string dir)
        {
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
        }

        private static void SetBool(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                return;
            }

            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
