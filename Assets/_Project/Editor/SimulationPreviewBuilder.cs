#if UNITY_EDITOR
using System.IO;
using LiverAR.Modules.Interaction.Runtime.Input;
using LiverAR.Modules.Simulation.Runtime;
using LiverAR.Modules.Simulation.Runtime.Data;
using LiverAR.Modules.UI.Runtime.Screens;
using LiverAR.Modules.Visuals.Runtime;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LiverAR.EditorTools
{
    /// <summary>
    /// AR olmadan, Editor'de Play ile test edilebilen senaryo önizleme sahnesi kurar.
    /// Karaciğer yerine bir küre + senaryo butonlu HUD oluşturur ve her şeyi bağlar.
    /// Menü: Post-transplantAR > Build Simulation Preview (No AR)
    /// </summary>
    public static class SimulationPreviewBuilder
    {
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string ScenePath = SceneFolder + "/SimulationPreview.unity";
        private const string StateAssetPath = "Assets/_Project/SimulationState.asset";

        [MenuItem("Post-transplantAR/Build Simulation Preview (No AR)")]
        public static void BuildPreview()
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

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Kamera + ışık ---
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // --- Karaciğer: indirilen model prefab'ı varsa onu, yoksa procedural mesh'i kullan ---
            var liver = CreateLiver(state);
            liver.transform.position = new Vector3(0f, 1.35f, 0.4f);
            liver.transform.localScale *= 2.5f;
            LiverRenderBootstrap.EnsureVisible(liver);

            var manipulator = liver.AddComponent<ModelManipulator>();
            SetField(manipulator, "previewTarget", liver.transform);
            SetFieldBool(manipulator, "enableMouseDrag", true);
            SetFieldBool(manipulator, "limitToUpperViewport", true);
            SetPropertyFloat(manipulator, "viewportMinYNormalized", 0.30f);
            SetPropertyFloat(manipulator, "rotationSpeed", 0.95f);
            SetPropertyFloat(manipulator, "mouseRotationSpeed", 10f);
            SetFieldBool(manipulator, "enableAutoRotate", true);
            SetPropertyFloat(manipulator, "autoRotateDegreesPerSecond", 22f);
            SetPropertyFloat(manipulator, "autoRotateResumeDelay", 0.45f);

            FrameCameraUpperViewport(cam, liver);
            cam.rect = new Rect(0f, 0.30f, 1f, 0.70f);

            // --- UI (zengin eğitim paneli; önizlemede tam ekran) ---
            var canvasGo = CreateCanvas();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = LiverAR.Modules.UI.Runtime.Theme.UITheme.ViewportBackground;
            // Alt menü + üstte 3D karaciğer görünsün (tam ekran Backdrop modeli gizliyordu).
            EducationUIBuilder.Build(canvasGo.transform, state, includeArStatusStrip: false,
                compactBottomSheet: true);

            EnsureEventSystem();

            SaveScene(scene);
            EditorUtility.DisplayDialog("Önizleme sahnesi yenilendi",
                "Üstte karaciğer 3D, altta menü.\n\n" +
                "Döndürme: üst siyah alanda fareyle sürükle (veya telefonda tek parmak).\n\n" +
                "Play'e basmadan önce ▶ kapalı olsun.\n" +
                "Gerçek AR için: Build AR Scene + ARMain sahnesi.",
                "Tamam");
            Debug.Log("[SimulationPreviewBuilder] Önizleme sahnesi kuruldu: " + ScenePath);
        }

        private static SimulationState LoadOrCreateState()
        {
            var state = AssetDatabase.LoadAssetAtPath<SimulationState>(StateAssetPath);
            if (state == null)
            {
                state = ScriptableObject.CreateInstance<SimulationState>();
                var dir = Path.GetDirectoryName(StateAssetPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                AssetDatabase.CreateAsset(state, StateAssetPath);
                AssetDatabase.SaveAssets();
            }
            return state;
        }

        private const string LiverPrefabPath = "Assets/_Project/Prefabs/LiverModel.prefab";
        private const string LiverMaterialPath = "Assets/_Project/Materials/LiverMaterial.mat";

        private static GameObject CreateLiver(SimulationState state)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LiverPrefabPath);
            if (prefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = new Vector3(0f, 0.8f, 0f);
                return instance;
            }

            // Prefab yoksa procedural karaciğer üret.
            var liver = new GameObject("Liver (Procedural)");
            liver.transform.position = new Vector3(0f, 0.8f, 0f);
            liver.AddComponent<MeshFilter>();
            var liverRenderer = liver.AddComponent<MeshRenderer>();
            liverRenderer.sharedMaterial = CreateLiverMaterial();
            liver.AddComponent<LiverMeshGenerator>();
            var visual = liver.AddComponent<LiverVisualController>();
            SetField(visual, "state", state);
            SetField(visual, "liverRenderer", liverRenderer);
            SetField(visual, "liverTransform", liver.transform);
            return liver;
        }

        /// <summary>Modeli ekranın üst ~%60 bölgesinde gösterecek kamera.</summary>
        private static void FrameCameraUpperViewport(Camera cam, GameObject target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                cam.transform.position = new Vector3(0f, 1.2f, -2.2f);
                cam.transform.LookAt(new Vector3(0f, 1.35f, 0.4f));
                return;
            }

            var bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            var maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            var dist = maxDim * 1.35f + 0.25f;
            var focus = bounds.center;
            cam.transform.position = focus + new Vector3(0f, maxDim * 0.05f, -dist);
            cam.transform.LookAt(focus);
            cam.nearClipPlane = Mathf.Max(0.01f, dist * 0.02f);
            cam.farClipPlane = Mathf.Max(100f, dist * 10f);
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
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }

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

        private static Text CreateText(Transform parent, string name, Vector2 anchor,
            Vector2 anchoredPos, TextAnchor align, int fontSize, string content)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = new Vector2(1f - anchor.x, anchor.y);
            rt.pivot = new Vector2(0.5f, anchor.y);
            rt.sizeDelta = new Vector2(-80f, 160f);
            rt.anchoredPosition = anchoredPos;

            var text = go.AddComponent<Text>();
            text.font = GetDefaultFont();
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = content;
            return text;
        }

        private static void CreateButton(Transform parent, string name, string label,
            Vector2 anchoredPos, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(260f, 110f);
            rt.anchoredPosition = anchoredPos;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.16f, 0.5f, 0.86f, 1f);
            var button = go.AddComponent<Button>();
            UnityEventTools.AddVoidPersistentListener(button.onClick, action);

            var labelText = CreateText(go.transform, "Label",
                Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, 24, label);
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
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

        private static Font GetDefaultFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            return font;
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

        private static void SetField(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogWarning($"[SimulationPreviewBuilder] '{field}' alanı bulunamadı: {target.GetType().Name}");
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
                return;
            }

            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetPropertyFloat(Object target, string property, float value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
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
