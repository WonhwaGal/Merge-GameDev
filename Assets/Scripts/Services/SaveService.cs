using System;
using System.Collections.Generic;
using GamePush;
using UnityEngine;

namespace Code.SaveLoad
{
    public sealed class SaveService : IService
    {
        private readonly ProgressHandler _handler;

        public SaveService()
        {
            _handler = new();
            GP_Achievements.OnAchievementsFetchPlayer += FetchAchievs;
            GameEventSystem.Subscribe<ManageDropEvent>(GatherData);
            GP_Achievements.Fetch();
        }

        public ProgressData ProgressData { get; private set; }
        public List<AchievementsFetchPlayer> FetchedAchievs { get; private set; }
        public ActiveBackgrounds Actives { get; private set; }

        public bool LoadProgress(string data)
        {
            ProgressData = JsonUtility.FromJson<ProgressData>(data);
            return ProgressData.SavedDropList != null && ProgressData.SavedDropList.Count > 0;
        }

        public void GatherData(ManageDropEvent @event)
        {
            if (!@event.ReturnToPool)
                _handler.FillData(@event.Drop);
        }

        public void AddKey(Transform transform) => _handler.AddKey(transform);

        public void SaveData(float currentScore, bool onlyScore)
        {
            GP_Player.SetScore(GetBestScore(currentScore));
            if(!onlyScore && currentScore != 0 && _handler.Drops.Count > 0)
            {
                GP_Player.Set(Constants.SavedScore, currentScore);
                ProgressData = new ProgressData(_handler.Drops);
                string json = JsonUtility.ToJson(ProgressData);
                GP_Player.Set(Constants.DropList, json);
            }

            GP_Player.Sync();
            ClearData();
        }

        public void ClearData() => _handler.Clear();

        private void FetchAchievs(List<AchievementsFetchPlayer> fetchedAchievs)
        {
            var actives = GP_Player.GetString("active_background");
            Actives = JsonUtility.FromJson<ActiveBackgrounds>(actives);
            FetchedAchievs = fetchedAchievs;
        }

        private float GetBestScore(float current)
        {
            var bestScore = GP_Player.GetScore();
            return (current > bestScore) ? current : bestScore;
        }

        public void Dispose()
        {
            GameEventSystem.UnSubscribe<ManageDropEvent>(GatherData);
            GC.SuppressFinalize(this);
        }
    }
}