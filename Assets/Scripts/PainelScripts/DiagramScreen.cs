using Oculus.Interaction;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiagramScreen : MonoBehaviour
{
    [SerializeField] private GameObject _printerBtn;
    [SerializeField] private GameObject _btnLeft;
    [SerializeField] private GameObject _btnRight;
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
    private bool _isMission2Locked = false;
    private int _unlockedDiagramsCount = 1;

    private void Start()
    {
        if (_diagramManager == null)
        {
            _diagramManager = FindAnyObjectByType<DiagramManager>();
        }

        _printerManager = FindAnyObjectByType<PrinterManager>();

        if (_printerManager != null)
        {
            _printerManager.OnPrinterStateChanged += _SetButtonPrintActive;
        }

        MissionManager mm = FindAnyObjectByType<MissionManager>();
        if (mm != null)
        {
            SetupMission(mm.GetMissionState());
        }
        else
        {
            SetDiagramBDActive();
            MoveCamera();
        }
    }

    private void OnEnable()
    {
        MissionManager mm = FindAnyObjectByType<MissionManager>();
        if (mm != null)
        {
            mm.OnMissionChanged -= SetupMission;
            mm.OnMissionChanged += SetupMission;
            SetupMission(mm.GetMissionState());
        }
    }

    private void OnDisable()
    {
        MissionManager mm = FindAnyObjectByType<MissionManager>();
        if (mm != null) mm.OnMissionChanged -= SetupMission;

        HideAll3DObjects();
    }

    private void SetupMission(MissionState state)
    {
        if (state == MissionState.FreeMode)
        {
            _isMission2Locked = false;
            _unlockedDiagramsCount = _diagramDataList.Count;
            BlockPrinterBtn(false);
        }
        else if (state == MissionState.Mission3)
        {
            _isMission2Locked = false;
            _unlockedDiagramsCount = Mathf.Min(3, _diagramDataList.Count);
            _currentDiagram = _unlockedDiagramsCount - 1;
            BlockPrinterBtn(false);
        }
        else if (state != MissionState.Disabled && state != MissionState.Menu)
        {
            _isMission2Locked = true;
            BlockPrinterBtn(true);

            if (state == MissionState.Mission1)
            {
                _unlockedDiagramsCount = 1;
                _currentDiagram = 0;
            }
        }
        else
        {
            return;
        }

        SetDiagramBDActive();
        MoveCamera();

        if (_btnLeft != null) _btnLeft.SetActive(_unlockedDiagramsCount > 1);
        if (_btnRight != null) _btnRight.SetActive(_unlockedDiagramsCount > 1);

        HideAll3DObjects();

        int limit = (state == MissionState.FreeMode) ? _unlockedDiagramsCount : (_unlockedDiagramsCount - 1);

        for (int i = 0; i < limit; i++)
        {
            if (i < _diagramDataList.Count)
            {
                if (_diagramDataList[i].GetDiagramObject() == null)
                {
                    SpawnSpecificDiagram(i);
                }
                else
                {
                    _diagramDataList[i].GetDiagramObject().SetActive(true);
                }
            }
        }
    }

    private void HideAll3DObjects()
    {
        foreach (var data in _diagramDataList)
        {
            if (data != null && data.GetDiagramObject() != null)
                data.GetDiagramObject().SetActive(false);
        }
    }

    private void ShowAll3DObjects()
    {
        foreach (var data in _diagramDataList)
        {
            if (data != null && data.GetDiagramObject() != null)
                data.GetDiagramObject().SetActive(true);
        }
    }

    private void _SetButtonPrintActive(bool obj)
    {
        if (!_isMission2Locked)
        {
            BlockPrinterBtn(obj);
        }
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

        if (Input.GetKeyDown(KeyCode.W))
        {
            PrinterBtn();
        }
    }

    private void SpawnAllDiagrams()
    {
        if (_diagramManager == null) return;

        Vector3 startPos = _spawnStartPoint != null ? _spawnStartPoint.position : Vector3.zero;

        for (int i = 0; i < _diagramDataList.Count; i++)
        {
            DiagramDataBP data = _diagramDataList[i];

            if (data != null && data.GetDiagram() != null && data.GetDiagramObject() == null)
            {
                Vector3 spawnPos = startPos;
                spawnPos.x += i * 10f;

                PartsScript spawnedPart = _diagramManager.SetDiagram(data.GetDiagram(), spawnPos);

                if (spawnedPart != null)
                {
                    spawnedPart.SetHierarchyLayerAndPhysics("Mask", true);
                    data.SetDiagramObject(spawnedPart.transform.parent.gameObject);
                }
            }
        }
    }

    public void UnlockAndAdvance()
    {
        int completedIndex = _unlockedDiagramsCount - 1;
        if (completedIndex >= 0 && completedIndex < _diagramDataList.Count)
        {
            SpawnSpecificDiagram(completedIndex);
        }

        if (_unlockedDiagramsCount < _diagramDataList.Count)
        {
            _unlockedDiagramsCount++;
            _currentDiagram = _unlockedDiagramsCount - 1;
        }

        SetDiagramBDActive();
        MoveCamera();

        if (_unlockedDiagramsCount > 1)
        {
            if (_btnLeft != null) _btnLeft.SetActive(true);
            if (_btnRight != null) _btnRight.SetActive(true);
        }
    }

    private void SpawnSpecificDiagram(int index)
    {
        if (index < 0 || index >= _diagramDataList.Count) return;

        DiagramDataBP data = _diagramDataList[index];

        if (data != null && data.GetDiagram() != null && data.GetDiagramObject() == null)
        {
            Vector3 startPos = _spawnStartPoint != null ? _spawnStartPoint.position : Vector3.zero;
            Vector3 spawnPos = startPos;
            spawnPos.x += index * 10f;

            PartsScript spawnedPart = _diagramManager.SetDiagram(data.GetDiagram(), spawnPos);

            if (spawnedPart != null)
            {
                spawnedPart.SetHierarchyLayerAndPhysics("Mask", true);
                data.SetDiagramObject(spawnedPart.transform.parent.gameObject);
                data.GetDiagramObject().SetActive(true);
            }
        }
        else if (data != null && data.GetDiagramObject() != null)
        {
            data.GetDiagramObject().SetActive(true);
        }
    }

    private void SetDiagramBDActive()
    {
        if (_currentDiagramBD != null)
            _currentDiagramBD.SetActive(false);

        if (_diagramDataList.Count > 0 && _currentDiagram < _diagramDataList.Count)
        {
            _currentDiagramBD = _diagramDataList[_currentDiagram].GetDiagramBD();

            if (_currentDiagramBD != null)
                _currentDiagramBD.SetActive(true);

            if (_diagramName != null && _diagramDataList[_currentDiagram].GetDiagram() != null)
            {
                _diagramName.text = _diagramDataList[_currentDiagram].GetDiagram().name;
            }
        }
    }

    public void ChangeDiagramInt(int value)
    {
        if (_unlockedDiagramsCount == 0 || value >= _unlockedDiagramsCount) return;

        _currentDiagram = value;
        SetDiagramBDActive();
        MoveCamera();
    }

    public void ChangeDiagram(bool left)
    {
        if (_unlockedDiagramsCount <= 1) return;

        _currentDiagram = (_currentDiagram + (left ? -1 : 1) + _unlockedDiagramsCount) % _unlockedDiagramsCount;
        SetDiagramBDActive();
        MoveCamera();
    }

    private void MoveCamera()
    {
        if (_cameraTransform != null && _diagramDataList.Count > 0)
        {
            Vector3 camPos = _cameraTransform.position;
            float startX = _spawnStartPoint != null ? _spawnStartPoint.position.x : 0f;
            camPos.x = startX + (_currentDiagram * 10f);
            _cameraTransform.position = camPos;
        }
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
        if (currentData == null || currentData.GetDiagram() == null) return;

        if (_lastSpawnedDiagram != null)
        {
            Destroy(_lastSpawnedDiagram);
        }

        Vector3 spawnPos = _spawnStartPoint != null ? _spawnStartPoint.position :
                          (_cameraTransform != null ? _cameraTransform.position : transform.position);

        PartsScript spawnedPart = _diagramManager.SetDiagram(currentData.GetDiagram(), spawnPos);

        if (spawnedPart != null)
        {
            _lastSpawnedDiagram = spawnedPart.transform.parent.gameObject;
            spawnedPart.SetHierarchyLayerAndPhysics("Mask", true);
        }
    }

    public void PrinterBtn()
    {
        if (_isMission2Locked) return;

        if (_diagramDataList.Count > 0)
        {
            _printerManager.Printer(GetObjectList());

            MissionManager missionManager = FindAnyObjectByType<MissionManager>();
            AlgoritmCreater algoritmCreater = FindAnyObjectByType<AlgoritmCreater>();

            if (missionManager != null && algoritmCreater != null && missionManager.GetMissionState() == MissionState.Mission3)
            {
                DiagramScriptableObject currentDiagramSO = _diagramDataList[_currentDiagram].GetDiagram();

                if (currentDiagramSO != null)
                {
                    algoritmCreater.RegisterPrint(currentDiagramSO.diagramImage);
                }
            }
        }
    }

    private GameObject GetObjectList()
    {
        return _diagramDataList[_currentDiagram].GetDiagramObject();
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