using System.Collections;
using UnityEngine;

public class S223Manager : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private s223Dialogue _dialogueSystem;

    [SerializeField] private GameObject _fakeScanner;
    [SerializeField] private GameObject _realScanner;

    private MissionManager _missionManager;
    private MissionState _currentMission;
    private string[] _currentLines;
    private bool _isAnimationFinished = false;

    private void Awake()
    {
        if (_dialogueSystem == null)
        {
            _dialogueSystem = GetComponent<s223Dialogue>();
        }
    }

    private void Start()
    {
        _dialogueSystem.HideDialogue();
        if (_fakeScanner != null) _fakeScanner.SetActive(false);
        if (_realScanner != null) _realScanner.SetActive(false);
    }

    public void StartDroneRoutine(MissionManager manager, MissionState mission, int step, string[] lines)
    {
        _missionManager = manager;
        _currentMission = mission;
        _currentLines = lines;

        if (_dialogueSystem != null) _dialogueSystem.HideDialogue();

        StopAllCoroutines();
        StartCoroutine(ExecuteRoutine(mission, step));
    }

    private IEnumerator ExecuteRoutine(MissionState mission, int step)
    {
        if (mission == MissionState.Mission1)
        {
            if (step == 0)
            {
                if (_animator != null)
                {
                    _isAnimationFinished = false;
                    _animator.SetTrigger("MoveMission1");
                    yield return new WaitUntil(() => _isAnimationFinished);
                }

                if (_currentLines != null && _currentLines.Length > 0 && _dialogueSystem != null)
                {
                    yield return StartCoroutine(_dialogueSystem.PlayDialogueRoutine(_currentLines));
                }
            }
            else if (step == 1)
            {
                if (_currentLines != null && _currentLines.Length > 0 && _dialogueSystem != null)
                {
                    yield return StartCoroutine(_dialogueSystem.PlayDialogueRoutine(_currentLines));
                }

                if (_animator != null)
                {
                    if (_fakeScanner != null) _fakeScanner.SetActive(true);
                    if (_realScanner != null) _realScanner.SetActive(false);

                    _isAnimationFinished = false;
                    _animator.SetTrigger("MoveMission1step2");

                    yield return new WaitUntil(() => _isAnimationFinished);
                }
            }
            else if (step == 2)
            {
                if (_currentLines != null && _currentLines.Length > 0 && _dialogueSystem != null)
                {
                    yield return StartCoroutine(_dialogueSystem.PlayDialogueRoutine(_currentLines));
                    _missionManager.SetMission(MissionState.Mission2);
                }
                yield break;
            }
        }
        else if (mission == MissionState.Mission2)
        {
                yield return StartCoroutine(_dialogueSystem.PlayDialogueRoutine(_currentLines));
        }
        else if (mission == MissionState.FreeMode)
        {
            if (_animator != null)
            {
                _animator.SetTrigger("MoveFreeMode");
            }

            if (_currentLines != null && _currentLines.Length > 0 && _dialogueSystem != null)
            {
                yield return StartCoroutine(_dialogueSystem.PlayDialogueRoutine(_currentLines));
            }
        }

        if (_missionManager != null)
        {
            _missionManager.StartGameplay();
        }
    }

    public void SwapScanners()
    {
        if (_fakeScanner != null) _fakeScanner.SetActive(false);
        if (_realScanner != null) _realScanner.SetActive(true);
    }

    public void OnAnimationStepComplete()
    {
        _isAnimationFinished = true;
    }
}