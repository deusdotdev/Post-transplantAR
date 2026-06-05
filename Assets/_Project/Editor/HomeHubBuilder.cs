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
            liver.transform.position = new Vector3(0f, 0.4f, 0f);
            liver.transform.localScale *= 2.2f;
            LiverRenderBootstrap.EnsureVisible(liver);

            var manipulator = liver.AddComponent<LiverAR.Modules.Interaction.Runtime.Input.ModelManipulator>();
            UiBuildKit.SetRef(manipulator, "previewTarget", liver.transform);
            SetBool(manipulator, "enableMouseDrag", true);
            SetBool(manipulator, "enableAutoRotate", true);
            UiBuildKit.SetFloat(manipulator, "autoRotateDegreesPerSecond", 20f);
            UiBuildKit.SetFloat(manipulator, "autoRotateResumeDelay", 0.5f);

            FrameCameraUpper(cam, liver);

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
            var panel = UiBuildKit.CreatePanel(canvas, "HomePanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.04f, 0.08f, 0.13f, 0.78f));

            UiBuildKit.CreateText(panel.transform, "Title",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(40f, -150f), new Vector2(-40f, -70f),
                TextAnchor.MiddleCenter, 44, "Post-transplantAR", UITheme.TextPrimary, bold: true);
            UiBuildKit.CreateText(panel.transform, "Subtitle",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(40f, -210f), new Vector2(-40f, -156f),
                TextAnchor.MiddleCenter, 24, "Nakil sonrası yolculuğunu keşfet. Bir kart seç.",
                UITheme.TextSecondary);

            // 4 kart, dikey ortalı.
            float top = -300f;
            const float h = 150f;
            const float gap = 28f;
            CreateCard(panel.transform, "CardJourney", "Nakil sonrası yolculuğum",
                "Karaciğerin zamanla nasıl iyileşiyor?", top, h, UITheme.Primary, hub.ShowJourney);
            top -= h + gap;
            CreateCard(panel.transform, "CardDrug", "İlaçlarım nereye etki ediyor? (AR)",
                "Bölgelerden çıkan oklarla ilaç etkisi.", top, h, UITheme.PrimaryDark, hub.OpenDrugRegionAR);
            top -= h + gap;
            CreateCard(panel.transform, "CardNutrition", "Beslenme önerilerim",
                "Yapılması ve kaçınılması gerekenler.", top, h, UITheme.AccentSecondary, hub.ShowNutrition);
            top -= h + gap;
            CreateCard(panel.transform, "CardExplore", "Karaciğeri keşfet (AR)",
                "Bölgeleri AR'da yakından incele.", top, h, UITheme.PrimaryDark, hub.OpenExploreAR);

            var safety = UiBuildKit.CreateText(panel.transform, "Safety",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(24f, 24f), new Vector2(-24f, 80f),
                TextAnchor.LowerCenter, 16,
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
                TextAnchor.UpperLeft, 20, subtitle, new Color(0.05f, 0.12f, 0.14f, 0.85f));
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
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(270f, -78f), new Vector2(-24f, -20f),
                TextAnchor.MiddleRight, 15, "Eğitim amaçlıdır; tanı koymaz.", UITheme.TextMuted);
            AddShadow(jSafety.gameObject);

            var stage = UiBuildKit.CreateText(panel.transform, "StageText",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -150f), new Vector2(-24f, -96f),
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
                new Color(0.04f, 0.08f, 0.13f, 0.66f));
            bodyBg.GetComponent<Image>().raycastTarget = false;

            var title = UiBuildKit.CreateText(bodyBg.transform, "JourneyTitle",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -64f), new Vector2(-20f, -12f),
                TextAnchor.MiddleLeft, 28, "", UITheme.TextPrimary, bold: true);
            var body = UiBuildKit.CreateText(bodyBg.transform, "JourneyBody",
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 16f), new Vector2(-20f, -72f),
                TextAnchor.UpperLeft, 21, "", UITheme.TextSecondary);

            var progress = UiBuildKit.CreateText(panel.transform, "Progress",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 96f), new Vector2(120f, 142f),
                TextAnchor.MiddleCenter, 22, "1 / 5", UITheme.TextSecondary);
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
                new Color(0.04f, 0.08f, 0.13f, 0.98f));
            panel.SetActive(false);

            CreateBackBar(panel.transform, hub);

            UiBuildKit.CreateText(panel.transform, "NutritionTitle",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -150f), new Vector2(-24f, -96f),
                TextAnchor.MiddleCenter, 34, "Beslenme önerileri", UITheme.TextPrimary, bold: true);

            var content = UiBuildKit.CreateVerticalScroll(panel.transform, "NutritionScroll",
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(24f, 96f), new Vector2(-24f, -170f));

            var nutrition = panel.AddComponent<NutritionController>();
            UiBuildKit.SetRef(nutrition, "contentContainer", content);

            UiBuildKit.CreateText(panel.transform, "NutritionSafety",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(24f, 24f), new Vector2(-24f, 80f),
                TextAnchor.LowerCenter, 15,
                "Bu öneriler eğitim amaçlıdır; kişisel diyetiniz için ekibinize danışın.", UITheme.TextMuted);

            return panel;
        }

        private static void CreateBackBar(Transform panel, HomeHubController hub)
        {
            UiBuildKit.CreateButton(panel, "BtnHome", "← Ana ekran",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -78f), new Vector2(260f, -18f),
                UITheme.PanelAccent, hub.ShowHome, 24);
        }

        private static void AddShadow(GameObject go)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
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

        private static void FrameCameraUpper(Camera cam, GameObject target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            Bounds bounds;
            if (renderers.Length == 0)
            {
                bounds = new Bounds(target.transform.position, Vector3.one * 0.4f);
            }
            else
            {
                bounds = renderers[0].bounds;
                foreach (var r in renderers)
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            var maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            var dist = maxDim * 1.7f + 0.3f;
            var focus = bounds.center;
            cam.transform.position = focus + new Vector3(0f, maxDim * 0.15f, -dist);
            // Bakışı biraz aşağı al ki model ekranın üst yarısında dursun (altta paneller var).
            cam.transform.LookAt(focus - new Vector3(0f, maxDim * 0.5f, 0f));
            cam.nearClipPlane = Mathf.Max(0.01f, dist * 0.02f);
            cam.farClipPlane = Mathf.Max(100f, dist * 10f);
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
