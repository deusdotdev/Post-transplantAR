using System.Collections.Generic;
using UnityEngine;

namespace LiverAR.Modules.Visuals.Runtime
{
    /// <summary>
    /// Karaciğeri kodla üretir: büyük sağ lob + küçük sol lob (iki elipsoid) birleştirilir.
    /// Dış 3B model dosyası gerektirmez. ExecuteAlways ile Editor'de de görünür.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class LiverMeshGenerator : MonoBehaviour
    {
        [Tooltip("Her lobun enlem/boylam çözünürlüğü. Yüksek = daha pürüzsüz, daha çok üçgen.")]
        [SerializeField] private int resolution = 18;

        [SerializeField] private bool regenerate;

        // Şekil güncellendiğinde artırılır; eski gömülü mesh'ler otomatik yeniden üretilir.
        private const string MeshName = "ProceduralLiver_v2";

        private MeshFilter _meshFilter;

        private void OnEnable()
        {
            EnsureMesh();
        }

        private void OnValidate()
        {
            if (regenerate)
            {
                regenerate = false;
                _meshFilter = null;
                EnsureMesh();
            }
        }

        private void EnsureMesh()
        {
            if (_meshFilter == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }

            if (_meshFilter.sharedMesh == null || _meshFilter.sharedMesh.name != MeshName)
            {
                _meshFilter.sharedMesh = BuildMesh();
            }
        }

        public Mesh BuildMesh()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();

            var stacks = Mathf.Max(8, resolution * 2);
            var slices = Mathf.Max(12, resolution * 3);

            // Tek parça gövde; küre yönü "sculpt" fonksiyonuyla karaciğer formuna deforme edilir.
            for (var lat = 0; lat <= stacks; lat++)
            {
                var theta = (float)lat / stacks * Mathf.PI;
                var sinTheta = Mathf.Sin(theta);
                var cosTheta = Mathf.Cos(theta);

                for (var lon = 0; lon <= slices; lon++)
                {
                    var phi = (float)lon / slices * Mathf.PI * 2f;
                    var dir = new Vector3(sinTheta * Mathf.Cos(phi), cosTheta, sinTheta * Mathf.Sin(phi));

                    vertices.Add(Sculpt(dir));
                    uvs.Add(new Vector2((float)lon / slices, (float)lat / stacks));
                }
            }

            var ring = slices + 1;
            for (var lat = 0; lat < stacks; lat++)
            {
                for (var lon = 0; lon < slices; lon++)
                {
                    var current = lat * ring + lon;
                    var next = current + ring;

                    triangles.Add(current);
                    triangles.Add(next + 1);
                    triangles.Add(next);

                    triangles.Add(current);
                    triangles.Add(current + 1);
                    triangles.Add(next + 1);
                }
            }

            var mesh = new Mesh { name = MeshName };
            mesh.indexFormat = vertices.Count > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Birim küre yönünü karaciğer siluetine deforme eder:
        /// genişlemesine kama formu, sola doğru incelme, üstte basıklık,
        /// önde keskin alt kenar (margo inferior) ve falciform çentik.
        /// </summary>
        private static Vector3 Sculpt(Vector3 d)
        {
            // Eksenler: x = sağ(+)/sol(-) uzunluk, y = üst(+)/alt(-), z = ön(+)/arka(-)
            const float rx = 1.2f;
            const float ry = 0.58f;
            const float rz = 0.78f;

            // Sağ lob dolgun, sol uca doğru sivrilen kama.
            var t = (d.x + 1f) * 0.5f;                 // 0 sol uç .. 1 sağ
            var taper = Mathf.Lerp(0.28f, 1f, Mathf.SmoothStep(0f, 1f, t));

            var x = d.x * rx;
            var y = d.y * ry * taper;
            var z = d.z * rz * taper;

            // Üst yüzey (diyafragmatik) basık, alt yüzey daha dolgun.
            if (d.y > 0f)
            {
                y *= Mathf.Lerp(1f, 0.72f, d.y);
            }

            // Ön-alt kenarı keskinleştir (margo inferior): önde aşağı doğru inceltme.
            if (z > 0f && y < 0f)
            {
                z *= Mathf.Lerp(1f, 0.7f, -d.y);
            }

            // Falciform çentik: ön-üst tarafta, lob ayrımını andıran ince oluk.
            var notch = Mathf.Exp(-Mathf.Pow((x + 0.05f) / 0.14f, 2f))
                        * Mathf.Clamp01(z) * Mathf.Clamp01(y * 2f);
            z -= notch * 0.22f;

            return new Vector3(x, y, z);
        }
    }
}
