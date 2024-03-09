using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePush;

namespace Code.MVC
{
    public class StartCanvas : MonoBehaviour, IView
    {
        public Button StartNewButton;
        public Button ContinueButton;
        public GameObject[] _desktopObjects;
        public GameObject[] _mobileObjects;
        private TextMeshProUGUI _startText;
        private TextMeshProUGUI _continueText;
        private bool _isMobile;
        public event Action OnDestroyView;

        private void Start()
        {
            _startText = StartNewButton.GetComponentInChildren<TextMeshProUGUI>();
            _continueText = ContinueButton.GetComponentInChildren<TextMeshProUGUI>();
#if UNITY_EDITOR
            _isMobile = false;
#else
            _isMobile = GP_Device.IsMobile();
#endif
            SetView();
        }

        public void SetTexts(string[] texts)
        {
            _startText.text = texts[0];
            _continueText.text = texts[1];
        }

        private void SetView()
        {
            for (int i = 0; i < _desktopObjects.Length; i++)
                _desktopObjects[i].SetActive(!_isMobile);
            for (int i = 0; i < _mobileObjects.Length; i++)
                _mobileObjects[i].SetActive(_isMobile);
        }

        private void OnDestroy()
        {
            OnDestroyView?.Invoke();
            OnDestroyView = null;
            StartNewButton.onClick.RemoveAllListeners();
            ContinueButton.onClick.RemoveAllListeners();
        }
    }
}