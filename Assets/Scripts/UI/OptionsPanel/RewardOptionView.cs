using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Code.MVC
{
    public class RewardOptionView : MonoBehaviour
    {
        [SerializeField] private Image _rewardImage;
        [SerializeField] private Image _lockedImage;
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private GameObject _adImage;
        [SerializeField] private int _index;
        private TextMeshProUGUI _applyText;
        private TextMeshProUGUI _cancelText;

        public event Action<int, bool> OnChangeState;

        private void Awake()
        {
            _lockedImage.gameObject.SetActive(true);
            _rewardImage.gameObject.SetActive(false);
            _applyButton.gameObject.SetActive(false);
            _cancelButton.gameObject.SetActive(false);
            _applyButton.onClick.AddListener(() => ButtonClicked(true));
            _cancelButton.onClick.AddListener(() => ButtonClicked(false));
            _applyText = _applyButton.GetComponentInChildren<TextMeshProUGUI>();
            _cancelText = _cancelButton.GetComponentInChildren<TextMeshProUGUI>();
            GameEventSystem.Subscribe<LoadADEvent>(BlockButton);
            UpdateTextAsync();
        }

        private void ButtonClicked(bool apply)
        {
            if (apply)
            {
                _applyButton.interactable = false;
                StartCoroutine(StartLoading());
            }
            OnChangeState?.Invoke(_index, apply);
        }

        public void BlockButton(LoadADEvent @event)
        {
            _applyButton.interactable = !@event.StartLoading;
            _cancelButton.interactable = !@event.StartLoading;
        }

        public void UpdateState(bool isActive) => UpdateView(isActive);

        private void UpdateView(bool toApply)
        {
            _rewardImage.gameObject.SetActive(true);
            _applyButton.gameObject.SetActive(!toApply);
            _cancelButton.gameObject.SetActive(toApply);
            _lockedImage.gameObject.SetActive(false);
            _applyButton.interactable = true;
            StopCoroutine(StartLoading());
        }

        private void UpdateTextAsync() =>
            LocalizationSettings.StringDatabase.GetTableAsync("TextTable").Completed +=
            handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var table = handle.Result;
                    _applyText.text = table.GetEntry("rewardApply")?.GetLocalizedString();
                    _cancelText.text = table.GetEntry("rewardRemove")?.GetLocalizedString();
                }
            };

        private IEnumerator StartLoading()
        {
            GameEventSystem.Send(new LoadADEvent(true));

            yield return new WaitForSecondsRealtime(Constants.LoadAdWaitTime);

            GameEventSystem.Send(new LoadADEvent(false));
        }

        private void OnDisable() => StopCoroutine(StartLoading());

        private void OnDestroy()
        {
            _applyButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.RemoveAllListeners();
            OnChangeState = null;
            GameEventSystem.UnSubscribe<LoadADEvent>(BlockButton);
        }
    }
}