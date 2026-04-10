using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ScannerLoadingBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        ResetBar();
    }

    public void UpdateBar(float progress)
    {
        if (_canvasGroup != null && _canvasGroup.alpha == 0f && progress > 0f)
        {
            _canvasGroup.alpha = 1f;
        }

        if (_fillImage != null)
        {
            _fillImage.fillAmount = progress;
        }
    }

    public void CompleteBar()
    {
        if (_fillImage != null)
        {
            _fillImage.fillAmount = 1f;
        }
        Invoke(nameof(HideBar), 1f);
    }

    public void ResetBar()
    {
        if (_fillImage != null)
        {
            _fillImage.fillAmount = 0f;
        }
        HideBar();
    }

    private void HideBar()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }
}