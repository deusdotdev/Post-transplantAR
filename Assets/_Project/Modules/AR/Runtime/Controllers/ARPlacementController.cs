using System;
using System.Collections.Generic;
using LiverAR.Modules.Visuals.Runtime;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace LiverAR.Modules.AR.Runtime.Controllers
{
    /// <summary>
    /// Dokunulan ekran noktasından AR düzlemine karaciğer modelini yerleştirir/yeniden konumlandırır.
    /// </summary>
    public sealed class ARPlacementController : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private GameObject liverPrefab;

        [Tooltip("Düzlem algılanana kadar yerleştirme kapalı tutulur (RAMS - Reliability).")]
        [SerializeField] private bool requirePlaneBeforePlacement = true;

        private static readonly List<ARRaycastHit> Hits = new();
        private GameObject _spawnedObject;
        private bool _placementEnabled;

        private void Start()
        {
            if (liverPrefab == null)
            {
                Debug.LogError("[ARPlacement] liverPrefab boş — Post-transplantAR > Import Liver Model veya Build AR Scene çalıştır.");
            }
        }

        /// <summary>Model ilk kez yerleştirildiğinde tetiklenir.</summary>
        public event Action<GameObject> ModelPlaced;

        /// <summary>Mevcut model yeniden konumlandırıldığında tetiklenir.</summary>
        public event Action<GameObject> ModelMoved;

        public bool HasModel => _spawnedObject != null;
        public GameObject SpawnedObject => _spawnedObject;

        /// <summary>Düzlem uygunluğuna göre yerleştirmeyi açıp kapatır.</summary>
        public void SetPlacementEnabled(bool value)
        {
            _placementEnabled = value;
        }

        public bool TryPlaceFromScreenTap(Vector2 screenPosition, bool allowFallbackInFrontOfCamera = false)
        {
            if (requirePlaneBeforePlacement && !_placementEnabled && !allowFallbackInFrontOfCamera)
            {
                return false;
            }

            if (liverPrefab == null)
            {
                Debug.LogWarning("[ARPlacement] Yerleştirme iptal: liverPrefab atanmadı.");
                return false;
            }

            Pose pose;
            if (raycastManager != null &&
                raycastManager.Raycast(screenPosition, Hits, TrackableType.PlaneWithinPolygon))
            {
                pose = Hits[0].pose;
            }
            else if (allowFallbackInFrontOfCamera && TryGetPoseInFrontOfCamera(out pose))
            {
                // Düzlem henüz yoksa bile demo için kameranın önüne yerleştir.
            }
            else
            {
                return false;
            }

            ApplyPose(pose);
            return true;
        }

        public bool TryPlaceAtViewportCenter(bool allowFallbackInFrontOfCamera = true)
        {
            var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.55f);
            return TryPlaceFromScreenTap(center, allowFallbackInFrontOfCamera);
        }

        private void ApplyPose(Pose pose)
        {
            if (_spawnedObject == null)
            {
                _spawnedObject = Instantiate(liverPrefab, pose.position, pose.rotation);
                LiverRenderBootstrap.EnsureVisible(_spawnedObject);
                Debug.Log($"[ARPlacement] Model yerleştirildi: {_spawnedObject.transform.position}");
                ModelPlaced?.Invoke(_spawnedObject);
            }
            else
            {
                _spawnedObject.transform.SetPositionAndRotation(pose.position, pose.rotation);
                ModelMoved?.Invoke(_spawnedObject);
            }
        }

        private static bool TryGetPoseInFrontOfCamera(out Pose pose)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                pose = default;
                Debug.LogWarning("[ARPlacement] Camera.main yok; fallback yerleştirme başarısız.");
                return false;
            }

            var forward = cam.transform.forward;
            var position = cam.transform.position + forward * 0.45f;
            var rotation = Quaternion.LookRotation(-forward, Vector3.up);
            pose = new Pose(position, rotation);
            return true;
        }
    }
}
