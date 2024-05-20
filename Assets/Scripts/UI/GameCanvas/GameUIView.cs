using System.Collections;
using System;
using GamePush;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private TextMeshProUGUI _keyText;

        [Header("Bomb settings")]
        [SerializeField] private Button _bombButton;
        [SerializeField] private Image _bombAdImage;
        [SerializeField] private Color _inactiveColor;

        [Header("Key settings")]
        [SerializeField] private Transform _keyFinalSpot;
        [SerializeField] private GameObject _keyPanel;

        private float _scoreValue;
        private float _highlightTime;
        private KeyBubble _keyView;
        private bool _bombButtonActive;

        public Button BombButton => _bombButton;
        public Button LeaderBoardButton => _leaderBoardButton;
        public KeyBubble KeyBubble { get => _keyView; set => _keyView = value; }
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
            _keyPanel.SetActive(false);
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

        public void MoveKey(bool firstClick)
        {
            if (firstClick)
            {
                StartCoroutine(MoveKeyCloser());
            }
            else
            {
                _keyPanel.SetActive(false);
                _bombButton.interactable = _bombButtonActive;
                _leaderBoardButton.interactable = true;
            }
        }

        public void SetTexts(string[] texts)
        {
            _nextText.text = texts[0];
            if (GP_Language.Current() == Language.Russian)
                _keyText.text = "Отличная работа!\r\n\r\nТвоя пекарня теперь открыта!";
            else
                _keyText.text = "Great job! \r\n\r\nYour bakery is now open!";
        }

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

        private IEnumerator MoveKeyCloser()
        {
            _bombButtonActive = BombButton.interactable;
            _bombButton.interactable = false;
            _leaderBoardButton.interactable = false;

            var key = KeyBubble.KetSetView;
            var multiplier = Constants.KeyMultiplier;
            key.Background.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            key.Background.SetActive(true);

            while (key.transform.localScale.x < Constants.ShowKeyScale)
            {
                key.transform.localScale *= multiplier;
                key.transform.position = Vector3.Slerp(key.transform.position, _keyFinalSpot.position, 0.05f);

                key.Background.transform.localScale *= 1.01f;
                yield return new WaitForSeconds(0.02f);
            }
            _keyPanel.SetActive(true);
            KeyBubble.KetSetView.ReadyToTake = true;
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