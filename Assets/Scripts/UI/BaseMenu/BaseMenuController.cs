using System;
using UnityEngine;

namespace Code.MVC
{
    public abstract class BaseMenuController<BaseV, BaseM> : Controller<BaseV, BaseM>
                    where BaseV : BaseMenuView
                    where BaseM : BaseMenuModel, new()
    {
        private GameAction _gameAction;
        protected GameAction _triggerAction;

        public BaseMenuController() : base()
        {
            GameEventSystem.Subscribe<GameControlEvent>(SetUpPanel);
        }

        public Func<float> OnRequestScore;

        private void SetUpPanel(GameControlEvent @event)
        {
            _gameAction = @event.ActionToDo;
            Model.LastAction = _gameAction;
            UpdateView();
        }

        public override void UpdateView()
        {
            UpdateTimeScale();
            if (_gameAction == _triggerAction)
                Show();
            else if (_gameAction == GameAction.Play)
                Hide();
        }

        protected void UpdateTimeScale()
        {
            if (_gameAction == GameAction.Save)
                return;

            Time.timeScale = _gameAction == GameAction.Play ? 1 : 0;
        }

        protected override void Hide() => View.gameObject.SetActive(false);
        protected override void Show() { }

        protected override void OnViewAdded()
        {
            View.RetryButton.onClick.AddListener(Model.PressRetry);
            View.MusicButton.SetBool(Model.GetVolume(SoundType.VolumeMusic));
            View.SoundButton.SetBool(Model.GetVolume(SoundType.VolumeSound));
            Model.OnLanguageChanged += View.SetTexts;
            InitComponents();
        }

        protected abstract void InitComponents();
        protected virtual void OnDispose() { }
        
        public override void Dispose()
        {
            GameEventSystem.UnSubscribe<GameControlEvent>(SetUpPanel);
            Model.OnLanguageChanged -= View.SetTexts;
            OnRequestScore = null;
            OnDispose();
            base.Dispose();
        }
    }
}