using System;
using Code.MVC;
using UnityEngine;
using GamePush;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using Code.Achievements;
using Unity.Burst.CompilerServices;

public class GameUIModel : IModel, IDisposable
{
    private DropObjectSO _dropData;
    private float _currentScore;
    private int _playerRating;
    private bool _bombActive;
    private int _bombActivationSpan;
    private AchievementService _achievementService;

    public int MergedRank { get; set; }

    public event Action<bool> OnActivateReward;
    public event Action<int> OnGetRating;
    public event Action<string[]> OnLanguageChanged;

    public void Init(DropObjectSO dropData)
    {
        _dropData = dropData;
#if UNITY_EDITOR
        _bombActivationSpan = Constants.BombActivationSpan;
#else
        _bombActivationSpan = GP_Variables.GetInt("RewardActivationSpan");
#endif
        GP_Leaderboard.OnFetchPlayerRatingSuccess += OnFetchRating;
        GameEventSystem.Subscribe<SaveEvent>(SaveBombStatus);
        _achievementService = ServiceLocator.Container.RequestFor<AchievementService>();
        RenewRating();
        UpdateTextAsync();
    }

    public void OpenLeaderBoard() => GP_Leaderboard.Open(withMe: WithMe.first);

    public void UpdateTextAsync() =>
        LocalizationSettings.StringDatabase.GetTableAsync("TextTable").Completed +=
            handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var table = handle.Result;
                    OnLanguageChanged?.Invoke(new string[2] 
                    { 
                        table.GetEntry("nextUI")?.GetLocalizedString(),
                        table.GetEntry("keyText")?.GetLocalizedString()
                    });
                    Debug.Log($"sending {table.GetEntry("keyText")?.GetLocalizedString()}");
                }
            };

    #region Rating
    public void RenewRating() => GP_Leaderboard.FetchPlayerRating();

    private void OnFetchRating(string category, int rating)
    {
        if (rating <= Constants.ShowableRating)
            _playerRating = rating;
        else
            _playerRating = 0;
        OnGetRating?.Invoke(_playerRating);
    }
    #endregion

    #region Score
    public Sprite GetNextRank()
    {
        var nextRank = DropQueueHandler.NextDrop;
        return _dropData.FindObjectData(nextRank).DropSprite;
    }

    public float SetScore()
    {
        OnActivateReward?.Invoke(GP_Player.GetBool(Constants.BombAvailable));
        _currentScore = GP_Player.GetInt(Constants.SavedScore);
        return _currentScore;
    }

    public float GetAddPoints(float currentScore)
    {
        Debug.Log($"TestMerge: merged rank {MergedRank}, score add {_dropData.FindObjectData(MergedRank - 1).MergeRewardPoint}");
        int firstCheck = (int)_currentScore / _bombActivationSpan;
        _currentScore = currentScore + _dropData.FindObjectData(MergedRank - 1).MergeRewardPoint;
        int secondCheck = (int)_currentScore / _bombActivationSpan;

        if (secondCheck > firstCheck && !_bombActive)
            SetBombStatus(true);
        _achievementService.CheckAchievement(AchievType.Score, _currentScore);
        return _currentScore;
    }
    #endregion

    #region Bomb_Reward
    public void ShowRewardAd()
    {
#if UNITY_EDITOR
        OnRewardSuccessful(Constants.BOMB);
#else
        GP_Ads.ShowRewarded(Constants.BOMB, OnRewardSuccessful, OnRewardStart, OnRewardClose);
#endif
    }

    private void OnRewardSuccessful(string key)
    {
        if (key != Constants.BOMB)
            return;
        SetBombStatus(false);
        GameEventSystem.Send(new RewardEvent(Constants.BombRank));
    }

    private void OnRewardStart()
    {
        GameEventSystem.Send(new LoadADEvent(true));
        GameEventSystem.Send(new SoundEvent(SoundType.BackGroundMusic, false));
    }

    private void OnRewardClose(bool isSuccess)
    {
        GameEventSystem.Send(new LoadADEvent(false));
        GameEventSystem.Send(new SoundEvent(SoundType.BackGroundMusic, true));
        if (!isSuccess)
        {
            Debug.LogWarning("GameUIModel: bomb reward not received - ad not watched");
            SetBombStatus(false);
        }
    }

    private void SetBombStatus(bool toActivate)
    {
        OnActivateReward?.Invoke(toActivate);
        _bombActive = toActivate;
    }

    private void SaveBombStatus(SaveEvent @event)
    {
        if (!@event.OnlyScore)
        {
            GP_Player.Set(Constants.BombAvailable, _bombActive);
            GP_Player.Sync();
        }
    }
    #endregion

    public void Dispose()
    {
        GameEventSystem.UnSubscribe<SaveEvent>(SaveBombStatus);
        OnLanguageChanged = null;
        OnActivateReward = null;
        OnGetRating = null;
    }
}