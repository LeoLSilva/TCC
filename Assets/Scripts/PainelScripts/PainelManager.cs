using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PainelManager : MonoBehaviour
{
    [SerializeField] private int _currentDiagram;
    [SerializeField] private List<DiagramScriptableObject> _diagramsList = new List<DiagramScriptableObject>();
    [SerializeField] private TextMeshProUGUI _diagramName;
    [SerializeField] private DiagramManager _diagramManager;
    [SerializeField] private PrinterManager _printerManager;

    private void Start()
    {
        _printerManager = FindAnyObjectByType<PrinterManager>();
        _diagramManager = FindAnyObjectByType<DiagramManager>();
        SetDiagram(0);
    }
    public void ChangeDiagram(bool left)
    {
        _currentDiagram = (_currentDiagram + (left ? -1 : 1) + _diagramsList.Count) % _diagramsList.Count;
        SetDiagram(_currentDiagram);
    }

    private void SetDiagram(int diagram)
    {
        if (_diagramManager == null) { _diagramManager = FindAnyObjectByType<DiagramManager>(); }
        //_diagramName.text = _diagramsList[diagram].name;
        Debug.Log(GetDiagram(diagram).name);
        _diagramManager.SetDiagram(GetDiagram(diagram)).SetPainel(true);
    }

    public void PrinterBtn()
    {
        _printerManager.Printer();
    }

    private DiagramScriptableObject GetDiagram(int diagram)
    {
        return _diagramsList[diagram];
    }
}
