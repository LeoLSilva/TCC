using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    [SerializeField] private MissionState _currentMission = MissionState.Disabled;
    [SerializeField] private int _currentStep = 0;
    [SerializeField] private S223Manager _s223Manager;

    [SerializeField] private AudioSource _s223Audio;

    [Header("Objetos da Missao")]
    [SerializeField] private GameObject _scannerObject;
    [SerializeField] private GameObject _menuButton;
    [SerializeField] private GameObject _algCanva;

    [Header("Screen")]
    [SerializeField] private DiagramScreen _diagramScreen;
    [SerializeField] private PartsScreen _partsScreen;

    [Header("Roteiros do Drone")]
    [SerializeField] private List<MissionDialogue> _missionDialogues;

    [Header("Gabaritos (Scriptables)")]
    [SerializeField] private DiagramScriptableObject _armDiagram;
    [SerializeField] private DiagramScriptableObject _headDiagram;
    [SerializeField] private DiagramScriptableObject _furbotDiagram;



    private bool _isGameplayActive = false;

    public event Action<MissionState> OnMissionChanged;
    public event Action<MissionState, int> OnStepChanged;
    public event Action<bool> OnGameplayActiveChanged;

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

    private void Update()
    {
         if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartFreeMode();
        }
    }

    public void StartFirstMission()
    {
        if (_menuButton != null) _menuButton.SetActive(false);
        SetMission(MissionState.Mission1);
    }

    public void StartFreeMode()
    {
        if (_menuButton != null) _menuButton.SetActive(false);
        SetMission(MissionState.FreeMode);
    }

    public void SetMission(MissionState mission)
    {
        if (_currentMission != mission)
        {
            if (mission != MissionState.Mission1 && mission != MissionState.Menu && mission != MissionState.Disabled)
            {
                ClearAllPartsAndContainers();
            }
            if(mission == MissionState.FreeMode)
            {
                ConfigFreeMode();
            }

            _currentMission = mission;
            _currentStep = 0;

            SetGameplayActive(false);
            OnMissionChanged?.Invoke(_currentMission);
            OnStepChanged?.Invoke(_currentMission, _currentStep);

            if (_s223Manager != null && mission != MissionState.Menu)
            {
                string[] linesToSpeak = GetDialoguesForMission(_currentMission, _currentStep);
                if (linesToSpeak != null && linesToSpeak.Length > 0)
                {
                    _s223Manager.StartDroneRoutine(this, _currentMission, _currentStep, linesToSpeak);
                }
                else
                {
                    StartGameplay();
                }
            }
            else
            {
                StartGameplay();
            }
        }
    }

    private void ConfigFreeMode()
    {
        ClearAllPartsAndContainers();
        SetGameplayActive(true);
        _partsScreen.ForceUnlockAllParts();
        _diagramScreen.SetFreeMode();
        _scannerObject.SetActive(true);
    }

    private void ClearAllPartsAndContainers()
    {
        PartsScript[] allParts = FindObjectsByType<PartsScript>(FindObjectsSortMode.None);
        List<GameObject> objectsToDestroy = new List<GameObject>();

        foreach (var p in allParts)
        {
            if (p != null && p.gameObject != null)
            {
                GameObject rootObj = p.gameObject;
                
                if (p.transform.parent != null && p.transform.parent.name.Contains("DiagramContainer"))
                {
                    rootObj = p.transform.parent.gameObject;
                }

                if (!objectsToDestroy.Contains(rootObj))
                {
                    objectsToDestroy.Add(rootObj);
                }
            }
        }

        foreach (var obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }

    public void NextStep()
    {

        //_audioSource.Play();
        _currentStep++;
        SetGameplayActive(false);
        OnStepChanged?.Invoke(_currentMission, _currentStep);

        if (_s223Manager != null)
        {
            string[] linesToSpeak = GetDialoguesForMission(_currentMission, _currentStep);
            if (linesToSpeak != null && linesToSpeak.Length > 0)
            {
                _s223Manager.StartDroneRoutine(this, _currentMission, _currentStep, linesToSpeak);
            }
            else
            {
                StartGameplay();
            }
        }
        else
        {
            StartGameplay();
        }
    }

    public void StartGameplay()
    {
        SetGameplayActive(true);

        if (_currentMission == MissionState.EndGame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (_currentMission == MissionState.Mission1 && _currentStep == 1)
        {
            if (_scannerObject != null) _scannerObject.SetActive(true);
        }
    }

    public void EndMission()
    {
        SetGameplayActive(false);
        SceneManager.LoadScene(0);
    }

    private void SetGameplayActive(bool isActive)
    {
        _isGameplayActive = isActive;
        OnGameplayActiveChanged?.Invoke(_isGameplayActive);
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

    public void ValidateCurrentMissionScan(GameObject scannedObj)
    {
        if ((_currentMission != MissionState.Mission2 && _currentMission != MissionState.Mission3) || !_isGameplayActive) return;

        GameObject container = scannedObj;
        if (scannedObj.transform.parent != null)
        {
            container = scannedObj.transform.parent.gameObject;
        }

        DiagramScriptableObject expectedDiagram = GetCurrentExpectedDiagram();

        if (expectedDiagram != null && ValidateAssembly(container, expectedDiagram))
        {
            if (_diagramScreen != null)
            {
                _diagramScreen.UnlockAndAdvance();
            }

            container.SetActive(false);
            Destroy(container);

            if (_currentMission == MissionState.Mission2)
            {
                if (_currentStep == 1)
                {
                    NextStep();
                }
                else if (_currentStep == 2)
                {
                    SetMission(MissionState.Mission3);
                }
            }
            else if (_currentMission == MissionState.Mission3)
            {
                SetMission(MissionState.EndGame);
            }
        }
        else
        {
            WrongAssemblyScanned(container, expectedDiagram);
        }
    }

    public bool ValidateAssembly(GameObject scannedObj, DiagramScriptableObject expectedDiagram)
    {
        if (scannedObj == null || expectedDiagram == null) return false;

        DiagramRegister[] registers = scannedObj.GetComponentsInChildren<DiagramRegister>();
        string expectedSig = expectedDiagram.signature.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();

        foreach (var reg in registers)
        {
            string localSig = reg.GetLocalSignature().Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
            if (localSig == expectedSig) return true;
        }
        return false;
    }

    private void WrongAssemblyScanned(GameObject wrongObj, DiagramScriptableObject expectedDiagram)
    {
        SetGameplayActive(false);

        if (_s223Manager != null)
        {
            StartCoroutine(ShowErrorAndRestoreRoutine(expectedDiagram));
        }
    }

    private IEnumerator ShowErrorAndRestoreRoutine(DiagramScriptableObject expectedDiagram)
    {
        string expectedName = expectedDiagram != null ? expectedDiagram.name : "pe�a";
        string[] errorLine = new string[] { $"Ops, n�o parece ser o {expectedName}." };
        _s223Manager.StartDroneRoutine(this, _currentMission, 99, errorLine);

        yield return new WaitForSeconds(5f);

        string[] linesToSpeak = GetDialoguesForMission(_currentMission, _currentStep);
        if (linesToSpeak != null && linesToSpeak.Length > 0)
        {
            _s223Manager.StartDroneRoutine(this, _currentMission, _currentStep, linesToSpeak);
        }
        else
        {
            StartGameplay();
        }
    }

    private void ForcarMissao3Liberada()
    {
        SetMission(MissionState.Mission3);
        StartGameplay();
    }

    public void ActiveAlgoritm(bool active)
    {
        if (_algCanva != null) _algCanva.SetActive(active);
    }

    public bool CanConnectParts() => _currentMission != MissionState.Mission1;
    public MissionState GetMissionState() => _currentMission;
    public bool IsGameplayActive() => _isGameplayActive;
    public int GetStep() => _currentStep;

    public DiagramScriptableObject GetCurrentExpectedDiagram()
    {
        if (_currentMission == MissionState.Mission2)
        {
            if (_currentStep == 1) return _armDiagram;
            if (_currentStep == 2) return _headDiagram;
        }
        else if (_currentMission == MissionState.Mission3)
        {
            return _furbotDiagram;
        }
        return null;
    }
}

public enum MissionState
{
    Menu,
    Mission1,
    Mission2,
    Mission3,
    EndGame,
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