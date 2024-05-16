using GamePush;
using System;
using UnityEngine;

public class KeySetView : MonoBehaviour
{
    [SerializeField] private GameObject _particles;
    [SerializeField] private GameObject _background;
    private bool _keyClicked;

    public bool ReadyToTake { get; set; }
    public GameObject Background => _background;

    public event Action<bool> OnKeyClicked; // bool = first time click

    private void OnEnable()
    {
        _particles.SetActive(false);
        _background.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.CircleCastAll(clickPosition, 0.01f, Vector2.zero);

            if (hits.Length > 0)
            {
                for(int i = 0; i < hits.Length; i++)
                {
                    var keyItem = hits[i].collider.gameObject.GetComponent(typeof(KeySetView));
                    var keyBubble = hits[i].collider.gameObject.GetComponent(typeof(KeyBubble));
                    if(keyItem is not null || keyBubble is not null)
                    {
                        RegisterClick();
                        return;
                    }

                }
                Debug.LogWarning("DIDNT find the right hit");
            }
        }
    }

    private void RegisterClick()
    {
        if (!_keyClicked)
        {
            GameEventSystem.Send(new SoundEvent(SoundType.Key, true));
            _particles.SetActive(true);
            _keyClicked = true;
            OnKeyClicked?.Invoke(true);
        }
        else if (ReadyToTake)
        {
            GP_Player.Set("has_key", true);
            OnKeyClicked?.Invoke(false);
            gameObject.SetActive(false);
            GP_Player.Sync();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        OnKeyClicked = null;
    }
}
