using System;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using Code.SaveLoad;
using GamePush;
using UnityEngine.Localization.Tables;

namespace Code.MVC
{
    public class BaseMenuModel : IModel
    {
        private readonly SaveService _saveService;

        public BaseMenuModel()
        {
            GameEventSystem.Subscribe<SaveEvent>(OnSaveData);
            _saveService = ServiceLocator.Container.RequestFor<SaveService>();
        }

        public GameAction LastAction { get; set; }

        public event Action<string[]> OnLanguageChanged;

        public void OnSaveData(SaveEvent @event)
        {
            if (LastAction == GameAction.Lose && !@event.OnlyScore)
            {
                _saveService.ClearData();
                return;
            }

            if (!@event.OnlyScore)
                GameEventSystem.Send(new GameControlEvent(GameAction.Save, false));
            _saveService.SaveData(@event.CurrentScore, @event.OnlyScore);
        }

        public void PressRetry() => GameEventSystem
            .Send(new GameControlEvent(GameAction.Play, restartWithRetry: true));

        public bool GetVolume(SoundType type)
        {
            if (type == SoundType.VolumeMusic)
                return GP_Player.GetBool(Constants.TotalMusic);
            else
                return GP_Player.GetBool(Constants.TotalSound);
        }

        public void UpdateTextAsync() =>
            LocalizationSettings.StringDatabase.GetTableAsync("TextTable").Completed +=
                handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        var table = handle.Result;
                        OnLanguageChanged?.Invoke(GetEntries(table));
                    }
                };

        protected virtual string[] GetEntries(StringTable table) => new string[] { };

        public void Dispose()
        {
            OnLanguageChanged = null;
            GameEventSystem.UnSubscribe<SaveEvent>(OnSaveData);
            GC.SuppressFinalize(this);
        }
    }
}