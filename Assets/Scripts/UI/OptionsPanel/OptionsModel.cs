using System;
using System.Collections.Generic;
using UnityEngine;
using GamePush;
using Code.Achievements;
using Code.SaveLoad;
using static AchievSO.AchievBlock;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Code.MVC
{
    public class OptionsModel : IModel
    {
        private AchievSO _achievSO;
        private ActiveBackgrounds _actives;
        private readonly List<int> _unlockedRewards = new();
        private int _currentIndex;
        private bool _requestedState;

        public event Action<int, bool> OnUpdateReward;
        public event Action<string[]> OnLanguageChanged;

        public void Init(AchievSO achievSO)
        {
            _achievSO = achievSO;
            var saveService = ServiceLocator.Container.RequestFor<SaveService>();
            _actives = saveService.Actives;
            ServiceLocator.Container.RequestFor<AchievementService>().OnUnlockAchiev
                += OnUnlockAchiev;
#if UNITY_EDITOR
            //Do nothing
#else
            UpdateSOInfo(saveService.FetchedAchievs);
#endif
        }

        public void UpdateSOInfo(List<AchievementsFetchPlayer> fetchedAchievs)
        {
            for (int i = 0; i < _achievSO.AchievsByType.Count; i++)
            {
                var block = _achievSO.AchievsByType[i];
                for (int y = 0; y < block.Achievements.Count; y++)
                {
                    var achiev = block.Achievements[y];
                    var sourceAchiev = fetchedAchievs.Find(x => x.achievementId == achiev.AchievID);
                    if (achiev.RewardIndex != 0 && sourceAchiev != null)
                    {
                        achiev.IsUnlocked = sourceAchiev.unlocked;
                        if (achiev.IsUnlocked)
                            _unlockedRewards.Add(achiev.RewardIndex);
                    }
                }
            }
        }

        public void UpdateStates()
        {
            for (int i = 0; i < _unlockedRewards.Count; i++)
                SendActivesUpdate(_unlockedRewards[i]);
        }

        public void OnUnlockAchiev(Achievement achiev) 
            => _unlockedRewards.Add(achiev.RewardIndex);

        public void OnChangeRewardState(int id, bool toApply)
        {
            _currentIndex = id;
            _requestedState = toApply;
            if (toApply)
                ShowRewardAd();
            else
                ChangeRewardState();
        }

        private void ShowRewardAd()
        {
            GP_Ads.ShowRewarded(Constants.REWARD, OnRewardSuccessful, OnRewardStart, OnRewardClose);
        }

        private void OnRewardSuccessful(string rewardName) => ChangeRewardState();
        private void OnRewardStart() => GameEventSystem.Send(new LoadADEvent(true));
        private void OnRewardClose(bool isSuccess)
        {
            GameEventSystem.Send(new LoadADEvent(false));
            OnUpdateReward?.Invoke(_currentIndex, !_requestedState);
        }

        private void ChangeRewardState()
        {
            //GameEventSystem.Send(new LoadADEvent(false));
            if (_requestedState)
                _actives.Actives.Add(_currentIndex);
            else
                _actives.Actives.Remove(_currentIndex);

            var data = JsonUtility.ToJson(_actives);
            GP_Player.Set("active_background", data);
            GP_Player.Sync();

            OnUpdateReward?.Invoke(_currentIndex, _requestedState);
        }

        private void SendActivesUpdate(int id)
        {
            var isActive = _actives.Actives.Contains(id);
            OnUpdateReward?.Invoke(id, isActive);
        }

        public void UpdateTextAsync() =>
            LocalizationSettings.StringDatabase.GetTableAsync("TextTable").Completed +=
            handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var table = handle.Result;
                    OnLanguageChanged?.Invoke(new string[1] { table.GetEntry("roomExit")?.GetLocalizedString() });
                }
            };

        public void Dispose()
        {
            _unlockedRewards.Clear();
            OnUpdateReward = null;
            OnLanguageChanged = null;
        }
    }
}