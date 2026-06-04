#if UNITY_EDITOR
using System.IO;
using LiverAR.Bootstrap;
using LiverAR.Modules.AR.Runtime.Controllers;
using LiverAR.Modules.Interaction.Runtime.Input;
using LiverAR.Modules.Simulation.Runtime.Data;
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
            SetField(placement, "liverPrefab", CreateLiverPrefab());

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

            var ui = EducationUIBuilder.Build(canvasGo.transform, state, includeArStatusStrip: true);
            EducationUIBuilder.BuildArPlacementPrompt(canvasGo.transform, ui.Flow, placement);

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

            var buildScene = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = new[] { buildScene };
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
