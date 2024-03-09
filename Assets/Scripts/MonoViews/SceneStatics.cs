using UnityEngine;
using GamePush;
using Code.Achievements;
using Code.SaveLoad;

namespace Code.Views
{
    public class SceneStatics : MonoBehaviour
    {
        [Header("Static Drops Queue")]
        [SerializeField] private StaticQueueView _currentQueue;

        [Header("BackGrounds")]
        [SerializeField] private GameObject[] _backgrounds;

        private bool _isMobile;
        private int _dropableRanks;
        private int _lastUnlockedRank;
        private AchievementService _achievService;

        private void Awake()
        {
            _currentQueue.gameObject.SetActive(true);
            _isMobile = _backgrounds.Length == 0;
            if(!_isMobile)
            {
                for (int i = 0; i < _backgrounds.Length; i++)
                    _backgrounds[i].gameObject.SetActive(false);
                GameEventSystem.Subscribe<BackgroundEvent>(UpdateBackground);
            }
            GameEventSystem.Subscribe<MergeEvent>(UnlockRankView);
        }

        private void Start()
        {
            SetUp();
            if (_currentQueue != null)
            {
                var lastIndex = _lastUnlockedRank - _dropableRanks;
                for (int i = 0; i < lastIndex; i++)
                    _currentQueue.LockedViews[i].SetActive(false);
            }
        }

        private void SetUp()
        {
            _achievService = ServiceLocator.Container.RequestFor<AchievementService>();
            _lastUnlockedRank = GP_Player.GetInt("unlocked_ranks");
#if UNITY_EDITOR
            _dropableRanks = Constants.DropableRanks;
#else
            _dropableRanks = GP_Variables.GetInt("DropableRanks");
#endif

            if (_lastUnlockedRank == 0)
                _lastUnlockedRank = _dropableRanks;

#if !UNITY_EDITOR
            if (!_isMobile)
                SetUpBackground();
#endif

        }

        private void SetUpBackground()
        {
            var service = ServiceLocator.Container.RequestFor<SaveService>();
            var activeBackgroundList = service.Actives.Actives;
            for (int i = 0; i < _backgrounds.Length; i++)
                _backgrounds[i].SetActive(activeBackgroundList.Contains(i));
        }

        private void UnlockRankView(MergeEvent @event)
        {
            if (@event.MergingRank == _lastUnlockedRank)
            {
                var index = _lastUnlockedRank - _dropableRanks;
                _currentQueue.LockedViews[index].SetActive(false);
                _lastUnlockedRank++;
                GP_Player.Set("unlocked_ranks", _lastUnlockedRank);
                GP_Player.Sync();
                _achievService.CheckAchievement(AchievType.FullSetUnlock, _lastUnlockedRank);
            }
        }

        private void UpdateBackground(BackgroundEvent @event)
        {
            if (@event.Index < _backgrounds.Length)
                _backgrounds[@event.Index].SetActive(@event.ToApply);
        }

        private void OnDestroy()
        {
            GameEventSystem.UnSubscribe<MergeEvent>(UnlockRankView);
            GameEventSystem.UnSubscribe<BackgroundEvent>(UpdateBackground);
        }
    }
}