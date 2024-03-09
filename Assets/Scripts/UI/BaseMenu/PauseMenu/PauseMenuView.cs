using GamePush;
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

        public float FinalScore { get; set; }
        public Button AchievementButton => _achievementButton;
        public Button RewardsButton => _rewardsButton;
        public Button BakeryButton => _bakeryButton;
        public GameObject Room => _room;

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

        private void OnDestroy()
        {
            RetryButton.onClick.RemoveAllListeners();
            RewardsButton.onClick.RemoveAllListeners();
            AchievementButton.onClick.RemoveAllListeners();
            BakeryButton.onClick.RemoveAllListeners();
            OnViewDestroyed();
        }
    }
}