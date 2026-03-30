using Oculus.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PainelTabs : MonoBehaviour
{
    private Dictionary<PointableUnityEventWrapper, GameObject> _listTabs = new Dictionary<PointableUnityEventWrapper, GameObject>();
    [SerializeField] private Image _img;
    [SerializeField] private Color _natural;
    [SerializeField] private Color _color;

    private void Start()
    {
        foreach (PointableUnityEventWrapper p  in _listTabs.Keys)
        {
            p.WhenSelect.AddListener(Select);
        }
    }

    private void Select(PointerEvent arg0)
    {
    }

    private void ChangeColor(bool natural)
    {
        if (natural) _img.color = _natural;
        else _img.color = _color;
    }
}
