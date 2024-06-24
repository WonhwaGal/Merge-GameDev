using Code.Achievements;
using GamePush;
using System;
using UnityEngine.Localization.Tables;
using static StatusSO;

namespace Code.MVC
{
    public class PauseMenuModel : BaseMenuModel
    {
        private AchievementService _achievementService;

        public PauseMenuModel() : base() { }

        public event Action<bool, string, bool, string, int> OnCurrentStatus;

        public void Init()
        {
            UpdateTextAsync();
            _achievementService = ServiceLocator.Container.RequestFor<AchievementService>();
            _achievementService.OnSetStatusAchiev += CheckPlayerStatus;
        }

        public void OpenAchievements() => _achievementService.Open();
        public int UpdateStatus() => _achievementService.ProgressPoints();

        protected override string[] GetEntries(StringTable table)
        {
            return new string[4] {table.GetEntry("retryB")?.GetLocalizedString(),
                            table.GetEntry("optionsB")?.GetLocalizedString(),
                            table.GetEntry("achievsB")?.GetLocalizedString(),
                            table.GetEntry("roomB")?.GetLocalizedString()};
        }

        private void CheckPlayerStatus(PlayerStatus status)
        {
            var hasNext = false;
            var isRussian = GP_Language.Current() == Language.Russian;
            var name = isRussian ? status.RussianName : status.EnglishName;
            string nextName = String.Empty;
            int nextGoal = 0;

            if (_achievementService.TryGetNextStatus(status, out PlayerStatus nextStatus))
            {
                hasNext = true;
                nextName = isRussian ? nextStatus.RussianName : nextStatus.EnglishName;
                nextGoal = nextStatus.Goal;
            }

            OnCurrentStatus?.Invoke(isRussian, name, hasNext, nextName, nextGoal);
        }
    }
}