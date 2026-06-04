using UnityEngine;
using UnityEngine.Rendering;

namespace LiverAR.Modules.Visuals.Runtime
{
    /// <summary>
    /// GLB/FBX içe aktarımında eksik veya uyumsuz materyalleri düzeltir (özellikle mobil build).
    /// </summary>
    public static class LiverRenderBootstrap
    {
        private static readonly Color DefaultLiverColor = new Color(0.62f, 0.22f, 0.18f, 1f);

        public static void EnsureVisible(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            var shader = ResolveLitShader();
            if (shader == null)
            {
                Debug.LogWarning("[LiverRenderBootstrap] Uygun shader bulunamadı.");
                return;
            }

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = true;
                var shared = renderer.sharedMaterials;
                if (shared == null || shared.Length == 0)
                {
                    shared = new[] { CreateMaterial(shader) };
                }
                else
                {
                    for (var i = 0; i < shared.Length; i++)
                    {
                        if (NeedsReplacement(shared[i]))
                        {
                            shared[i] = CreateMaterial(shader);
                        }
                    }
                }

                renderer.sharedMaterials = shared;
            }
        }

        private static Shader ResolveLitShader()
        {
            var useUrp = GraphicsSettings.currentRenderPipeline != null;
            if (useUrp)
            {
                return Shader.Find("Universal Render Pipeline/Lit")
                       ?? Shader.Find("Universal Render Pipeline/Simple Lit");
            }

            return Shader.Find("Standard")
                   ?? Shader.Find("Legacy Shaders/Diffuse")
                   ?? Shader.Find("Diffuse");
        }

        private static bool NeedsReplacement(Material mat)
        {
            if (mat == null || mat.shader == null)
            {
                return true;
            }

            if (!mat.shader.isSupported)
            {
                return true;
            }

            var name = mat.shader.name;
            var onBuiltIn = GraphicsSettings.currentRenderPipeline == null;
            if (onBuiltIn && name.Contains("Universal Render Pipeline"))
            {
                return true;
            }

            return false;
        }

        private static Material CreateMaterial(Shader shader)
        {
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", DefaultLiverColor);
            }

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", DefaultLiverColor);
            }

            return mat;
        }
    }
}
