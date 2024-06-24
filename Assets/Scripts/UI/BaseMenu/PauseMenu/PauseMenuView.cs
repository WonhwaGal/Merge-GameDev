using GamePush;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MVC
{
    public class PauseMenuView : BaseMenuView
    {
        [SerializeField] private Button _achievementButton;
        [SerializeField] private Button _rewardsButton;
        [SerializeField] private Button _bakeryButton;
        [SerializeField] private GameObject _room;
        [Space]
        [Header("Player Status")]
        [SerializeField] private TextMeshProUGUI _statusHead;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _nextStatus;
        [SerializeField] private TextMeshProUGUI _pointsToNextStatus;

        private int _nextGoal;
        private readonly WaitForSecondsRealtime _fontDelay = new(0.1f);

        public float FinalScore { get; set; }
        public Button AchievementButton => _achievementButton;
        public Button RewardsButton => _rewardsButton;
        public Button BakeryButton => _bakeryButton;
        public GameObject Room => _room;

        public event Func<int> OnGettingProgressPoints;

        private void OnEnable() => UpdateStatusView();

        public void ShowContent()
        {
#if UNITY_EDITOR
            _bakeryButton.gameObject.SetActive(false);
#else
            if (GP_Device.IsMobile())
                _bakeryButton.gameObject.SetActive(true);
#endif
            GameEventSystem.Send(new SaveEvent(FinalScore, onlyScore: false));
        }

        public override void SetTexts(string[] texts)
        {
            RetryButton.GetComponentInChildren<TextMeshProUGUI>().text = texts[0];
            _rewardsButton.GetComponentInChildren<TextMeshProUGUI>().text = texts[1];
            _achievementButton.GetComponentInChildren<TextMeshProUGUI>().text = texts[2];
            _bakeryButton.GetComponentInChildren<TextMeshProUGUI>().text = texts[3];
        }

        public void SetStatusInfo(bool isRussian, string statusName, bool hasNext, string nextName, int nextGoal)
        {
            Debug.Log($"TestMerge: Status: updating status view");
            _statusHead.text = isRussian ? "“вой титул:" : "Your title:";
            _statusText.text = $"{statusName}";

            _nextStatus.text = isRussian ? $" до \"{nextName}\"" : $" more for \"{nextName}\"";
            _nextGoal = nextGoal;
            _nextStatus.gameObject.SetActive(hasNext);
        }

        private void UpdateStatusView()
        {
            _pointsToNextStatus.gameObject.SetActive(_nextGoal != 0);
            _nextStatus.gameObject.SetActive(_nextGoal != 0);

            var currentPoints = OnGettingProgressPoints?.Invoke();
            _pointsToNextStatus.text = $"...{_nextGoal - currentPoints}";
        }

        private void OnDestroy()
        {
            RetryButton.onClick.RemoveAllListeners();
            RewardsButton.onClick.RemoveAllListeners();
            AchievementButton.onClick.RemoveAllListeners();
            BakeryButton.onClick.RemoveAllListeners();
            OnGettingProgressPoints = null;
            OnViewDestroyed();
        }
    }
}