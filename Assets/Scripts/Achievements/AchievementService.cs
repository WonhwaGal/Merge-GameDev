using System;
using static AchievSO.AchievBlock;
using GamePush;
using static StatusSO;


namespace Code.Achievements
{
    public sealed class AchievementService : IService, IDisposable
    {
        private readonly AchievSO _achievSO;
        private readonly StatusSO _statusSo;
        private bool _playingNewGame;
        private int _savedScore;

        private string _statusAchievId;

        public AchievementService(AchievSO so, StatusSO statusSo)
        {
            _achievSO = so;
            _statusSo = statusSo;

            for (int i = 0; i < _achievSO.AchievsByType.Count; i++)
            {
                var achievBlock = _achievSO.AchievsByType[i];
                for (int j = 0; j < achievBlock.Achievements.Count; j++)
                {
                    var achievement = achievBlock.Achievements[j];
#if !UNITY_EDITOR
                    var newValue = GP_Variables.GetInt($"Achiev_{achievement.AchievID}");
                    if (newValue == 0)
                        continue;

                    if (achievBlock.AchievementType == AchievType.MergeByRank)
                        achievement.Condition = GP_Variables.GetInt($"Achiev_{achievement.AchievID}");
                    else
                        achievement.ReferenceValue = GP_Variables.GetInt($"Achiev_{achievement.AchievID}");
#endif
                }
            }
        }

        public event Action<Achievement> OnUnlockAchiev;
        public event Action<PlayerStatus> OnSetStatusAchiev;

        public void Open() => GP_Achievements.Open();

        public int ProgressPoints()
        {
            if(_statusAchievId == null)
            {
                var achiev = GetStatusAchievement();
                _statusAchievId = achiev.AchievID.ToString();
                achiev.SavedProgress = GP_Achievements.GetProgress(_statusAchievId);
            }
#if !UNITY_EDITOR
            UpdateStatusAchievement(_statusAchievId);
#endif
            return GP_Achievements.GetProgress(_statusAchievId);
        }

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
                    {
                        // new code
                        if (_achievSO.AchievsByType[i].AchievementType == AchievType.MergeByRank)
                            achiev.Condition = GP_Variables.GetInt($"Achiev_{achiev.AchievID}");
                        else
                            achiev.ReferenceValue = GP_Variables.GetInt($"Achiev_{achiev.AchievID}");
                        // end
                        continue;
                    }

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
            if (id != null)
            {
                GP_Achievements.Unlock(id);
                achiev.IsUnlocked = true;
                if (!CheckIfStatusAchivement(achiev))
                    OnUnlockAchiev?.Invoke(achiev);
            }
        }

        private bool CheckIfStatusAchivement(Achievement achiev)
        {
            if (achiev.IsTotal && achiev.RewardIndex >= Constants.BeginStatusAchiev)
            {
                if (_statusSo.TryGetByType(achiev.RewardIndex, out PlayerStatus status))
                    OnSetStatusAchiev?.Invoke(status);
            }
            return false;
        }

        private void UpdateStatusAchievement(string achievId)
        {
            var progress = GP_Achievements.GetProgress(achievId);
            if (progress == 0)
                GP_Achievements.SetProgress(achievId, (int)GP_Player.GetScore());

            for (int i = _statusSo.Statuses.Count - 1; i >= 0; i--)
            {
                if (_statusSo.Statuses[i].Goal <= progress)
                {
                    OnSetStatusAchiev?.Invoke(_statusSo.Statuses[i]);
                    UnityEngine.Debug.Log($"TestMerge: Status: found status {_statusSo.Statuses[i].Type}");
                    return;
                }
            }
        }

        private Achievement GetStatusAchievement()
        {
            var block = _achievSO.AchievsByType.Find(x => x.AchievementType == AchievType.Score);
            return block.Achievements[block.Achievements.Count - 1];
        }

        public bool TryGetNextStatus(PlayerStatus currentStatus, out PlayerStatus nextStatus)
        {
            nextStatus = null;
            if (_statusSo.TryGetByType((int)currentStatus.Next, out PlayerStatus status))
            {
                nextStatus = status;
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            OnUnlockAchiev = null;
            OnSetStatusAchiev = null;
        }
    }
}