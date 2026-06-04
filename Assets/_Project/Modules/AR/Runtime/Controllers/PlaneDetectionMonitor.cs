using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace LiverAR.Modules.AR.Runtime.Controllers
{
    /// <summary>
    /// Algılanan düzlem sayısını izler ve "yerleştirmeye hazır mı" durumunu raporlar.
    /// RAMS - Reliability: düzlem yokken yerleştirme aksiyonu pasifleştirilir,
    /// kullanıcıya aydınlatma/yüzey yönlendirmesi göstermek için sinyal üretir.
    /// </summary>
    [RequireComponent(typeof(ARPlaneManager))]
    public sealed class PlaneDetectionMonitor : MonoBehaviour
    {
        [SerializeField] private ARPlaneManager planeManager;

        [Tooltip("Yerleştirmeye izin vermek için gereken en az algılanmış düzlem sayısı.")]
        [SerializeField] private int minimumPlaneCount = 1;

        /// <summary>Düzlem uygunluğu değiştiğinde tetiklenir (true = en az bir düzlem var).</summary>
        public event Action<bool> PlaneAvailabilityChanged;

        public bool HasUsablePlane { get; private set; }

        private bool _initialized;

        private void Reset()
        {
            planeManager = GetComponent<ARPlaneManager>();
        }

        private void Awake()
        {
            if (planeManager == null)
            {
                planeManager = GetComponent<ARPlaneManager>();
            }
        }

        private void Update()
        {
            if (planeManager == null)
            {
                return;
            }

            var available = planeManager.trackables.count >= Mathf.Max(1, minimumPlaneCount);

            if (!_initialized || available != HasUsablePlane)
            {
                _initialized = true;
                HasUsablePlane = available;
                PlaneAvailabilityChanged?.Invoke(available);
            }
        }
    }
}
