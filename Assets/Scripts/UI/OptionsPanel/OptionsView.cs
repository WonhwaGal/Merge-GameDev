using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MVC
{
    public class OptionsView : MonoBehaviour, IView
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _backButtonText;
        [SerializeField] private TextMeshProUGUI _roomBackButttonText;
        [SerializeField] private RewardOptionView[] _rewardButtons;

        public RewardOptionView[] RewardButtons => _rewardButtons;

        public event Action OnDestroyView;

        private void Start() => _backButton.onClick.AddListener(() => gameObject.SetActive(false));

        public void SetTexts(string[] texts)
        {
            _backButtonText.text = texts[0];
            if (_roomBackButttonText)
                _roomBackButttonText.text = texts[0];
        }

        public void UpdateOption(int index, bool isActive)
        {
            if (index < _rewardButtons.Length)
                RewardButtons[index].UpdateState(isActive);
            GameEventSystem.Send(new BackgroundEvent(index, isActive));
        }

        private void OnDestroy()
        {
            OnDestroyView?.Invoke();
            OnDestroyView = null;
            _backButton.onClick.RemoveAllListeners();
        }
    }
}