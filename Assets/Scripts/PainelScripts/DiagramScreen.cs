using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DiagramScreen : MonoBehaviour
{
    [SerializeField] private int _currentDiagram;
    [SerializeField] private List<DiagramScriptableObject> _diagramsList = new List<DiagramScriptableObject>();
    [SerializeField] private List<GameObject> _gameObjectList = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI _diagramName;
    [SerializeField] private DiagramManager _diagramManager;
    [SerializeField] private PrinterManager _printerManager;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _spawnStartPoint;


    public DiagramScriptableObject _diagramaTeste;
    private int _spawnedCount = 0;

    private void Start()
    {
        _printerManager = FindAnyObjectByType<PrinterManager>();
        _diagramManager = FindAnyObjectByType<DiagramManager>();

        SpawnAllDiagrams();
        MoveCamera();
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

        for (int i = 0; i < _diagramsList.Count; i++)
        {
            Vector3 spawnPos = startPos;
            spawnPos.x += i * 10f;

            var spawnedObj = _diagramManager.SetDiagram(_diagramsList[i], spawnPos);
            spawnedObj.SetHierarchyLayerAndPhysics("Mask", true);
            _gameObjectList.Add(spawnedObj.gameObject);
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
        _gameObjectList.Add(spawnedObj.gameObject);
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
        _printerManager.Printer();
    }

    private DiagramScriptableObject GetObjectList()
    {
        return _diagramsList[_currentDiagram];
    }
}
