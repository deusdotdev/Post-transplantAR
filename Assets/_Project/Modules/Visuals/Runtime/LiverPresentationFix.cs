using UnityEngine;

namespace LiverAR.Modules.Visuals.Runtime
{
    /// <summary>
    /// GLB yanlış açıyla import edildiyse geniş yüzü kameraya çevirir (ince kenar = dikey çizgi görünümü).
    /// </summary>
    [DefaultExecutionOrder(-20)]
    public sealed class LiverPresentationFix : MonoBehaviour
    {
        private void Awake()
        {
            if (GetComponentInChildren<LiverMeshGenerator>(true) != null)
            {
                return;
            }

            var renderer = GetComponentInChildren<Renderer>(true);
            if (renderer == null)
            {
                return;
            }

            var meshRoot = renderer.transform;
            var size = GetMeshExtents(renderer);
            meshRoot.localRotation = LiverOrientation.RotationForWideFace(size);
        }

        private static Vector3 GetMeshExtents(Renderer renderer)
        {
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                return meshFilter.sharedMesh.bounds.size;
            }

            return renderer.bounds.size;
        }
    }
}
