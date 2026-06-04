#if UNITY_EDITOR
using LiverAR.Modules.Simulation.Runtime.Data;
using LiverAR.Modules.Visuals.Runtime;
using UnityEditor;
using UnityEngine;

namespace LiverAR.EditorTools
{
    internal static class LiverProceduralPrefab
    {
        private const string MaterialPath = "Assets/_Project/Materials/LiverMaterial.mat";

        public static GameObject BuildWrapper(float targetMeters = 0.18f)
        {
            var wrapper = new GameObject("LiverModel");
            var liver = new GameObject("Liver (Procedural)");
            liver.transform.SetParent(wrapper.transform, false);

            var filter = liver.AddComponent<MeshFilter>();
            var renderer = liver.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = LoadOrCreateMaterial();

            var generator = liver.AddComponent<LiverMeshGenerator>();
            generator.EnsureMeshNow();

            var mesh = filter.sharedMesh;
            if (mesh != null)
            {
                var bounds = mesh.bounds;
                var maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                wrapper.transform.localScale = Vector3.one * (targetMeters / Mathf.Max(0.0001f, maxDim));
            }

            var visual = wrapper.AddComponent<LiverVisualController>();
            SetField(visual, "state", LoadOrCreateState());
            SetField(visual, "liverRenderer", renderer);
            SetField(visual, "liverTransform", wrapper.transform);

            return wrapper;
        }

        private static Material LoadOrCreateMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat != null)
            {
                return mat;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader) { name = "LiverMaterial" };
            var color = new Color(0.55f, 0.16f, 0.16f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            return mat;
        }

        private static SimulationState LoadOrCreateState()
        {
            const string path = "Assets/_Project/SimulationState.asset";
            var state = AssetDatabase.LoadAssetAtPath<SimulationState>(path);
            if (state != null)
            {
                return state;
            }

            state = ScriptableObject.CreateInstance<SimulationState>();
            AssetDatabase.CreateAsset(state, path);
            AssetDatabase.SaveAssets();
            return state;
        }

        private static void SetField(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
