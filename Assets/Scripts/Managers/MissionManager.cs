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

    [Header("Roteiros do Drone")]
    [SerializeField] private List<MissionDialogue> _missionDialogues;

    [Header("Gabaritos (Scriptables)")]
    [SerializeField] private DiagramScriptableObject _bracoDiagram;
    [SerializeField] private DiagramScriptableObject _headDiagram;
    [SerializeField] private DiagramScriptableObject _furbotDiagram;

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

        SetMission(MissionState.Menu);
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

    public void DiagramValidate(GameObject scannedObj)
    {
        if (_currentMission != MissionState.Mission2) return;

        GameObject container = scannedObj;
        if (scannedObj.transform.parent != null)
        {
            container = scannedObj.transform.parent.gameObject;
        }

        if (_currentStep == 1)
        {
            if (ValidateAssembly(container, _bracoDiagram))
            {
                Destroy(scannedObj);
                ForceDiagramTabAndAdvance();
                NextStep();
            }
            else
            {
                WrongAssemblyScanned(container);
            }
        }
        else if (_currentStep == 2)
        {
            if (ValidateAssembly(container, _headDiagram))
            {
                Destroy(container);
                ForceDiagramTabAndAdvance();
                SetMission(MissionState.Mission3);
            }
            else
            {
                WrongAssemblyScanned(container);
            }
        }
        else if (_currentStep == 3)
        {
            if (ValidateAssembly(container, _furbotDiagram))
            {
                Destroy(container);
                ForceDiagramTabAndAdvance();
            }
            else
            {
                WrongAssemblyScanned(container);
            }
        }
    }

    public bool ValidateAssembly(GameObject scannedObj, DiagramScriptableObject expectedDiagram)
    {
        if (scannedObj == null)
        {
            Debug.LogError("DEBUG MISSION: O objeto escaneado é nulo!");
            return false;
        }

        if (expectedDiagram == null)
        {
            Debug.LogError("DEBUG MISSION: ERRO GRAVE! O DiagramScriptableObject está vazio. Arraste ele para o Inspector do MissionManager!");
            return false;
        }

        Debug.Log("DEBUG MISSION: ----- INICIANDO VALIDAÇÃO -----");
        Debug.Log("DEBUG MISSION: Assinatura Esperada (Gabarito): '" + expectedDiagram.signature + "'");

        DiagramRegister[] registers = scannedObj.GetComponentsInChildren<DiagramRegister>();

        foreach (var reg in registers)
        {
            string scannedSignature = reg.GetLocalSignature();
            Debug.Log("DEBUG MISSION: Assinatura Lida na peça " + reg.gameObject.name + ": '" + scannedSignature + "'");

            if (scannedSignature == expectedDiagram.signature)
            {
                Debug.Log("DEBUG MISSION: SUCESSO! A assinatura bateu!");
                return true;
            }
        }

        Debug.LogWarning("DEBUG MISSION: FALHOU. Nenhuma assinatura lida bateu com o gabarito.");
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
    }

    private void WrongAssemblyScanned(GameObject wrongObj)
    {
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
        yield return new WaitForSeconds(3f);
        string[] linesToSpeak = GetDialoguesForMission(_currentMission, _currentStep);
        _s223Manager.StartDroneRoutine(this, _currentMission, _currentStep, linesToSpeak);
    }

    public int GetStep()
    {
        return _currentStep;
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