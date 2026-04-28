using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField] private MissionState _currentMission = MissionState.Disabled;
    [SerializeField] private int _currentStep = 0;
    [SerializeField] private S223Manager _s223Manager;

    [Header("Objetos da Missao")]
    [SerializeField] private GameObject _scannerObject;
    [SerializeField] private GameObject _menuButton;
    [SerializeField] private GameObject _activePCanva;

    [Header("Roteiros do Drone")]
    [SerializeField] private List<MissionDialogue> _missionDialogues;

    [Header("Gabaritos (Scriptables)")]
    [SerializeField] private DiagramScriptableObject _armDiagram;
    [SerializeField] private DiagramScriptableObject _headDiagram;
    [SerializeField] private DiagramScriptableObject _furbotDiagram;

    [Header("---- AREA DE TESTE (EXCLUIR DEPOIS) ----")]
    [SerializeField] private bool _iniciarMissao3Liberada = false;

    private bool _isGameplayActive = false;

    public event Action<MissionState> OnMissionChanged;

    private void OnEnable()
    {
        PartsScript.OnPieceDetached += CheckDisassemblyCompletion;
    }

    private void OnDisable()
    {
        PartsScript.OnPieceDetached -= CheckDisassemblyCompletion;
    }

    private void Start()
    {
        if (_scannerObject != null) _scannerObject.SetActive(false);
        if (_menuButton != null) _menuButton.SetActive(true);

        //SetMission(MissionState.Menu);
        ForcarMissao3Liberada();
    }

    private void Update()
    {
        if (_iniciarMissao3Liberada)
        {
            _iniciarMissao3Liberada = false;
            ForcarMissao3Liberada();
        }
    }

    public void StartFirstMission()
    {
        if (_menuButton != null) _menuButton.SetActive(false);
        SetMission(MissionState.Mission1);
    }

    public void SetMission(MissionState mission)
    {
        if (_currentMission != mission)
        {
            _currentMission = mission;
            _currentStep = 0;
            _isGameplayActive = false;
            OnMissionChanged?.Invoke(_currentMission);

            if (_s223Manager != null && mission != MissionState.Menu)
            {
                string[] linesToSpeak = GetDialoguesForMission(_currentMission, _currentStep);
                _s223Manager.StartDroneRoutine(this, _currentMission, _currentStep, linesToSpeak);
            }
            else
            {
                StartGameplay();
            }
        }
    }

    private void CheckDisassemblyCompletion()
    {
        if (_currentMission != MissionState.Mission1 || _currentStep != 0 || !_isGameplayActive) return;

        PartsScript[] allParts = FindObjectsByType<PartsScript>(FindObjectsSortMode.None);
        int pecasSobrando = 0;

        foreach (var p in allParts)
        {
            if (p.GetStatus() != PieceStatus.none)
            {
                pecasSobrando++;
            }
        }

        if (pecasSobrando > 0) return;

        NextStep();
    }

    private string[] GetDialoguesForMission(MissionState state, int step)
    {
        foreach (var md in _missionDialogues)
        {
            if (md.missionState == state && md.missionStep == step)
            {
                return md.lines;
            }
        }
        return new string[0];
    }

    public void StartGameplay()
    {
        _isGameplayActive = true;

        if (_currentMission == MissionState.Mission1 && _currentStep == 1)
        {
            if (_scannerObject != null)
            {
                _scannerObject.SetActive(true);
            }
        }
    }

    public void NextStep()
    {
        _currentStep++;
        _isGameplayActive = false;

        if (_s223Manager != null)
        {
            string[] linesToSpeak = GetDialoguesForMission(_currentMission, _currentStep);
            _s223Manager.StartDroneRoutine(this, _currentMission, _currentStep, linesToSpeak);
        }
        else
        {
            StartGameplay();
        }
    }

    public bool CanConnectParts()
    {
        if (_currentMission == MissionState.Mission1) return false;

        return true;
    }

    public MissionState GetMissionState()
    {
        return _currentMission;
    }

    public bool IsGameplayActive()
    {
        return _isGameplayActive;
    }

    public void EndMission()
    {
        _isGameplayActive = false;
    }

    public DiagramScriptableObject GetCurrentExpectedDiagram()
    {
        if (_currentMission != MissionState.Mission3) return null;

        if (_currentStep == 0) return _armDiagram;
        if (_currentStep == 1) return _headDiagram;
        if (_currentStep == 2) return _furbotDiagram;

        return null;
    }

    public void ActiveAlgoritm(bool active)
    {
        _activePCanva.SetActive(active);
    }

    public void DiagramValidate(GameObject scannedObj)
    {
        ValidateMission2Scan(scannedObj);
    }

    public void ValidateMission2Scan(GameObject scannedObj)
    {
        if (_currentMission != MissionState.Mission2 || !_isGameplayActive) return;

        GameObject container = scannedObj;
        if (scannedObj.transform.parent != null)
        {
            container = scannedObj.transform.parent.gameObject;
        }

        DiagramScriptableObject expectedDiagram = null;
        if (_currentStep == 0) expectedDiagram = _armDiagram;
        else if (_currentStep == 1) expectedDiagram = _headDiagram;
        else if (_currentStep == 2) expectedDiagram = _furbotDiagram;

        if (expectedDiagram != null && ValidateAssembly(container, expectedDiagram))
        {
            Destroy(container);
            ForceDiagramTabAndAdvance();
        }
        else
        {
            WrongAssemblyScanned(container);
        }
    }

    public bool ValidateAssembly(GameObject scannedObj, DiagramScriptableObject expectedDiagram)
    {
        if (scannedObj == null || expectedDiagram == null) return false;

        DiagramRegister[] registers = scannedObj.GetComponentsInChildren<DiagramRegister>();

        foreach (var reg in registers)
        {
            if (reg.GetLocalSignature() == expectedDiagram.signature)
            {
                return true;
            }
        }

        return false;
    }

    private void ForceDiagramTabAndAdvance()
    {
        PainelUI painel = FindAnyObjectByType<PainelUI>();
        if (painel != null)
        {
            painel.SelectTab(1);
        }

        DiagramScreen ds = FindAnyObjectByType<DiagramScreen>();
        if (ds != null)
        {
            ds.UnlockAndAdvance();
        }

        NextStep();
    }

    private void WrongAssemblyScanned(GameObject wrongObj)
    {
        Destroy(wrongObj);
        _isGameplayActive = false;

        if (_s223Manager != null)
        {
            StartCoroutine(ShowErrorAndRestoreRoutine());
        }
    }

    private IEnumerator ShowErrorAndRestoreRoutine()
    {
        string[] errorLine = new string[] { "Erro detectado. Montagem incorreta. Objeto destruido." };
        _s223Manager.StartDroneRoutine(this, _currentMission, 99, errorLine);

        yield return new WaitForSeconds(5f);

        string[] linesToSpeak = GetDialoguesForMission(_currentMission, _currentStep);
        _s223Manager.StartDroneRoutine(this, _currentMission, _currentStep, linesToSpeak);
    }

    public int GetStep()
    {
        return _currentStep;
    }
    private void ForcarMissao3Liberada()
    {
        _currentMission = MissionState.Mission3;
        _currentStep = 0;
        _isGameplayActive = true;

        PartsScreen partsScreen = FindAnyObjectByType<PartsScreen>();
        if (partsScreen != null)
        {
            partsScreen.ForceUnlockAllParts();
        }
    }
}

public enum MissionState
{
    Menu,
    Mission1,
    Mission2,
    Mission3,
    FreeMode,
    Disabled
}

[System.Serializable]
public class MissionDialogue
{
    public MissionState missionState;
    public int missionStep;
    [TextArea(2, 5)]
    public string[] lines;
}