using UnityEngine;

public class BoolClickView : MonoBehaviour
{
    [SerializeField] private GameObject _onImage;
    [SerializeField] private GameObject _offImage;
    [SerializeField] private bool _initialStateOn;
    private bool _isOn;

    private void Start()
    {
        _isOn = _initialStateOn;
        _onImage.SetActive(_isOn);
        _offImage.SetActive(!_isOn);
    }

    private void OnMouseDown()
    {
        _isOn = !_isOn;
        _onImage.SetActive(_isOn);
        _offImage.SetActive(!_isOn);
    }
}