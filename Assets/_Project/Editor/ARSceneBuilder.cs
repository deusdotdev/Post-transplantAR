#if UNITY_EDITOR
using System.IO;
using LiverAR.Bootstrap;
using LiverAR.Modules.AR.Runtime.Controllers;
using LiverAR.Modules.Education.Runtime;
using LiverAR.Modules.Interaction.Runtime.Input;
using LiverAR.Modules.Simulation.Runtime.Data;
using LiverAR.Modules.UI.Runtime.Screens;
using LiverAR.Modules.UI.Runtime.Theme;
using LiverAR.Modules.Visuals.Runtime;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace LiverAR.EditorTools
{
    /// <summary>
    /// Post-transplantAR sahnesini tek tıkla kurar: AR Session, XR Origin (AR),
    /// kamera, Canvas + UI ve tüm script bağlantıları otomatik yapılır.
    /// Menü: Post-transplantAR > Build AR Scene
    /// </summary>
    public static class ARSceneBuilder
    {
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string ScenePath = SceneFolder + "/ARMain.unity";

        [MenuItem("Post-transplantAR/Build AR Scene")]
        public static void BuildScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Play modunu kapat",
                    "Sahne kurulumu Play modunda yapılamaz.\n\n" +
                    "Önce üstteki ▶ (Play) tuşuna basıp Play modundan çık, sonra tekrar dene.",
                    "Tamam");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Bootstrap ---
            var bootstrapGo = new GameObject("Bootstrap");
            bootstrapGo.AddComponent<SceneBootstrap>();

            // --- AR Session ---
            var sessionGo = new GameObject("AR Session");
            var arSession = sessionGo.AddComponent<ARSession>();
            sessionGo.AddComponent<ARInputManager>();
            var sessionController = sessionGo.AddComponent<ARSessionController>();
            SetField(sessionController, "arSession", arSession);

            // --- XR Origin (AR) + kamera ---
            var originGo = new GameObject("XR Origin (AR)");
            var xrOrigin = originGo.AddComponent<XROrigin>();

            var offsetGo = new GameObject("Camera Offset");
            offsetGo.transform.SetParent(originGo.transform, false);

            var cameraGo = new GameObject("AR Camera");
            cameraGo.transform.SetParent(offsetGo.transform, false);
            cameraGo.tag = "MainCamera";

            var cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 20f;

            cameraGo.AddComponent<ARCameraManager>();
            cameraGo.AddComponent<ARCameraBackground>();

            var poseDriver = cameraGo.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(
                TrackedPoseDriver.DeviceType.GenericXRDevice,
                TrackedPoseDriver.TrackedPose.ColorCamera);
            poseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            poseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

            xrOrigin.Camera = cam;
            xrOrigin.CameraFloorOffsetObject = offsetGo;

            var planeManager = originGo.AddComponent<ARPlaneManager>();
            var raycastManager = originGo.AddComponent<ARRaycastManager>();
            var planeMonitor = originGo.AddComponent<PlaneDetectionMonitor>();
            SetField(planeMonitor, "planeManager", planeManager);

            // --- Yerleştirme + etkileşim ---
            var interactionGo = new GameObject("AR Interaction");
            var placement = interactionGo.AddComponent<ARPlacementController>();
            SetField(placement, "raycastManager", raycastManager);
            var liverPrefab = CreateLiverPrefab();
            EnsureRegionMarkers(liverPrefab);
            SetField(placement, "liverPrefab", liverPrefab);

            var tapInput = interactionGo.AddComponent<TapToPlaceInput>();
            SetField(tapInput, "placementController", placement);

            var manipulator = interactionGo.AddComponent<ModelManipulator>();
            SetField(manipulator, "placementController", placement);
            SetFieldBool(manipulator, "enableMouseDrag", true);
            SetFieldBool(manipulator, "limitToUpperViewport", false);
            SetFieldFloat(manipulator, "rotationSpeed", 0.95f);
            SetFieldFloat(manipulator, "mouseRotationSpeed", 10f);
            SetFieldBool(manipulator, "enableAutoRotate", true);
            SetFieldFloat(manipulator, "autoRotateDegreesPerSecond", 22f);
            SetFieldFloat(manipulator, "autoRotateResumeDelay", 0.45f);

            // --- UI (eğitim paneli + AR durum şeridi) ---
            var canvasGo = CreateCanvas();
            var state = LoadOrCreateState();
            state.ResetToDefault();
            state.CurrentScenario = ScenarioType.None;
            EditorUtility.SetDirty(state);

            var launch = LoadOrCreateLaunchContext();
            var ui = EducationUIBuilder.Build(canvasGo.transform, state, includeArStatusStrip: true,
                compactBottomSheet: true, buildScenarioMenu: false);
            EducationUIBuilder.BuildArPlacementPrompt(canvasGo.transform, ui.Flow, placement);
            BuildDrugRegionUI(canvasGo.transform, launch, cam);
            BuildHomeButton(canvasGo.transform);

            // --- Koordinatör ---
            var coordinatorGo = new GameObject("AR Coordinator");
            var coordinator = coordinatorGo.AddComponent<ARExperienceCoordinator>();
            SetField(coordinator, "sessionController", sessionController);
            SetField(coordinator, "planeMonitor", planeMonitor);
            SetField(coordinator, "placementController", placement);
            SetField(coordinator, "setupGuide", ui.SetupGuide);
            SetField(coordinator, "educationFlow", ui.Flow);
            SetFieldBool(ui.Flow, "deferMenuUntilModelPlaced", true);

            // --- EventSystem ---
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            SaveScene(scene);
            EditorUtility.DisplayDialog("AR sahnesi yenilendi",
                "Yeni eğitim arayüzü ARMain'e yazıldı.\nTelefona yüklemeden önce buradan build al.",
                "Tamam");
            Debug.Log("[ARSceneBuilder] Sahne kuruldu: " + ScenePath);
        }

        private static GameObject CreateCanvas()
        {
            var canvasGo = new GameObject("UI Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
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

            // Ana ekran varsa giriş sahnesi (index 0) olarak kalsın; AR ikinci sahne.
            const string hubScenePath = SceneFolder + "/EducationHub.unity";
            var arScene = new EditorBuildSettingsScene(ScenePath, true);
            if (File.Exists(hubScenePath))
            {
                var hub = new EditorBuildSettingsScene(hubScenePath, true);
                EditorBuildSettings.scenes = new[] { hub, arScene };
            }
            else
            {
                EditorBuildSettings.scenes = new[] { arScene };
            }
        }

        private const string LiverPrefabPath = "Assets/_Project/Prefabs/LiverModel.prefab";
        private const string LiverMaterialPath = "Assets/_Project/Materials/LiverMaterial.mat";
        private const string StateAssetPath = "Assets/_Project/SimulationState.asset";

        /// <summary>Koddan üretilen karaciğeri bir prefab asset'ine kaydeder ve döndürür.</summary>
        private static GameObject CreateLiverPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LiverPrefabPath);
            if (existing != null && existing.GetComponentsInChildren<Renderer>(true).Length > 0)
            {
                return existing;
            }

            if (existing != null)
            {
                Debug.LogWarning("[ARSceneBuilder] LiverModel.prefab mesh içermiyor; procedural prefab üretiliyor.");
            }

            var temp = LiverProceduralPrefab.BuildWrapper(0.15f);
            EnsureFolder(Path.GetDirectoryName(LiverPrefabPath));
            var saved = PrefabUtility.SaveAsPrefabAsset(temp, LiverPrefabPath);
            Object.DestroyImmediate(temp);
            return saved;
        }

        private static Material CreateLiverMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(LiverMaterialPath);
            if (existing != null)
            {
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard")
                         ?? Shader.Find("Diffuse");
            var mat = new Material(shader) { name = "LiverMaterial" };
            var color = new Color(0.55f, 0.16f, 0.16f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            EnsureFolder(Path.GetDirectoryName(LiverMaterialPath));
            AssetDatabase.CreateAsset(mat, LiverMaterialPath);
            AssetDatabase.SaveAssets();
            return mat;
        }

        private const string LaunchAssetPath = "Assets/_Project/ARLaunchContext.asset";

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

        /// <summary>Liver prefab'ına bölge çapalarını (Sağ/Sol lob, safra, damar) ekler (idempotent).</summary>
        private static void EnsureRegionMarkers(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
            try
            {
                if (root.GetComponentInChildren<LiverRegionMarker>(true) != null)
                {
                    return; // zaten eklenmiş
                }

                var renderers = root.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                {
                    return;
                }

                var bounds = renderers[0].bounds;
                foreach (var r in renderers)
                {
                    bounds.Encapsulate(r.bounds);
                }

                var c = bounds.center;
                var e = bounds.extents;
                var maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                var radius = Mathf.Max(0.005f, maxDim * 0.05f);
                var labelDist = Mathf.Max(0.04f, maxDim * 0.55f);

                AddMarker(root.transform, "Marker_RightLobe", LiverRegionId.RightLobe, "Sağ lob",
                    c + new Vector3(e.x * 0.45f, e.y * 0.15f, 0f),
                    new Vector3(1f, 0.7f, 0f), labelDist, radius);
                AddMarker(root.transform, "Marker_LeftLobe", LiverRegionId.LeftLobe, "Sol lob",
                    c + new Vector3(-e.x * 0.55f, e.y * 0.1f, 0f),
                    new Vector3(-1f, 0.7f, 0f), labelDist, radius);
                AddMarker(root.transform, "Marker_BileDuct", LiverRegionId.BileDuct, "Safra yolları",
                    c + new Vector3(0f, -e.y * 0.5f, e.z * 0.2f),
                    new Vector3(0.2f, -1f, 0.3f), labelDist, radius);
                AddMarker(root.transform, "Marker_VesselInlet", LiverRegionId.VesselInlet, "Damar girişi",
                    c + new Vector3(0f, e.y * 0.1f, -e.z * 0.5f),
                    new Vector3(0f, 0.6f, -1f), labelDist, radius);

                PrefabUtility.SaveAsPrefabAsset(root, AssetDatabase.GetAssetPath(prefab));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AddMarker(Transform parent, string name, LiverRegionId region,
            string displayName, Vector3 worldPos, Vector3 labelDir, float labelDist, float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, true);
            go.transform.position = worldPos;
            go.transform.rotation = parent.rotation;

            var marker = go.AddComponent<LiverRegionMarker>();
            var so = new SerializedObject(marker);
            so.FindProperty("region").enumValueIndex = (int)region;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("labelDirectionLocal").vector3Value = labelDir;
            so.FindProperty("labelWorldDistance").floatValue = labelDist;
            so.FindProperty("markerWorldRadius").floatValue = radius;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildDrugRegionUI(Transform canvas, ARLaunchContext launch, Camera cam)
        {
            var panel = UiBuildKit.CreatePanel(canvas, "DrugRegionPanel",
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 560f),
                new Color(0.04f, 0.08f, 0.13f, 0.84f));
            panel.SetActive(false);

            var title = UiBuildKit.CreateText(panel.transform, "TitleText",
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -66f), new Vector2(-20f, -12f),
                TextAnchor.MiddleLeft, 30, "Seç", UITheme.TextPrimary, bold: true);

            var detail = UiBuildKit.CreateText(panel.transform, "DetailText",
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 118f), new Vector2(-20f, -72f),
                TextAnchor.UpperLeft, 21, "", UITheme.TextSecondary);

            var container = new GameObject("TopicButtons");
            container.transform.SetParent(panel.transform, false);
            var crt = container.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 0f);
            crt.anchorMax = new Vector2(1f, 0f);
            crt.offsetMin = new Vector2(12f, 12f);
            crt.offsetMax = new Vector2(-12f, 104f);

            var layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Kontrolcü ayrı, her zaman aktif bir nesnede olmalı: panel kapalıyken
            // panelin üzerindeki bileşenin Update'i çalışmaz ve kendini açamaz.
            var controllerGo = new GameObject("Drug Region Controller");
            controllerGo.transform.SetParent(canvas, false);
            var controller = controllerGo.AddComponent<DrugRegionController>();
            UiBuildKit.SetRef(controller, "launchContext", launch);
            UiBuildKit.SetRef(controller, "viewCamera", cam);
            UiBuildKit.SetRef(controller, "panelRoot", panel);
            UiBuildKit.SetRef(controller, "topicButtonContainer", crt);
            UiBuildKit.SetRef(controller, "titleText", title);
            UiBuildKit.SetRef(controller, "detailText", detail);
            UiBuildKit.SetFloat(controller, "arrowReferenceSize", 0.15f);
        }

        private static void BuildHomeButton(Transform canvas)
        {
            var navGo = new GameObject("Scene Navigator");
            navGo.transform.SetParent(canvas, false);
            var nav = navGo.AddComponent<SceneNavigator>();

            UiBuildKit.CreateButton(canvas, "BtnHome", "← Ana ekran",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -70f), new Vector2(236f, -16f),
                UITheme.PanelAccent, nav.GoHome, 22);
        }

        private static LiverAR.Modules.Simulation.Runtime.Data.SimulationState LoadOrCreateState()
        {
            var state = AssetDatabase
                .LoadAssetAtPath<LiverAR.Modules.Simulation.Runtime.Data.SimulationState>(StateAssetPath);
            if (state == null)
            {
                state = ScriptableObject
                    .CreateInstance<LiverAR.Modules.Simulation.Runtime.Data.SimulationState>();
                EnsureFolder(Path.GetDirectoryName(StateAssetPath));
                AssetDatabase.CreateAsset(state, StateAssetPath);
                AssetDatabase.SaveAssets();
            }
            return state;
        }

        private static void EnsureFolder(string dir)
        {
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>private [SerializeField] alanlarına Inspector bağlantısı yapar.</summary>
        private static void SetField(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogWarning($"[ARSceneBuilder] '{field}' alanı bulunamadı: {target.GetType().Name}");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFieldBool(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogWarning($"[ARSceneBuilder] '{field}' alanı bulunamadı: {target.GetType().Name}");
                return;
            }

            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFieldFloat(Object target, string field, float value)
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
    }
}
#endif
