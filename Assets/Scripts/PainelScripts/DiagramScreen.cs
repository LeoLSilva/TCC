using Oculus.Interaction;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiagramScreen : MonoBehaviour
{
    [SerializeField] private GameObject _printerBtn;
    [SerializeField] private int _currentDiagram;
    [SerializeField] private TextMeshProUGUI _diagramName;

    [Header("Lists")]
    [SerializeField] private List<DiagramScriptableObject> _diagramsList = new List<DiagramScriptableObject>();
    [SerializeField] private List<GameObject> _gameObjectList = new List<GameObject>();

    [Header("Other Scripts")]
    [SerializeField] private DiagramManager _diagramManager;
    [SerializeField] private PrinterManager _printerManager;
    [SerializeField] private PainelUI _painelUi;

    [Header("Transforms")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _spawnStartPoint;

    [Header("UI Optimization")]
    [SerializeField] private GameObject _containerDeDiagramas;

    private int _spawnedCount = 0;

    private void Start()
    {
        _printerManager = FindAnyObjectByType<PrinterManager>();
        _diagramManager = FindAnyObjectByType<DiagramManager>();
        _printerManager.OnPrinterStateChanged += _SetButtonPrintActive;

        if (_containerDeDiagramas == null)
        {
            _containerDeDiagramas = new GameObject("Container_DiagramScreen_3D");
        }
    }

    private void OnEnable()
    {
        if (_containerDeDiagramas != null)
        {
            _containerDeDiagramas.SetActive(true);
        }

        if (_diagramsList.Count > 0)
            SpawnAllDiagrams();

        MoveCamera();
    }

    private void OnDisable()
    {
        if (_containerDeDiagramas != null)
        {
            _containerDeDiagramas.SetActive(false);
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

    public void ChangeDiagram(bool left)
    {
        if (_diagramsList.Count == 0) return;

        _currentDiagram = (_currentDiagram + (left ? -1 : 1) + _diagramsList.Count) % _diagramsList.Count;
        MoveCamera();
    }

    private void SpawnAllDiagrams()
    {
        if (_diagramManager == null)
        {
            _diagramManager = FindAnyObjectByType<DiagramManager>();
        }

        Vector3 startPos = _spawnStartPoint != null ? _spawnStartPoint.position : Vector3.zero;

        for (int i = _gameObjectList.Count; i < _diagramsList.Count; i++)
        {
            Vector3 spawnPos = startPos;
            spawnPos.x += i * 10f;

            var spawnedObj = _diagramManager.SetDiagram(_diagramsList[i], spawnPos);
            spawnedObj.SetHierarchyLayerAndPhysics("Mask", true);

            GameObject pastaDoDiagrama = spawnedObj.transform.parent.gameObject;

            if (_containerDeDiagramas != null)
            {
                pastaDoDiagrama.transform.SetParent(_containerDeDiagramas.transform, true);
            }

            _gameObjectList.Add(pastaDoDiagrama);
            _spawnedCount++;
        }
    }

    public void AddNewDiagram(DiagramScriptableObject newDiagram)
    {
        if (_diagramManager == null)
        {
            _diagramManager = FindAnyObjectByType<DiagramManager>();
        }

        _diagramsList.Add(newDiagram);

        Vector3 startPos = _spawnStartPoint != null ? _spawnStartPoint.position : Vector3.zero;
        Vector3 spawnPos = startPos;
        spawnPos.x += _spawnedCount * 10f;

        var spawnedObj = _diagramManager.SetDiagram(newDiagram, spawnPos);
        spawnedObj.SetHierarchyLayerAndPhysics("Mask", true);

        GameObject pastaDoDiagrama = spawnedObj.transform.parent.gameObject;

        if (_containerDeDiagramas != null)
        {
            pastaDoDiagrama.transform.SetParent(_containerDeDiagramas.transform, true);
        }

        _gameObjectList.Add(pastaDoDiagrama);
        _spawnedCount++;
    }

    private void MoveCamera()
    {
        if (_cameraTransform != null)
        {
            Vector3 camPos = _cameraTransform.position;
            float startX = _spawnStartPoint != null ? _spawnStartPoint.position.x : 0f;
            camPos.x = startX + (_currentDiagram * 10f);
            _cameraTransform.position = camPos;
        }
    }

    public void PrinterBtn()
    {
        _printerManager.Printer(GetObjectList());
    }

    private GameObject GetObjectList()
    {
        return _gameObjectList[_currentDiagram];
    }

    public void BlockPrinterBtn(bool block)
    {
        _printerBtn.GetComponent<RayInteractable>().enabled = !block;
        _painelUi.BlockButtonColor(_printerBtn.GetComponent<Image>(), block);
    }
}