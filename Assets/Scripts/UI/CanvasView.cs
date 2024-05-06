using Code.UI;
using System.Collections;
using UnityEngine;

namespace Code.MVC
{
    public class CanvasView : MonoBehaviour
    {
        [SerializeField] private PauseImage _pauseImage;
        [SerializeField] private GameUIView _gameUIView;
        [SerializeField] private PauseMenuView _pauseView;
        [SerializeField] private LoseMenuView _loseView;
        [SerializeField] private OptionsView _optionsView;
        [SerializeField] private LoadingPanel _loadingPanel;

        public PauseImage PauseImage => _pauseImage;
        public GameUIView GameUIView => _gameUIView;
        public OptionsView OptionsView => _optionsView;
        public PauseMenuView PauseNew => _pauseView;
        public LoseMenuView LoseView => _loseView;

        private void Start()
        {
            _loadingPanel.gameObject.SetActive(false);
            GameEventSystem.Subscribe<LoadADEvent>(StartLoadingAnimation);
        }

        private void StartLoadingAnimation(LoadADEvent @event)
        {
            _loadingPanel.gameObject.SetActive(@event.StartLoading);
            if (@event.StartLoading)
                StartCoroutine(TurnOnLoadingPnel());
            else
                StopAllCoroutines();
        }

        private IEnumerator TurnOnLoadingPnel()
        {
            yield return new WaitForSecondsRealtime(Constants.LoadAdWaitTime);
            _loadingPanel.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            GameEventSystem.UnSubscribe<LoadADEvent>(StartLoadingAnimation);
        }
    }
}