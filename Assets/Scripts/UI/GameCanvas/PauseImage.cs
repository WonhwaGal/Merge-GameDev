using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public sealed class PauseImage : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _playImage;
        private bool _isPaused;
        private GameAction _toDoAction;
        private bool _shouldBeDisabled = false;

        private void Start()
        {
            _playImage.gameObject.SetActive(false);
            GameEventSystem.Subscribe<GameControlEvent>(UpdatePause);
            GameEventSystem.Subscribe<KeyEvent>(ReactToKeyEvent);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_toDoAction == GameAction.Lose || _shouldBeDisabled)
                return;

            _isPaused = !_isPaused;
            _toDoAction = _isPaused ? GameAction.Pause : GameAction.Play;
            GameEventSystem.Send(new GameControlEvent(_toDoAction, false));
            UpdateImage();
        }

        private void UpdatePause(GameControlEvent @event)
        {
            _toDoAction = @event.ActionToDo;
            if (_toDoAction == GameAction.Pause || _toDoAction == GameAction.Lose)
                _isPaused =true;
            else if(_toDoAction == GameAction.Play)
                _isPaused =false;
            UpdateImage();
        }

        private void UpdateImage() => _playImage.gameObject.SetActive(_isPaused);

        private void ReactToKeyEvent(KeyEvent @event)
        {
            UnityEngine.Debug.Log($"Key: pause button received event from KEY");
            @event.KeyBubble.KetSetView.OnKeyClicked += DisableButton;
        }

        private void DisableButton(bool firstClick) => _shouldBeDisabled = firstClick;

        private void OnDestroy()
        {
            GameEventSystem.UnSubscribe<GameControlEvent>(UpdatePause);
            GameEventSystem.UnSubscribe<KeyEvent>(ReactToKeyEvent);
        }
    }
}