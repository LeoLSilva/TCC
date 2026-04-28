using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionLineUI : MonoBehaviour
{
    [SerializeField] private Image _item1Image;
    [SerializeField] private Image _actionIcon;
    [SerializeField] private Image _item2Image;
    [SerializeField] private GameObject _equalsIcon;
    [SerializeField] private Image _resultImage;
    [SerializeField] private TextMeshProUGUI _countText;

    [SerializeField] private Sprite _printIcon;
    [SerializeField] private Sprite _connectIcon;
    [SerializeField] private Sprite _disconnectIcon;

    public void SetupLine(PlayerActionType actionType, int cont, Sprite item1, Sprite item2 = null, Sprite result = null)
    {
        _countText.text = cont.ToString();

        _item1Image.sprite = item1;
        _item1Image.gameObject.SetActive(true);

        switch (actionType)
        {
            case PlayerActionType.Print:
                _actionIcon.sprite = _printIcon;
                _actionIcon.gameObject.SetActive(true);

                if (_item2Image != null) _item2Image.gameObject.SetActive(false);
                if (_equalsIcon != null) _equalsIcon.SetActive(false);
                if (_resultImage != null) _resultImage.gameObject.SetActive(false);
                break;

            case PlayerActionType.Connect:
                _actionIcon.sprite = _connectIcon;
                _actionIcon.gameObject.SetActive(true);

                if (_item2Image != null)
                {
                    _item2Image.sprite = item2;
                    _item2Image.gameObject.SetActive(true);
                }

                if (result != null)
                {
                    if (_equalsIcon != null) _equalsIcon.SetActive(true);
                    if (_resultImage != null)
                    {
                        _resultImage.sprite = result;
                        _resultImage.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (_equalsIcon != null) _equalsIcon.SetActive(false);
                    if (_resultImage != null) _resultImage.gameObject.SetActive(false);
                }
                break;

            case PlayerActionType.Disconnect:
                _actionIcon.sprite = _disconnectIcon;
                _actionIcon.gameObject.SetActive(true);

                if (_item2Image != null)
                {
                    _item2Image.sprite = item2;
                    _item2Image.gameObject.SetActive(true);
                }

                if (_equalsIcon != null) _equalsIcon.SetActive(false);
                if (_resultImage != null) _resultImage.gameObject.SetActive(false);
                break;
        }
    }
}