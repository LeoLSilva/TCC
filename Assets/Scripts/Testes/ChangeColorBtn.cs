using UnityEngine;
using UnityEngine.UI;

public class ChangeColorBtn : MonoBehaviour
{
    [SerializeField] private Image _img;
    [SerializeField] private Color _natural;
    [SerializeField] private Color _color;


    public void ChangeColor(bool natural)
    {
        if (natural) _img.color = _natural;
        else _img.color = _color;
    }
}
