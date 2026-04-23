using System;
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
        } else if(_currentMission == MissionState.Mission2)
        {
            
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

    public void ValidateMission2Scan(GameObject scannedObj)
    {
        if (_currentMission != MissionState.Mission2 || !_isGameplayActive) return;

        GameObject container = scannedObj;
        if (scannedObj.transform.parent != null)
        {
            container = scannedObj.transform.parent.gameObject;
        }

        if (_currentStep == 0)
        {
            if (IsCorrectArmAssembly(container))
            {
                Destroy(container);
                ForceDiagramTabAndAdvance();
            }
            else
            {
                WrongAssemblyScanned(container);
            }
        }
        else if (_currentStep == 1)
        {
            if (IsCorrectHeadAssembly(container))
            {
                Destroy(container);
                ForceDiagramTabAndAdvance();
            }
            else
            {
                WrongAssemblyScanned(container);
            }
        }
        else if (_currentStep == 2)
        {
            if (IsCorrectFurbotAssembly(container))
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
            string[] errorLine = new string[] { "Erro detectado. Montagem incorreta. Objeto destruido." };
            _s223Manager.StartDroneRoutine(this, _currentMission, 99, errorLine);
        }
    }

    private bool IsCorrectArmAssembly(GameObject obj)
    {
        PartsScript[] parts = obj.GetComponentsInChildren<PartsScript>();
        if (parts.Length != 2) return false;

        bool hasArm = false;
        bool hasHand = false;

        foreach (var p in parts)
        {
            string pName = p.gameObject.name.ToLower();
            if (pName.Contains("braco") || pName.Contains("bra�o")) hasArm = true;
            if (pName.Contains("mao") || pName.Contains("m�o")) hasHand = true;
        }

        return hasArm && hasHand;
    }

    private bool IsCorrectHeadAssembly(GameObject obj)
    {
        PartsScript[] parts = obj.GetComponentsInChildren<PartsScript>();
        if (parts.Length != 4) return false;

        bool hasHead = false;
        int eyeCount = 0;
        bool hasMouth = false;

        foreach (var p in parts)
        {
            string pName = p.gameObject.name.ToLower();
            if (pName.Contains("cabeca") || pName.Contains("cabe�a")) hasHead = true;
            if (pName.Contains("olho")) eyeCount++;
            if (pName.Contains("boca")) hasMouth = true;
        }

        return hasHead && hasMouth && (eyeCount == 2);
    }

    private bool IsCorrectFurbotAssembly(GameObject obj)
    {
        PartsScript[] parts = obj.GetComponentsInChildren<PartsScript>();

        int diagramArms = 0;
        int diagramHeads = 0;

        foreach (var p in parts)
        {
            string pName = p.gameObject.name.ToLower();
            if (pName.Contains("diagram"))
            {
                if (pName.Contains("braco") || pName.Contains("bra�o")) diagramArms++;
                if (pName.Contains("cabeca") || pName.Contains("cabe�a")) diagramHeads++;
            }
        }

        return diagramArms >= 2 && diagramHeads >= 1;
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