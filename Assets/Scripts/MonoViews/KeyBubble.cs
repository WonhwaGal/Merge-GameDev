using System.Collections;
using UnityEngine;
using GamePush;
using UnityEditor;
using Code.SaveLoad;

public class KeyBubble : MonoBehaviour
{
    [SerializeField] private KeySetView _keyView;

    private float _highlightTime;
    private Vector3 _initialScale;
    private SaveService _saveService;

    public KeySetView KetSetView => _keyView;

    private void OnEnable()
    {
        _initialScale = KetSetView.transform.localScale;
        _keyView.OnKeyClicked += OnKeyClicked;
#if UNITY_EDITOR
        _highlightTime = Constants.HIGHLIGHT_BOMB_TIME;
#else
        _highlightTime = GP_Variables.GetFloat("HighlightTime");
#endif
        StartCoroutine(PumpKey());
        GameEventSystem.Send(new KeyEvent(this));
        GameEventSystem.Subscribe<GameControlEvent>(Register);
        _saveService = ServiceLocator.Container.RequestFor<SaveService>();
    }

    private void OnKeyClicked(bool firstClick)
    {
        if (firstClick)
            StopCoroutine(PumpKey());
        else
            gameObject.SetActive(false);
    }

    private IEnumerator PumpKey()
    {
        float count = 0;
        while (count < _highlightTime && Time.timeScale > 0)
        {
            if (Time.deltaTime != 0)
            {
                count += Time.deltaTime;
                KetSetView.transform.localScale = _initialScale * (Mathf.PingPong(count, 0.5f) + 1);
                yield return null;
            }
        }
        KetSetView.transform.localScale = _initialScale;
    }

    private void Register(GameControlEvent @event)
    {
        bool shouldRegister = @event.ActionToDo != GameAction.Pause && @event.ActionToDo != GameAction.Lose;
        if (shouldRegister)
            _saveService.AddKey(transform);
    }

    private void OnDisable()
    {
        _keyView.OnKeyClicked -= OnKeyClicked;
        StopAllCoroutines();
        GameEventSystem.UnSubscribe<GameControlEvent>(Register);
    }
}