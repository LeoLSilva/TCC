using Oculus.Interaction;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartsScreen : MonoBehaviour
{
    [SerializeField] private List<PartsSoloScriptableObject> _parts = new List<PartsSoloScriptableObject>();
    [SerializeField] private GameObject _currentSelected;
    [SerializeField] private Image _diagramImage;

    [SerializeField] private GameObject _btnPrefab;
    [SerializeField] private GameObject _containerBtn;
    [SerializeField] private GameObject _printerBtn;
    [SerializeField] private PainelUI _painelUi;

    private PrinterManager _printerManager;
    private MissionManager _missionManager;

    [Header("Missions")]
    [SerializeField] private int _partsCount = 8;
    [SerializeField] private TextMeshProUGUI _alertTxt;

    private bool _mission2bool = false;

    private void Awake()
    {
        _painelUi = FindAnyObjectByType<PainelUI>();
        _printerManager = FindAnyObjectByType<PrinterManager>();
        _missionManager = FindAnyObjectByType<MissionManager>();
    }

    private void Start()
    {
        if (_printerManager != null)
        {
            _printerManager.OnPrinterStateChanged += BlockPrinterBtn;
        }
    }

    private void OnEnable()
    {
        if (_missionManager != null)
        {
            _missionManager.OnMissionChanged += SetupMission;
        }
    }

    private void OnDisable()
    {
        if (_missionManager != null)
        {
            _missionManager.OnMissionChanged -= SetupMission;
        }
    }

    private void SetupMission(MissionState state)
    {
        if (state == MissionState.Mission1)
        {
            ClearAllButtons();
            _parts.Clear();
            _currentSelected = null;
            if (_diagramImage != null) _diagramImage.gameObject.SetActive(false);
            _printerBtn.gameObject.SetActive(false);
        }
        else if (state == MissionState.Mission2)
        {
            _printerBtn.gameObject.SetActive(true);
            BlockPrinterBtn(false);
        }
    }

    public void ReceiveScannedObject(GameObject scannedObj)
    {
        if (_missionManager != null && _missionManager.GetMissionState() != MissionState.Mission1) return;
        if (scannedObj == null) return;

        ScannablePart scannable = scannedObj.GetComponentInParent<ScannablePart>();

        if (scannable != null && !scannable.alreadyScanned && scannable.partData != null)
        {
            scannable.alreadyScanned = true;
            UnlockPart(scannable.partData);
            Destroy(scannable.gameObject);

            _partsCount--;
            if (_partsCount <= 0)
            {
                if (_missionManager != null)
                {
                    _missionManager.NextStep();
                }
            }
        }
    }

    public void UnlockPart(PartsSoloScriptableObject newPart)
    {
        if (newPart == null) return;

        if (!_parts.Contains(newPart))
        {
            _parts.Add(newPart);
            CreatePartButton(newPart);

            if (_parts.Count == 1)
            {
                ChangeSelected(newPart);
            }
        }
    }

    private void CreatePartButton(PartsSoloScriptableObject part)
    {
        if (_btnPrefab == null || _containerBtn == null) return;

        GameObject btn = Instantiate(_btnPrefab, _containerBtn.transform);
        btn.GetComponent<Image>().sprite = part.select;

        var ev = btn.GetComponent<PointableUnityEventWrapper>();
        if (ev != null)
        {
            ev.WhenSelect.AddListener((_) => ChangeSelected(part));
        }
    }

    private void ClearAllButtons()
    {
        if (_containerBtn == null) return;

        foreach (Transform child in _containerBtn.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ChangeSelected(PartsSoloScriptableObject p)
    {
        _currentSelected = p.prefab;
        if (_diagramImage != null)
        {
            _diagramImage.gameObject.SetActive(true);
            _diagramImage.sprite = p.diagram;
        }
        if (_missionManager.GetMissionState() == MissionState.Mission2 && _missionManager.GetStep() == 0)
        {
            _mission2bool = (_currentSelected.name != "Braco" && _currentSelected.name != "Mao");
            Debug.Log(_mission2bool);
            if (_mission2bool)
            {
                _alertTxt.text = _currentSelected.name + " não pertence ao Braço";
            }
            _alertTxt.gameObject.SetActive(_mission2bool);
            BlockPrinterBtn(_mission2bool);
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

    public void PrinterBtn()
    {
        if (_printerManager != null && _currentSelected != null)
        {
            if(_missionManager.GetMissionState() == MissionState.Mission2 && _missionManager.GetStep() == 0)
            {
                _painelUi.AddItemMission2(_currentSelected.name);
            }
            _printerManager.Printer(_currentSelected);
        }
    }
}