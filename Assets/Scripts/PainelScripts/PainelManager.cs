using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PainelManager : MonoBehaviour
{
    [SerializeField] private int _currentDiagram;
    [SerializeField] private TextMeshProUGUI _diagramName;
    [SerializeField] private List<DiagramScriptableObject> _diagramsList = new List<DiagramScriptableObject>();

    private void Start()
    {
        SetDiagram(0);
    }
    public void ChangeDiagram(bool left)
    {
        if (left) _currentDiagram--;
        else _currentDiagram++;
        if (_currentDiagram < 0) _currentDiagram = _diagramsList.Count - 1;
        else if (_currentDiagram > _diagramsList.Count) _currentDiagram = 0;

        SetDiagram(_currentDiagram);
    }

    private void SetDiagram(int diagram)
    {
        _diagramName.text = _diagramsList[diagram].name;
    }


}
