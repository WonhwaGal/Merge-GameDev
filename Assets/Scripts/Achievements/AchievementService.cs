using System;
using static AchievSO.AchievBlock;
using GamePush;


namespace Code.Achievements
{
    public sealed class AchievementService : IService, IDisposable
    {
        private readonly AchievSO _achievSO;
        private bool _playingNewGame;
        private int _savedScore;

        public AchievementService(AchievSO so) => _achievSO = so;

        public event Action<Achievement> OnUnlockAchiev;

        public void Open() => GP_Achievements.Open();

        public void CheckAchievement(AchievType type, float referenceValue)
        {
            var achievBlock = _achievSO.FindAchiev(type);
            if (achievBlock == null)
                return;

            for (int i = 0; i < achievBlock.Achievements.Count; i++)
            {
                var achiev = achievBlock.Achievements[i];
                if (achiev.IsUnlocked)
                    continue;

                CheckAchievement(achiev, type == AchievType.MergeByRank, referenceValue);
            }
            UnityEngine.Debug.LogWarning($"TestMerge: _achievService done");
        }

        public void SetInitialProgress(bool toZero)
        {
            _playingNewGame = toZero;
            _savedScore = GP_Player.GetInt(Constants.SavedScore);
            for (int i = 0; i < _achievSO.AchievsByType.Count; i++)
            {
                var achievList = _achievSO.AchievsByType[i].Achievements;
                for (int ach = 0; ach < achievList.Count; ach++)
                {
                    var achiev = _achievSO.AchievsByType[i].Achievements[ach];
                    if (achiev.IsUnlocked)
                        continue;

                    var id = achiev.AchievID.ToString();
                    if (toZero && !achiev.IsTotal && achiev.HasProgress)
                        GP_Achievements.SetProgress(id, 0);
                    else if (achiev.IsTotal)
                        achiev.SavedProgress = GP_Achievements.GetProgress(id);
                }
            }
        }

        private void CheckAchievement(Achievement achiev, bool isMerge, float referenceValue)
        {
            SetProgress(isMerge, achiev, referenceValue);

            if (isMerge && referenceValue == achiev.ReferenceValue)
                HandleMergeAchiev(achiev);
            else if (!isMerge && referenceValue >= achiev.ReferenceValue)
                UnlockAchievement(achiev);
        }

        private void SetProgress(bool isMergeByRank, Achievement achiev, float referenceValue)
        {
            if (isMergeByRank || !achiev.HasProgress)
                return;

            var progress = (int)referenceValue;
            if (achiev.IsTotal)
            {
                progress += achiev.SavedProgress;
                progress = _playingNewGame ? progress : progress - _savedScore;
            }
            GP_Achievements.SetProgress(achiev.AchievID.ToString(), progress);
        }

        private void HandleMergeAchiev(Achievement achiev)
        {
            var progressValue = ++achiev.SavedProgress;
            GP_Achievements.SetProgress(achiev.AchievID.ToString(), progressValue);
            if (progressValue >= achiev.Condition)
                UnlockAchievement(achiev);
        }

        private void UnlockAchievement(Achievement achiev)
        {
            var id = achiev.AchievID.ToString();
            if(id != null)
            {
                GP_Achievements.Unlock(id);
                achiev.IsUnlocked = true;
                OnUnlockAchiev?.Invoke(achiev);
            }
        }

        public void Dispose() => OnUnlockAchiev = null;
    }
}