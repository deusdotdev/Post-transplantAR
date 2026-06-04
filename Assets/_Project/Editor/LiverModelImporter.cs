#if UNITY_EDITOR
using System.IO;
using System.Linq;
using LiverAR.Modules.AR.Runtime.Controllers;
using LiverAR.Modules.Simulation.Runtime.Data;
using LiverAR.Modules.Visuals.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LiverAR.EditorTools
{
    /// <summary>
    /// Assets/_Project/Models klasörüne bırakılan bir 3B modeli (FBX/GLB/OBJ) alır,
    /// ölçeğini AR için normalize eder, LiverVisualController ile bağlar ve
    /// LiverModel.prefab olarak kaydeder. Açık AR sahnesindeki yerleştiriciye de atar.
    /// Menü: Post-transplantAR > Import Liver Model (Models klasöründen)
    /// </summary>
    public static class LiverModelImporter
    {
        private const string ModelsFolder = "Assets/_Project/Models";
        private const string PrefabPath = "Assets/_Project/Prefabs/LiverModel.prefab";
        private const string StateAssetPath = "Assets/_Project/SimulationState.asset";
        private const float TargetMeters = 0.18f;

        [MenuItem("Post-transplantAR/Import Liver Model (Models klasöründen)")]
        public static void ImportModel()
        {
            EnsureFolder(ModelsFolder);

            var source = FindModelAsset();
            if (source == null)
            {
                EditorUtility.DisplayDialog("Model bulunamadı",
                    $"İndirdiğin modeli (.fbx / .glb / .obj) şu klasöre bırak:\n\n{ModelsFolder}\n\n" +
                    "Sonra bu menüyü tekrar çalıştır.\n\n" +
                    "Not: .glb kullanıyorsan glTFast paketi gerekir (manifest'e eklendi).", "Tamam");
                return;
            }

            var wrapper = BuildLiverObject(source);
            EnsureFolder(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(wrapper, PrefabPath);
            Object.DestroyImmediate(wrapper);

            TryAssignToOpenScene(prefab);

            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LiverModelImporter] Model bağlandı: {AssetDatabase.GetAssetPath(source)} " +
                      $"-> {PrefabPath}. AR sahnesini yeniden kurarsan otomatik kullanılır.");
        }

        private static GameObject FindModelAsset()
        {
            var guids = AssetDatabase.FindAssets("t:GameObject", new[] { ModelsFolder });
            // İlk geçerli model asset'ini seç.
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".fbx" || ext == ".glb" || ext == ".gltf" || ext == ".obj")
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }

            // Uzantı eşleşmese bile (gltFast bazen alt-asset üretir) ilk GameObject'i dene.
            return guids.Length > 0
                ? AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]))
                : null;
        }

        private static GameObject BuildLiverObject(GameObject source)
        {
            var wrapper = new GameObject("LiverModel");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.transform.SetParent(wrapper.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var renderers = instance.GetComponentsInChildren<Renderer>();
            Renderer mainRenderer = null;

            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                var largest = renderers[0].bounds.size.sqrMagnitude;
                mainRenderer = renderers[0];

                foreach (var r in renderers)
                {
                    bounds.Encapsulate(r.bounds);
                    var size = r.bounds.size.sqrMagnitude;
                    if (size > largest)
                    {
                        largest = size;
                        mainRenderer = r;
                    }
                }

                // Modeli merkeze al (wrapper henüz ölçek 1 olduğundan dünya = yerel).
                instance.transform.localPosition = -bounds.center;

                // En büyük boyutu hedef metreye ölçekle.
                var maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                wrapper.transform.localScale = Vector3.one * (TargetMeters / Mathf.Max(0.0001f, maxDim));
            }

            var visual = wrapper.AddComponent<LiverVisualController>();
            SetField(visual, "state", LoadOrCreateState());
            SetField(visual, "liverTransform", wrapper.transform);
            if (mainRenderer != null)
            {
                SetField(visual, "liverRenderer", mainRenderer);
            }

            return wrapper;
        }

        private static void TryAssignToOpenScene(GameObject prefab)
        {
            var placement = Object.FindObjectOfType<ARPlacementController>();
            if (placement == null)
            {
                return;
            }

            SetField(placement, "liverPrefab", prefab);
            EditorSceneManager.MarkSceneDirty(placement.gameObject.scene);
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

        private static void EnsureFolder(string dir)
        {
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
        }

        private static void SetField(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogWarning($"[LiverModelImporter] '{field}' alanı bulunamadı: {target.GetType().Name}");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
