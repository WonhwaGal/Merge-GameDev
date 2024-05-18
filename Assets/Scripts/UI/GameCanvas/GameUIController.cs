
using System.Diagnostics;

namespace Code.MVC
{
    public sealed class GameUIController : Controller<GameUIView, GameUIModel>
    {
        private bool _queueWasMoved;

        public GameUIController(DropObjectSO data) : base()
        {
            Model.Init(data);
            GameEventSystem.Subscribe<CreateDropEvent>(UpdateUIData);
            GameEventSystem.Subscribe<GameControlEvent>(ReactToRetry);
            GameEventSystem.Subscribe<KeyEvent>(ReactToKeyEvent);
        }

        public float GetScore() => View.Score;
        public void SetScore() => View.Score = Model.SetScore();

        public override void UpdateView()
        {
            if (_queueWasMoved)
                View.NextSprite = Model.GetNextRank();
            else
                View.Score = Model.GetAddPoints(View.Score);
        }

        private void UpdateUIData(CreateDropEvent @event)
        {
            _queueWasMoved = @event.QueueMoved;
            Model.MergedRank = @event.CurrentRank;
            UpdateView();
        }

        private void ReactToRetry(GameControlEvent @event)
        {
            Model.RenewRating();
            if (@event.RestartWithRetry)
            {
                View.Score = 0;
                Model.RenewRating();
            }
        }

        private void ReactToKeyEvent(KeyEvent @event)
        {
            UnityEngine.Debug.Log($"Key: ui controller received event from KEY");
            View.KeyBubble = @event.KeyBubble;
            @event.KeyBubble.KetSetView.OnKeyClicked += View.MoveKey;
        }

        protected override void OnViewAdded()
        {
            View.BombButton.onClick.AddListener(Model.ShowRewardAd);
            View.LeaderBoardButton.onClick.AddListener(Model.OpenLeaderBoard);
            Model.OnActivateReward += View.ActivateRewardButton;
            Model.OnGetRating += View.SetRating;
            Model.OnLanguageChanged += View.SetTexts;
        }

        protected override void Hide() => View.gameObject.SetActive(false);
        protected override void Show() => View.gameObject.SetActive(true);

        public override void Dispose()
        {
            GameEventSystem.UnSubscribe<CreateDropEvent>(UpdateUIData);
            GameEventSystem.UnSubscribe<GameControlEvent>(ReactToRetry);
            GameEventSystem.UnSubscribe<KeyEvent>(ReactToKeyEvent);
            Model.Dispose();
            base.Dispose();
        }
    }
}