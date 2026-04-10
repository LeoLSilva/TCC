using Oculus.Interaction;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatedDiagramScreen : MonoBehaviour
{
    [SerializeField] private GameObject _printerBtn;
    [SerializeField] private int _currentDiagram;
    [SerializeField] private TextMeshProUGUI _diagramName;

    [SerializeField] private List<DiagramCreaterManager.LoadedDiagram> _diagramsList = new List<DiagramCreaterManager.LoadedDiagram>();
    [SerializeField] private List<GameObject> _gameObjectList = new List<GameObject>();

    [SerializeField] private DiagramCreaterManager _diagramCreaterManager;
    [SerializeField] private DiagramManager _diagramManager;
    [SerializeField] private PrinterManager _printerManager;
    [SerializeField] private PainelUI _painelUi;

    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _spawnStartPoint;

    [Header("UI Optimization")]
    [SerializeField] private GameObject _containerDeDiagramas;

    private int _spawnedCount = 0;

    private void Start()
    {
        _printerManager = FindAnyObjectByType<PrinterManager>();
        _diagramManager = FindAnyObjectByType<DiagramManager>();
        _diagramCreaterManager = FindAnyObjectByType<DiagramCreaterManager>();

        if (_printerManager != null)
            _printerManager.OnPrinterStateChanged += BlockPrinterBtn;

        if (_containerDeDiagramas == null)
        {
            _containerDeDiagramas = new GameObject("Container_CreatedDiagramScreen_3D");
        }
    }

    private void OnEnable()
    {
        if (_containerDeDiagramas != null)
        {
            _containerDeDiagramas.SetActive(true);
        }

        UpdateDiagramList();
        MoveCamera();
    }

    private void OnDisable()
    {
        if (_containerDeDiagramas != null)
        {
            _containerDeDiagramas.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) ChangeDiagram(true);
        else if (Input.GetKeyDown(KeyCode.E)) ChangeDiagram(false);
        else if (Input.GetKeyDown(KeyCode.W)) PrinterBtn();
    }

    public void ChangeDiagram(bool left)
    {
        if (_diagramsList.Count == 0) return;

        _currentDiagram = (_currentDiagram + (left ? -1 : 1) + _diagramsList.Count) % _diagramsList.Count;
        MoveCamera();
    }

    public void UpdateDiagramList()
    {
        if (!gameObject.activeInHierarchy) return;

        if (_diagramCreaterManager != null)
        {
            _diagramCreaterManager.RefreshDiagramList();
            _diagramsList = _diagramCreaterManager.GetLoadedDiagrams();
        }

        if (_diagramsList.Count != _gameObjectList.Count)
        {
            ClearAllDiagrams();
            SpawnAllDiagrams();
        }
    }

    private void ClearAllDiagrams()
    {
        foreach (GameObject obj in _gameObjectList)
        {
            if (obj != null) Destroy(obj);
        }

        _gameObjectList.Clear();
        _spawnedCount = 0;
        _currentDiagram = 0;
    }

    private void SpawnAllDiagrams()
    {
        if (_diagramManager == null)
            _diagramManager = FindAnyObjectByType<DiagramManager>();

        Vector3 startPos = _spawnStartPoint != null ? _spawnStartPoint.position : Vector3.zero;

        for (int i = 0; i < _diagramsList.Count; i++)
        {
            Vector3 spawnPos = startPos;
            spawnPos.x += i * 10f;
            var spawnedObj = _diagramManager.SetDiagramJson(_diagramsList[i].rootNode, spawnPos);

            if (spawnedObj != null)
            {
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
    }

    private void MoveCamera()
    {
        if (_cameraTransform != null && _diagramsList.Count > 0)
        {
            Vector3 camPos = _cameraTransform.position;
            float startX = _spawnStartPoint != null ? _spawnStartPoint.position.x : 0f;
            camPos.x = startX + (_currentDiagram * 10f);
            _cameraTransform.position = camPos;

            if (_diagramName != null)
            {
                _diagramName.text = _diagramsList[_currentDiagram].diagramName;
            }
        }
    }

    public void PrinterBtn()
    {
        if (_gameObjectList.Count > 0)
            _printerManager.Printer(GetObjectList());
    }

    private GameObject GetObjectList()
    {
        if (_currentDiagram >= 0 && _currentDiagram < _gameObjectList.Count)
        {
            return _gameObjectList[_currentDiagram];
        }
        return null;
    }

    public void BlockPrinterBtn(bool block)
    {
        if (_printerBtn != null)
        {
            _printerBtn.GetComponent<RayInteractable>().enabled = !block;
            _painelUi.BlockButtonColor(_printerBtn.GetComponent<Image>(), block);
        }
    }
}