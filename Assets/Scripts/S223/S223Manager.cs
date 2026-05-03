using System.Collections;
using UnityEngine;

public class S223Manager : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private s223Dialogue _dialogueSystem;

    [SerializeField] private GameObject _fakeScanner;
    [SerializeField] private GameObject _realScanner;

    [SerializeField] private float _lookThreshold = 0.85f;

    private MissionManager _missionManager;
    private MissionState _currentMission;
    private string[] _currentLines;
    private bool _isAnimationFinished = false;
    private Transform _playerCamera;

    private void Awake()
    {
        if (_dialogueSystem == null)
        {
            _dialogueSystem = GetComponent<s223Dialogue>();
        }
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            _playerCamera = Camera.main.transform;
        }

        if (_dialogueSystem != null) _dialogueSystem.HideDialogue();

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
        if (mission == MissionState.Mission1 && step == 0)
        {
            yield return PlayAnimationAndWait("MoveMission1");
        }
        else if (mission == MissionState.FreeMode)
        {
            if (_animator != null) _animator.SetTrigger("MoveFreeMode");
        }

        yield return StartCoroutine(PlayDialogueWithDelay(3f));

        if (mission == MissionState.Mission1)
        {
            if (step == 1)
            {
                if (_fakeScanner != null) _fakeScanner.SetActive(true);
                if (_realScanner != null) _realScanner.SetActive(false);

                yield return PlayAnimationAndWait("MoveMission1step2");
            }
            else if (step == 2)
            {
                _missionManager.SetMission(MissionState.Mission2);
                yield break;
            }
        }
        else if (mission == MissionState.Mission3)
        {
            if (step == 0)
            {
                _missionManager.ActiveAlgoritm(true);
            }
            else
            {
                _missionManager.GoMenu();
                yield break;
            }
        }
        else if (mission == MissionState.EndGame)
        {
            _missionManager.EndMission();
            yield break;
        }

        if (_missionManager != null)
        {
            _missionManager.StartGameplay();
        }
    }

    private IEnumerator PlayDialogueWithDelay(float maxDelaySeconds)
    {
        float timer = 0f;
        float lookTimer = 0f;

        while (timer < maxDelaySeconds)
        {
            if (_playerCamera != null)
            {
                Vector3 dirToDrone = (transform.position - _playerCamera.position).normalized;
                float dotProduct = Vector3.Dot(_playerCamera.forward, dirToDrone);

                if (dotProduct >= _lookThreshold)
                {
                    lookTimer += Time.deltaTime;
                    if (lookTimer >= 1f)
                    {
                        break;
                    }
                }
                else
                {
                    lookTimer = 0f;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (_currentLines != null && _currentLines.Length > 0 && _dialogueSystem != null)
        {
            yield return StartCoroutine(_dialogueSystem.PlayDialogueRoutine(_currentLines));
        }
    }

    private IEnumerator PlayAnimationAndWait(string triggerName)
    {
        if (_animator != null)
        {
            _isAnimationFinished = false;
            _animator.SetTrigger(triggerName);
            yield return new WaitUntil(() => _isAnimationFinished);
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