using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuDog : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Transform _objectT;
    [SerializeField, Range(50, 120)] private float _speed;
    private Vector2 _shift;
    private bool _running;

    private void Start() 
        => _shift = new(0, (transform.position.y - _objectT.position.y) / _speed);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_running)
            StartCoroutine(AnimateObject());
    }

    private IEnumerator AnimateObject()
    {
        _running = true;
        var startPos = _objectT.position.y;
        while (_objectT.position.y < transform.position.y)
        {
            MoveObject(_shift);
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);

        while(_objectT.position.y >= startPos)
        {
            MoveObject(-_shift);
            yield return null;
        }
        _running = false;
    }

    private void MoveObject(Vector2 vector)
    {
        _objectT.position = Vector3.Slerp(
            _objectT.position, (Vector2)_objectT.position + vector, 1);
    }
}