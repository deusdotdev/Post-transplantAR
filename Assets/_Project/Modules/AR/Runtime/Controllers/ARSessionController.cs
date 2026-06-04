using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace LiverAR.Modules.AR.Runtime.Controllers
{
    /// <summary>
    /// AR oturumunun yaşam döngüsünü ve cihaz uyumluluğunu yönetir.
    /// RAMS - Availability: desteklenmeyen cihazda akışı durdurup bilgilendirici fallback tetikler.
    /// </summary>
    public sealed class ARSessionController : MonoBehaviour
    {
        public enum SessionStatus
        {
            CheckingAvailability,
            Unsupported,
            NeedsInstall,
            Installing,
            Initializing,
            Ready
        }

        [SerializeField] private ARSession arSession;

        /// <summary>Kullanıcıya gösterilecek üst seviye durum değiştiğinde tetiklenir.</summary>
        public event Action<SessionStatus> StatusChanged;

        public SessionStatus CurrentStatus { get; private set; } = SessionStatus.CheckingAvailability;
        public bool IsReady => CurrentStatus == SessionStatus.Ready;

        private void Reset()
        {
            arSession = FindObjectOfType<ARSession>();
        }

        private void OnEnable()
        {
            ARSession.stateChanged += OnSessionStateChanged;
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= OnSessionStateChanged;
        }

        private void Start()
        {
            StartCoroutine(InitializeRoutine());
        }

        private IEnumerator InitializeRoutine()
        {
            SetStatus(SessionStatus.CheckingAvailability);

            if ((ARSession.state == ARSessionState.None) ||
                (ARSession.state == ARSessionState.CheckingAvailability))
            {
                yield return ARSession.CheckAvailability();
            }

            if (ARSession.state == ARSessionState.Unsupported)
            {
                SetStatus(SessionStatus.Unsupported);
                yield break;
            }

            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                SetStatus(SessionStatus.NeedsInstall);
                yield return ARSession.Install();

                if (ARSession.state == ARSessionState.Unsupported)
                {
                    SetStatus(SessionStatus.Unsupported);
                    yield break;
                }
            }

            if (arSession != null)
            {
                arSession.enabled = true;
            }

            SyncStatusFromState(ARSession.state);
        }

        private void OnSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            SyncStatusFromState(args.state);
        }

        private void SyncStatusFromState(ARSessionState state)
        {
            switch (state)
            {
                case ARSessionState.Unsupported:
                    SetStatus(SessionStatus.Unsupported);
                    break;
                case ARSessionState.NeedsInstall:
                    SetStatus(SessionStatus.NeedsInstall);
                    break;
                case ARSessionState.Installing:
                    SetStatus(SessionStatus.Installing);
                    break;
                case ARSessionState.SessionInitializing:
                    SetStatus(SessionStatus.Initializing);
                    break;
                case ARSessionState.SessionTracking:
                    SetStatus(SessionStatus.Ready);
                    break;
                case ARSessionState.Ready:
                    SetStatus(SessionStatus.Initializing);
                    break;
            }
        }

        private void SetStatus(SessionStatus status)
        {
            if (CurrentStatus == status)
            {
                return;
            }

            CurrentStatus = status;
            StatusChanged?.Invoke(status);
        }
    }
}
