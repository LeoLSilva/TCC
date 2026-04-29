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

    [Header("Objetos da Missao")]
    [SerializeField] private GameObject _scannerObject;
    [SerializeField] private GameObject _menuButton;
    [SerializeField] private GameObject _algCanva;

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

        SetMission(MissionState.Menu);
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

        if (_currentMission == MissionState.EndGame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (_currentMission == MissionState.Mission1 && _currentStep == 1)
        {
            if (_scannerObject != null)
            {
                _scannerObject.SetActive(true);
            }
        }
        else if (_currentMission == MissionState.Mission3 && _currentStep == 0)
        {
            PartsScreen partsScreen = FindAnyObjectByType<PartsScreen>();
            if (partsScreen != null)
            {
                partsScreen.ForceUnlockAllParts();
            }
        }
    }

    public void NextStep()
    {
        _currentStep++;

        if (_s223Manager != null)
        {
            string[] linesToSpeak = GetDialoguesForMission(_currentMission, _currentStep);
            if (linesToSpeak != null && linesToSpeak.Length > 0)
            {
                _isGameplayActive = false;
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

    public void ActiveAlgoritm(bool active)
    {
        if (_algCanva != null)
        {
            _algCanva.SetActive(active);
        }
    }

    public void DiagramValidate(GameObject scannedObj)
    {
        ValidateCurrentMissionScan(scannedObj);
    }

    public void ValidateCurrentMissionScan(GameObject scannedObj)
    {
        if ((_currentMission != MissionState.Mission2 && _currentMission != MissionState.Mission3) || !_isGameplayActive) return;

        GameObject container = scannedObj;
        if (scannedObj.transform.parent != null)
        {
            container = scannedObj.transform.parent.gameObject;
        }

        DiagramScriptableObject expectedDiagram = null;
        if (_currentMission == MissionState.Mission2)
        {
            if (_currentStep == 1) expectedDiagram = _armDiagram;
            else if (_currentStep == 2) expectedDiagram = _headDiagram;
        }
        else if (_currentMission == MissionState.Mission3)
        {
            expectedDiagram = _furbotDiagram;
        }

        if (expectedDiagram != null && ValidateAssembly(container, expectedDiagram))
        {
            container.SetActive(false);
            Destroy(container);

            if (_currentMission == MissionState.Mission2)
            {
                if (_currentStep == 1)
                {
                    ForceDiagramTabAndAdvance();
                }
                else if (_currentStep == 2)
                {
                    PainelUI painel = FindAnyObjectByType<PainelUI>();
                    if (painel != null)
                    {
                        painel.SelectTab(1);
                    }

                    SetMission(MissionState.Mission3);
                }
            }
            else if (_currentMission == MissionState.Mission3)
            {
                Debug.Log("Final");
                SetMission(MissionState.EndGame);
            }
        }
        else
        {
            Debug.Log("Saiu");
            WrongAssemblyScanned(container);
        }
    }

    public bool ValidateAssembly(GameObject scannedObj, DiagramScriptableObject expectedDiagram)
    {
        if (scannedObj == null)
        {
            Debug.LogError("[DETETIVE] O objeto escaneado sumiu antes de ser validado!");
            return false;
        }

        if (expectedDiagram == null)
        {
            Debug.LogError("[DETETIVE] CULPADO ENCONTRADO: O Gabarito esperado está NULO! O slot no Inspector do MissionManager está vazio!");
            return false;
        }

        DiagramRegister[] registers = scannedObj.GetComponentsInChildren<DiagramRegister>();

        string expectedSig = expectedDiagram.signature.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();

        foreach (var reg in registers)
        {
            string localSig = reg.GetLocalSignature().Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();

            if (localSig == expectedSig)
            {
                return true;
            }
            else
            {
                Debug.Log($"[DETETIVE] Lendo peça: {reg.gameObject.name}\nTamanho Esperado: {expectedSig.Length} | Tamanho Gerado: {localSig.Length}");

                int minLength = Mathf.Min(expectedSig.Length, localSig.Length);
                for (int i = 0; i < minLength; i++)
                {
                    if (expectedSig[i] != localSig[i])
                    {
                        Debug.LogWarning($"[DETETIVE] Diferença exata no caractere {i}! Esperado a letra '{expectedSig[i]}' mas veio a letra '{localSig[i]}'");
                        break;
                    }
                }
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
        wrongObj.SetActive(false);
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
        if (linesToSpeak != null && linesToSpeak.Length > 0)
        {
            _s223Manager.StartDroneRoutine(this, _currentMission, _currentStep, linesToSpeak);
        }
        else
        {
            StartGameplay();
        }
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

        OnMissionChanged?.Invoke(_currentMission);

        PainelUI painel = FindAnyObjectByType<PainelUI>();
        if (painel != null)
        {
            painel.SelectTab(1);
        }

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