using Oculus.Interaction;
using System.Collections;
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

    [Header("Image Display")]
    [SerializeField] private Image _diagramImageDisplay;

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
    private int _scannedDiagramsCount = 0;

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
            mm.OnGameplayActiveChanged -= HandleGameplayActive;
            mm.OnGameplayActiveChanged += HandleGameplayActive;
            SetupMission(mm.GetMissionState());
        }
    }

    private void OnDisable()
    {
        MissionManager mm = FindAnyObjectByType<MissionManager>();
        if (mm != null)
        {
            mm.OnMissionChanged -= SetupMission;
            mm.OnGameplayActiveChanged -= HandleGameplayActive;
        }
    }

    private void SetupMission(MissionState state)
    {
        if (state == MissionState.FreeMode)
        {
            _isMission2Locked = false;
            _unlockedDiagramsCount = _diagramDataList.Count;
            _scannedDiagramsCount = _diagramDataList.Count;
        }
        else if (state == MissionState.Mission3)
        {
            _isMission2Locked = false;
            _unlockedDiagramsCount = Mathf.Min(3, _diagramDataList.Count);
            _scannedDiagramsCount = 2;
            _currentDiagram = _unlockedDiagramsCount - 1;
        }
        else if (state != MissionState.Disabled && state != MissionState.Menu)
        {
            _isMission2Locked = true;
            BlockPrinterBtn(true);

            if (state == MissionState.Mission1)
            {
                _unlockedDiagramsCount = 1;
                _scannedDiagramsCount = 0;
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
    }

    private void HandleGameplayActive(bool isActive)
    {
        if (!_isMission2Locked)
        {
            bool isScanned = _currentDiagram < _scannedDiagramsCount;
            BlockPrinterBtn(!isActive || !isScanned);
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

        if (Input.GetKeyDown(KeyCode.I))
        {
            SpawnCurrentDiagramToCamera();
        }
    }

    public void UnlockAndAdvance()
    {
        if (_scannedDiagramsCount < _diagramDataList.Count)
        {
            _scannedDiagramsCount++;
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

        if (_painelUi != null)
        {
            _painelUi.SelectTab(1);
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

            DiagramScriptableObject currentSO = _diagramDataList[_currentDiagram].GetDiagram();
            if (currentSO != null)
            {
                if (_diagramName != null)
                {
                    _diagramName.text = currentSO.name;
                }

                bool isScanned = _currentDiagram < _scannedDiagramsCount;

                if (_diagramImageDisplay != null)
                {
                    _diagramImageDisplay.gameObject.SetActive(isScanned);

                    if (isScanned)
                    {
                        _diagramImageDisplay.sprite = currentSO.ImgForAlgoritm != null ? currentSO.ImgForAlgoritm : currentSO.diagramImage;
                    }
                }

                if (!_isMission2Locked)
                {
                    MissionManager mm = FindAnyObjectByType<MissionManager>();
                    bool isActive = mm != null && mm.IsGameplayActive();
                    BlockPrinterBtn(!isActive || !isScanned);
                }
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
            DiagramRegister reg = spawnedPart.GetComponent<DiagramRegister>();
            if (reg != null)
            {
                reg.InjectSavedData(currentData.GetDiagram().rootPart);
            }

            _lastSpawnedDiagram = spawnedPart.transform.parent.gameObject;
            StartCoroutine(DelayPhysicsRoutine(spawnedPart));
        }
    }

    private IEnumerator DelayPhysicsRoutine(PartsScript part)
    {
        yield return new WaitForEndOfFrame();
        if (part != null)
        {
            part.SetHierarchyLayerAndPhysics("Mask", true);
        }
    }

    public void PrinterBtn()
    {
        MissionManager missionManager = FindAnyObjectByType<MissionManager>();
        if (missionManager != null && !missionManager.IsGameplayActive()) return;

        if (_isMission2Locked) return;

        if (_currentDiagram >= _scannedDiagramsCount) return;

        if (_diagramDataList.Count > 0)
        {
            DiagramScriptableObject currentDiagramSO = _diagramDataList[_currentDiagram].GetDiagram();

            if (currentDiagramSO != null)
            {
                _printerManager.PrinterDiagram(currentDiagramSO);

                AlgoritmCreater algoritmCreater = FindAnyObjectByType<AlgoritmCreater>();

                if (missionManager != null && algoritmCreater != null && missionManager.GetMissionState() == MissionState.Mission3)
                {
                    algoritmCreater.RegisterPrint(currentDiagramSO.ImgForAlgoritm);
                }
            }
        }
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