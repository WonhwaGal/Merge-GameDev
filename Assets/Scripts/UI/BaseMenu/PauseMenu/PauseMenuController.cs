using GamePush;
using System;

namespace Code.MVC
{
    public class PauseMenuController : BaseMenuController<PauseMenuView, PauseMenuModel>
    {
        public PauseMenuController() : base() => _triggerAction = GameAction.Pause;

        public event Action OnRequestRewards;

        protected override void Show()
        {
            View.FinalScore = OnRequestScore.Invoke();
            View.gameObject.SetActive(true);
            View.ShowContent();
        }

        protected override void InitComponents()
        {
            View.RewardsButton.onClick.AddListener(() => OnRequestRewards?.Invoke());
            View.AchievementButton.onClick.AddListener(Model.OpenAchievements);
            SetUpBakeryPanel();
            Model.Init();
            Model.OnCurrentStatus += View.SetStatusInfo;
            View.OnGettingProgressPoints += Model.UpdateStatus;
            Model.UpdateStatus();
        }

        private void SetUpBakeryPanel()
        {
#if UNITY_EDITOR
            View.BakeryButton.gameObject.SetActive(false);
#else
            if (GP_Device.IsMobile())
            {
                View.Room.SetActive(false);
                View.BakeryButton.onClick.AddListener(() => View.Room.SetActive(true));
            }
            else
            {
                View.BakeryButton.gameObject.SetActive(false);
            }
#endif
        }

        protected override void OnDispose()
        {
            OnRequestRewards = null;
            Model.OnCurrentStatus -= View.SetStatusInfo;
            View.OnGettingProgressPoints -= Model.UpdateStatus;
        }
    }
}