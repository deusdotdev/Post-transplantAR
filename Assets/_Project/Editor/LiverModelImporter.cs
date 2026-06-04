#if UNITY_EDITOR
using System.IO;
using LiverAR.Modules.AR.Runtime.Controllers;
using LiverAR.Modules.Simulation.Runtime.Data;
using LiverAR.Modules.Visuals.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LiverAR.EditorTools
{
    /// <summary>
    /// Models klasöründeki FBX/GLB/OBJ dosyasını AR prefab'ına dönüştürür.
    /// GLB için com.unity.cloud.gltfast gerekir; import başarısızsa procedural karaciğer kullanılır.
    /// </summary>
    public static class LiverModelImporter
    {
        private const string ModelsFolder = "Assets/_Project/Models";
        private const string PrefabPath = "Assets/_Project/Prefabs/LiverModel.prefab";
        private const string StateAssetPath = "Assets/_Project/SimulationState.asset";
        private const float TargetMeters = 0.32f;
        private const string GltfastImporterGuid = "715df9372183c47e389bb6e19fbc3b52";
        private const string HumansLiverGlb = ModelsFolder + "/humans_liver.glb";

        /// <summary>GLB meta + reimport + prefab + önizleme sahnesi — tek menü.</summary>
        [MenuItem("Post-transplantAR/Karaciğeri düzelt ve önizlemeyi yenile")]
        public static void FixLiverAndRefreshPreview()
        {
            EnsureGltfastGlbMeta();
            ReimportModels();
            ImportModel();
            SimulationPreviewBuilder.BuildPreview();
        }

        [MenuItem("Post-transplantAR/Import Liver Model (Models klasöründen)")]
        public static void ImportModel()
        {
            EnsureFolder(ModelsFolder);

            var sourcePath = FindModelPath();
            GameObject wrapper;

            if (string.IsNullOrEmpty(sourcePath))
            {
                EditorUtility.DisplayDialog("Model bulunamadı",
                    $"İndirdiğin modeli (.fbx önerilir, .glb de olur) şu klasöre bırak:\n\n{ModelsFolder}",
                    "Tamam");
                return;
            }

            ForceReimportIfNeeded(sourcePath);

            if (!TryBuildFromAsset(sourcePath, out wrapper))
            {
                var ext = Path.GetExtension(sourcePath).ToLowerInvariant();
                var glbHint = ext is ".glb" or ".gltf"
                    ? "\n\nNeden: Unity .glb dosyasını tek başına açamaz; glTFast paketi gerekir.\n" +
                      "Kontrol: Window > Package Manager > glTFast yüklü mü?\n" +
                      "Sonra: Post-transplantAR > Reimport Models Folder\n" +
                      "humans_liver.glb.meta içinde DefaultImporter yazıyorsa glTFast henüz devreye girmemiş.\n\n" +
                      "Alternatif: modeli .fbx olarak kaydet veya procedural kullan."
                    : string.Empty;

                var useProcedural = EditorUtility.DisplayDialog(
                    "3B import başarısız",
                    $"Dosya okundu ama mesh bulunamadı: {sourcePath}{glbHint}\n\n" +
                    "Procedural (kodla üretilmiş) karaciğer kullanılsın mı? AR'de yine çalışır.",
                    "Procedural kullan",
                    "İptal");

                if (!useProcedural)
                {
                    return;
                }

                wrapper = LiverProceduralPrefab.BuildWrapper(TargetMeters);
            }

            EnsureFolder(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(wrapper, PrefabPath);
            Object.DestroyImmediate(wrapper);

            TryAssignToOpenScene(prefab);

            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var isProcedural = prefab.GetComponentInChildren<LiverMeshGenerator>(true) != null;
            var meshLabel = isProcedural
                ? "Procedural (kodla üretilmiş — indirdiğin GLB değil)"
                : $"3B model: {Path.GetFileName(sourcePath)} (Sketchfab dokuları korundu)";

            EditorUtility.DisplayDialog("Karaciğer prefab hazır",
                $"{meshLabel}\n\nKaydedildi:\n{PrefabPath}\n\nSonra: Post-transplantAR > Build AR Scene",
                "Tamam");
            Debug.Log($"[LiverModelImporter] Prefab güncellendi ({meshLabel}): {PrefabPath}");
        }

        private static void EnsureGltfastGlbMeta()
        {
            var metaPath = HumansLiverGlb + ".meta";
            if (!File.Exists(metaPath))
            {
                return;
            }

            var text = File.ReadAllText(metaPath);
            if (text.Contains("ScriptedImporter:") && text.Contains(GltfastImporterGuid))
            {
                return;
            }

            var yaml =
                "fileFormatVersion: 2\n" +
                "guid: f5d7572638e214989aefbddbb0a68be9\n" +
                "ScriptedImporter:\n" +
                "  internalIDToNameTable: []\n" +
                "  externalObjects: {}\n" +
                "  serializedVersion: 2\n" +
                $"  script: {{fileID: 11500000, guid: {GltfastImporterGuid}, type: 3}}\n" +
                "  userData: \n" +
                "  assetBundleName: \n" +
                "  assetBundleVariant: \n";

            File.WriteAllText(metaPath, yaml);
            AssetDatabase.Refresh();
            Debug.Log("[LiverModelImporter] humans_liver.glb.meta → glTFast (GltfImporter) olarak ayarlandı.");
        }

        [MenuItem("Post-transplantAR/Reimport Models Folder (GLB/FBX)")]
        public static void ReimportModels()
        {
            EnsureGltfastGlbMeta();

            if (!Directory.Exists(ModelsFolder))
            {
                EditorUtility.DisplayDialog("Klasör yok", ModelsFolder, "Tamam");
                return;
            }

            var count = 0;
            foreach (var file in Directory.GetFiles(ModelsFolder))
            {
                var path = file.Replace('\\', '/');
                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext is ".fbx" or ".glb" or ".gltf" or ".obj")
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    count++;
                }
            }

            AssetDatabase.Refresh();
            var glbPath = $"{ModelsFolder}/humans_liver.glb";
            var importerName = AssetImporter.GetAtPath(glbPath)?.GetType().Name ?? "yok";
            EditorUtility.DisplayDialog("Reimport bitti",
                $"{count} dosya yenilendi.\n\nhumans_liver.glb importer: {importerName}\n\n" +
                (importerName == "DefaultImporter"
                    ? "Hâlâ DefaultImporter → Package Manager'da glTFast yok/yüklenmedi."
                    : "glTFast aktif görünüyor → Import Liver Model çalıştır."),
                "Tamam");
            Debug.Log($"[LiverModelImporter] Reimport: {count} dosya. GLB importer: {importerName}");
        }

        private static void ForceReimportIfNeeded(string assetPath)
        {
            var ext = Path.GetExtension(assetPath).ToLowerInvariant();
            if (ext is not ".glb" and not ".gltf")
            {
                return;
            }

            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null || importer.GetType().Name != "DefaultImporter")
            {
                return;
            }

            Debug.Log($"[LiverModelImporter] GLB için zorunlu reimport: {assetPath}");
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }

        private static string FindModelPath()
        {
            if (!Directory.Exists(ModelsFolder))
            {
                return null;
            }

            var preferred = $"{ModelsFolder}/humans_liver.glb";
            if (File.Exists(preferred))
            {
                return preferred;
            }

            string best = null;
            var bestTime = 0L;

            foreach (var file in Directory.GetFiles(ModelsFolder))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext is not (".fbx" or ".glb" or ".gltf" or ".obj"))
                {
                    continue;
                }

                var t = File.GetLastWriteTimeUtc(file).Ticks;
                if (t >= bestTime)
                {
                    bestTime = t;
                    best = file.Replace('\\', '/');
                }
            }

            return best;
        }

        private static bool TryBuildFromAsset(string assetPath, out GameObject wrapper)
        {
            wrapper = null;

            if (!IsGlbImportReady(assetPath))
            {
                return false;
            }

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (source == null)
            {
                Debug.LogWarning($"[LiverModelImporter] GameObject yüklenemedi: {assetPath}");
                return false;
            }

            wrapper = BuildLiverObject(source);
            var renderers = wrapper.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Object.DestroyImmediate(wrapper);
                wrapper = null;
                Debug.LogWarning($"[LiverModelImporter] Mesh/renderer yok: {assetPath}");
                return false;
            }

            return true;
        }

        private static bool IsGlbImportReady(string assetPath)
        {
            var ext = Path.GetExtension(assetPath).ToLowerInvariant();
            if (ext is not ".glb" and not ".gltf")
            {
                return true;
            }

            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                return false;
            }

            if (importer.GetType().Name == "DefaultImporter")
            {
                Debug.LogWarning(
                    $"[LiverModelImporter] {assetPath} glTFast ile import edilmemiş. " +
                    "Package Manager bitince: Post-transplantAR > Reimport Models Folder.");
                return false;
            }

            return true;
        }

        private static GameObject BuildLiverObject(GameObject source)
        {
            var wrapper = new GameObject("LiverModel");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.transform.SetParent(wrapper.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            Renderer mainRenderer = null;

            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                var largest = bounds.size.sqrMagnitude;
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

                var meshSize = LiverOrientation.GetMeshExtents(mainRenderer);
                instance.transform.localRotation = LiverOrientation.RotationForWideFace(meshSize);
                bounds = renderers[0].bounds;
                foreach (var r in renderers)
                {
                    bounds.Encapsulate(r.bounds);
                }

                instance.transform.localPosition = -bounds.center;
                var maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                wrapper.transform.localScale = Vector3.one * (TargetMeters / Mathf.Max(0.0001f, maxDim));
            }

            FixImportMaterials(instance);

            wrapper.AddComponent<LiverPresentationFix>();
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

        /// <summary>GLB/FBX içe aktarım materyallerini korur; yalnızca bozuk shader'ları düzeltir.</summary>
        private static void FixImportMaterials(GameObject modelRoot)
        {
            LiverRenderBootstrap.EnsureVisible(modelRoot);
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
