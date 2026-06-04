using System;
using System.Collections.Generic;
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

        public bool TryPlaceFromScreenTap(Vector2 screenPosition)
        {
            if (requirePlaneBeforePlacement && !_placementEnabled)
            {
                return false;
            }

            if (raycastManager == null || liverPrefab == null)
            {
                return false;
            }

            if (!raycastManager.Raycast(screenPosition, Hits, TrackableType.PlaneWithinPolygon))
            {
                return false;
            }

            var hitPose = Hits[0].pose;

            if (_spawnedObject == null)
            {
                _spawnedObject = Instantiate(liverPrefab, hitPose.position, hitPose.rotation);
                ModelPlaced?.Invoke(_spawnedObject);
            }
            else
            {
                _spawnedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                ModelMoved?.Invoke(_spawnedObject);
            }

            return true;
        }
    }
}
