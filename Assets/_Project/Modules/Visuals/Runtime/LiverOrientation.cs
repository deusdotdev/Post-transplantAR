using UnityEngine;

namespace LiverAR.Modules.Visuals.Runtime
{
    /// <summary>
    /// Karaciğer mesh'inin ince eksenini derinlik (Z) eksenine hizalar; kamera -Z'den geniş yüzü görür.
    /// </summary>
    public static class LiverOrientation
    {
        /// <summary>Geniş yüz kameraya baksın diye yerel rotasyon.</summary>
        public static Quaternion RotationForWideFace(Vector3 size)
        {
            // Sketchfab humans_liver: ince eksen genelde Z — ekstra dönüş gerekmez.
            if (size.z <= size.x && size.z <= size.y)
            {
                return Quaternion.identity;
            }

            if (size.y <= size.x && size.y <= size.z)
            {
                return Quaternion.Euler(90f, 0f, 0f);
            }

            return Quaternion.Euler(0f, 90f, 0f);
        }

        public static void CenterAndFaceCamera(Transform meshRoot)
        {
            if (meshRoot == null)
            {
                return;
            }

            var renderers = meshRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            meshRoot.localRotation = Quaternion.identity;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            var size = GetMeshExtents(renderers[0]);
            meshRoot.localRotation = RotationForWideFace(size);
            bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            meshRoot.localPosition = -bounds.center;
        }

        public static Vector3 GetMeshExtents(Renderer renderer)
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
