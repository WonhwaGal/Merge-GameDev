using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    [SerializeField] private Image _loadingImage;

    private void OnEnable()
    {
        StartCoroutine(LoadAnimation());
    }

    private IEnumerator LoadAnimation()
    {
        _loadingImage.fillAmount = 0;
        while (gameObject.activeInHierarchy)
        {
            _loadingImage.fillAmount += Constants.DeltaTimeStep / 2;
            if(_loadingImage.fillAmount >= 1)
                _loadingImage.fillAmount = 0;
            yield return new WaitForSecondsRealtime(Constants.DeltaTimeStep);
        }
    }

    private void OnDisable()
    {
        StopCoroutine(LoadAnimation());
    }
}
