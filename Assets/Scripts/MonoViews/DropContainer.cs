using System;
using UnityEngine;
using System.Collections;
using TMPro;
using System.Runtime.InteropServices;
using GamePush;

namespace Code.DropLogic
{
    public sealed class DropContainer : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private float _queueMoveDelay;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private float _leftBorder;
        [SerializeField] private float _rightBorder;

        private Camera _cam;
        private Vector3 _startPosition;
        private bool _isDragging;
        private bool _isBlocked;

        public DropBase CurrentDrop { get; set; }

        public event Func<Transform, bool, DropBase> OnObjectDrop;

        private void Start()
        {
            _cam = Camera.main;
            _startPosition = transform.position;
            GameEventSystem.Subscribe<RewardEvent>(CreateBomb);
            GameEventSystem.Subscribe<LoadADEvent>(ReactToLoadingAd);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus && Time.timeScale != 0)
                if (Int32.TryParse(_scoreText.text, out int finalScore))
                    GameEventSystem.Send(new SaveEvent(finalScore, onlyScore: false));

            if (_isBlocked)
                return;

            GameEventSystem.Send(new SoundEvent(SoundType.BackGroundMusic, focus));
        }

        private void OnMouseDrag()
        {
            if (Time.timeScale == 0 || _isBlocked)
                return;
            _isDragging = true;
            var currentMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
            if (currentMousePos.x > _leftBorder && currentMousePos.x < _rightBorder)
                CurrentDrop.Pos = new Vector3(currentMousePos.x, transform.position.y, 0.0f);
        }

        private void OnMouseUp()
        {
            if (!_isDragging || !CurrentDrop.gameObject.activeSelf || _isBlocked)
                return;
            GameEventSystem.Send(new SoundEvent(SoundType.Drop, true));
            PrepairNext();
            StartCoroutine(DelayQueueMove());
        }

        private void PrepairNext()
        {
            CurrentDrop.Drop();
            transform.position = _startPosition;
            Debug.LogWarning("TestMerge: changed container pos");
            CurrentDrop = OnObjectDrop?.Invoke(transform, true);  // true = random drop object
            Debug.LogWarning("TestMerge: assigned new drop");
            if (CurrentDrop == null)
                Debug.LogWarning("TestMerge: Current drop is null");
            CurrentDrop.gameObject.SetActive(false);
            MergeCounter.MergesInARow = 0;
        }

        IEnumerator DelayQueueMove()
        {
            float delay = 0;
            while (delay < _queueMoveDelay)
            {
                if (Time.timeScale != 0)
                    delay += Time.deltaTime;
                yield return null;
            }
            CurrentDrop.gameObject.SetActive(true);
            _isDragging = false;
        }

        private void CreateBomb(RewardEvent @event)
        {
            GameEventSystem.Send(new ManageDropEvent(CurrentDrop, true, withEffects: false));
            CurrentDrop = OnObjectDrop?.Invoke(transform, false);
            Debug.LogWarning("TestMerge: set up the bomb");
        }

        private void ReactToLoadingAd(LoadADEvent @event) => _isBlocked = @event.StartLoading;

        private void OnDestroy()
        {
            OnObjectDrop = null;
            GameEventSystem.UnSubscribe<RewardEvent>(CreateBomb);
            GameEventSystem.UnSubscribe<LoadADEvent>(ReactToLoadingAd);
        }
    }
}