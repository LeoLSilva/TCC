using Oculus.Interaction;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiagramScreen : MonoBehaviour
{
    [SerializeField] private GameObject _printerBtn;
    [SerializeField] private GameObject _currentDiagramBD;
    [SerializeField] private int _currentDiagram;
    [SerializeField] private TextMeshProUGUI _diagramName;

    [Header("Data")]
    [SerializeField] private List<DiagramDataBP> _diagramDataList = new List<DiagramDataBP>();

    [Header("Other Scripts")]
    [SerializeField] private DiagramManager _diagramManager;
    [SerializeField] private PrinterManager _printerManager;
    [SerializeField] private PainelUI _painelUi;

    [Header("Transforms")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _spawnStartPoint;

    private GameObject _lastSpawnedDiagram;

    private void Start()
    {
        if (_diagramManager == null)
        {
            _diagramManager = FindAnyObjectByType<DiagramManager>();
        }

        _printerManager = FindAnyObjectByType<PrinterManager>();
        _printerManager.OnPrinterStateChanged += _SetButtonPrintActive;
        SetDiagramBDActive();
    }

    private void OnEnable()
    {
        foreach (var data in _diagramDataList)
        {
            if (data != null && data.DiagramObject != null) 
                data.DiagramObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        foreach (var data in _diagramDataList)
        {
            if (data != null && data.DiagramObject != null) 
                data.DiagramObject.SetActive(false);
        }
    }

    private void _SetButtonPrintActive(bool obj)
    {
        BlockPrinterBtn(obj);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ChangeDiagram(true);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            ChangeDiagram(false);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            PrinterBtn();
        }
    }

    private void SetDiagramBDActive()
    {
        if (_currentDiagramBD != null)
            _currentDiagramBD.SetActive(false);

        if (_diagramDataList.Count > 0 && _currentDiagram < _diagramDataList.Count)
        {
            _currentDiagramBD = _diagramDataList[_currentDiagram].DiagramBD;
            
            if (_currentDiagramBD != null)
                _currentDiagramBD.SetActive(true);

            if (_diagramName != null && _diagramDataList[_currentDiagram].Diagram != null)
            {
                _diagramName.text = _diagramDataList[_currentDiagram].Diagram.name;
            }
        }
    }

    public void ChangeDiagramInt(int value)
    {
        if (_diagramDataList.Count == 0) return;
        _currentDiagram = value;
        SetDiagramBDActive();
    }

    public void ChangeDiagram(bool left)
    {
        if (_diagramDataList.Count == 0) return;

        _currentDiagram = (_currentDiagram + (left ? -1 : 1) + _diagramDataList.Count) % _diagramDataList.Count;
        SetDiagramBDActive();
    }

    public void AddNewDiagramData(DiagramDataBP newData)
    {
        _diagramDataList.Add(newData);
        SetDiagramBDActive();
    }

    public void SpawnCurrentDiagramToCamera()
    {
        if (_diagramDataList.Count == 0 || _currentDiagram >= _diagramDataList.Count) return;

        DiagramDataBP currentData = _diagramDataList[_currentDiagram];
        if (currentData == null || currentData.Diagram == null) return;

        if (_lastSpawnedDiagram != null)
        {
            Destroy(_lastSpawnedDiagram);
        }

        Vector3 spawnPos = _spawnStartPoint != null ? _spawnStartPoint.position : 
                          (_cameraTransform != null ? _cameraTransform.position : transform.position);

        PartsScript spawnedPart = _diagramManager.SetDiagram(currentData.Diagram, spawnPos);
        
        if (spawnedPart != null)
        {
            _lastSpawnedDiagram = spawnedPart.transform.parent.gameObject;
            spawnedPart.SetHierarchyLayerAndPhysics("Mask", true);
        }
    }

    public void PrinterBtn()
    {
        if (_diagramDataList.Count > 0)
        {
            _printerManager.Printer(GetObjectList());
        }
    }

    private GameObject GetObjectList()
    {
        return _diagramDataList[_currentDiagram].DiagramObject;
    }

    public void BlockPrinterBtn(bool block)
    {
        if (_printerBtn != null)
        {
            var interactable = _printerBtn.GetComponent<RayInteractable>();
            if (interactable != null) interactable.enabled = !block;
            
            if (_painelUi != null)
            {
                _painelUi.BlockButtonColor(_printerBtn.GetComponent<Image>(), block);
            }
        }
    }
}