using System;
using UnityEngine;

namespace LiverAR.Modules.UI.Runtime.Screens
{
    /// <summary>Güvenlik uyarısı → anatomi girişi → eğitim paneli akışı.</summary>
    [DefaultExecutionOrder(-50)]
    public sealed class EducationFlowController : MonoBehaviour
    {
        [SerializeField] private SafetyDisclaimerScreen disclaimerScreen;
        [SerializeField] private AnatomyIntroScreen anatomyIntro;
        [SerializeField] private GameObject educationPanelRoot;

        private bool _disclaimerDone;
        private bool _introDone;

        public bool IsFlowComplete => _disclaimerDone && _introDone;

        public event Action FlowComplete;

        private void OnEnable()
        {
            if (disclaimerScreen != null)
            {
                disclaimerScreen.Acknowledged += OnDisclaimerAcknowledged;
            }

            if (anatomyIntro != null)
            {
                anatomyIntro.Continued += OnIntroContinued;
            }
        }

        private void OnDisable()
        {
            if (disclaimerScreen != null)
            {
                disclaimerScreen.Acknowledged -= OnDisclaimerAcknowledged;
            }

            if (anatomyIntro != null)
            {
                anatomyIntro.Continued -= OnIntroContinued;
            }
        }

        private void Start()
        {
            if (educationPanelRoot != null)
            {
                educationPanelRoot.SetActive(_introDone && _disclaimerDone);
            }

            if (IsFlowComplete)
            {
                FlowComplete?.Invoke();
            }
        }

        private void OnDisclaimerAcknowledged()
        {
            _disclaimerDone = true;
            if (anatomyIntro != null && anatomyIntro.gameObject.activeInHierarchy)
            {
                anatomyIntro.Show();
            }
            else
            {
                CompleteIntro();
            }
        }

        private void OnIntroContinued()
        {
            CompleteIntro();
        }

        private void CompleteIntro()
        {
            _introDone = true;
            if (educationPanelRoot != null)
            {
                educationPanelRoot.SetActive(true);
            }

            FlowComplete?.Invoke();
        }

        public void SetEducationPanelVisible(bool visible)
        {
            if (educationPanelRoot != null && IsFlowComplete)
            {
                educationPanelRoot.SetActive(visible);
            }
        }
    }
}
