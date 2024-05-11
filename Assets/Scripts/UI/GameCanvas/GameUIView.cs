using System.Collections;
using System;
using GamePush;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

namespace Code.MVC
{
    public class GameUIView : MonoBehaviour, IView
    {
        [SerializeField] private Image _nextImage;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject _ratingPanel;
        [SerializeField] private TextMeshProUGUI _ratingText;
        [SerializeField] private Button _leaderBoardButton;
        [SerializeField] private TextMeshProUGUI _nextText;

        [Header("Bomb settings")]
        [SerializeField] private Button _bombButton;
        [SerializeField] private Image _bombAdImage;
        [SerializeField] private Color _inactiveColor;

        private float _scoreValue;
        private float _highlightTime;

        public Button BombButton => _bombButton;
        public Button LeaderBoardButton => _leaderBoardButton;
        public Sprite NextSprite { get => _nextImage.sprite; set => _nextImage.sprite = value; }
        public float Score
        {
            get => _scoreValue;
            set
            {
                _scoreValue = value;
                _scoreText.text = _scoreValue.ToString();
            }
        }

        public event Action OnDestroyView;

        private void OnEnable()
        {
            _bombButton.interactable = false;
            _bombAdImage.color = SetAdImage(_bombButton.interactable);
            _bombButton.onClick.AddListener(Animate);
        }

        private void Start()
        {
#if UNITY_EDITOR
            _highlightTime = Constants.HIGHLIGHT_BOMB_TIME;
#else
            _highlightTime = GP_Variables.GetFloat("HighlightTime");
#endif
        }

        private void Animate() => StartCoroutine(StartLoading());

        public void SetRating(int rating)
        {
            var toShow = rating != 0;
            _ratingPanel.SetActive(toShow);
            _ratingText.text = toShow ? rating.ToString() : string.Empty;
        }

        public void SetTexts(string[] texts) => _nextText.text = texts[0];

        public void ActivateRewardButton(bool active)
        {
            BombButton.interactable = active;
            _bombAdImage.color = SetAdImage(_bombButton.interactable);
            if (active)
                StartCoroutine(ShowRewardAvailable());
        }

        private Color SetAdImage(bool active) => active ? Color.white : _inactiveColor;

        private IEnumerator ShowRewardAvailable()
        {
            float count = 0;
            while (count < _highlightTime && Time.timeScale > 0 && BombButton.interactable)
            {
                if (Time.deltaTime != 0)
                {
                    count += Time.deltaTime;
                    _bombButton.transform.localScale = Vector3.one * (Mathf.PingPong(count, 0.5f) + 1);
                    yield return null;
                }
            }
            _bombButton.transform.localScale = Vector3.one;
        }

        private IEnumerator StartLoading()
        {
            StopCoroutine(ShowRewardAvailable());
            _bombButton.interactable = false;

            yield return new WaitForSecondsRealtime(Constants.LoadAdWaitTime);

            _bombAdImage.color = SetAdImage(false);
            GameEventSystem.Send(new LoadADEvent(false));
        }

        private void OnDestroy()
        {
            OnDestroyView?.Invoke();
            OnDestroyView = null;
            _bombButton.onClick.RemoveAllListeners();
            _leaderBoardButton.onClick.RemoveAllListeners();
        }
    }
}