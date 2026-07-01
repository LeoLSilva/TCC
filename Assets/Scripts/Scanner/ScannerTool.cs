using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ScannerTool : MonoBehaviour, IHandGrabUseDelegate
{
    [SerializeField] private Transform _trigger;
    [SerializeField] private float _triggerStartZ = -0.07448174f;
    [SerializeField] private float _triggerEndZ = -0.09448174f;
    [SerializeField] private AnimationCurve _triggerMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField][Range(0f, 1f)] private float _releaseThreshold = 0.3f;
    [SerializeField][Range(0f, 1f)] private float _fireThreshold = 0.7f;
    [SerializeField] private float _triggerSpeed = 10f;
    [SerializeField] private AnimationCurve _strengthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField] private float _scanDuration = 3f;
    [SerializeField] private bool _canScan = true;
    [SerializeField] private float _gracePeriod = 0.3f;

    [Header("Raycast Settings")]
    [SerializeField] private Transform _scanOrigin;
    [SerializeField] private float _maxScanDistance = 2f;
    [SerializeField] private float _forgivenessRadius = 0.05f;
    [SerializeField] private LayerMask _scanLayerMask = ~0;
    [SerializeField] private string _targetTag = "obj";
    [SerializeField] private string _ignoreTag = "Ground";


    public UnityEvent<GameObject> OnScanStarted;
    public UnityEvent<float> OnScanProgress;
    public UnityEvent<GameObject> OnScanComplete;
    public UnityEvent OnScanCanceled;

    private float _dampedUseStrength = 0;
    private float _lastUseTime;
    private bool _isScanning = false;
    private float _currentScanTime = 0f;
    private float _timeLostTarget = 0f;
    private bool _scanFinished = false;
    private GameObject _currentTarget;

    [Header("Testes")]
    public GameObject itenScannerText;

    [Header("Dependencias (Arraste no Inspector)")]
    [SerializeField] private PartsScreen _partsScreen;
    [SerializeField] private MissionManager _missionManager;
    private ScanManager _scanManager;

    private void Awake()
    {
        if (_partsScreen == null) _partsScreen = FindAnyObjectByType<PartsScreen>();
        if (_missionManager == null) _missionManager = FindAnyObjectByType<MissionManager>();

        _scanManager = GetComponent<ScanManager>();
    }

    public void BeginUse()
    {
        _dampedUseStrength = 0f;
        _lastUseTime = Time.realtimeSinceStartup;
    }

    public void EndUse()
    {
        CancelScan();
    }

    public float ComputeUseStrength(float strength)
    {
        float delta = Time.realtimeSinceStartup - _lastUseTime;
        _lastUseTime = Time.realtimeSinceStartup;

        if (strength > _dampedUseStrength)
        {
            _dampedUseStrength = Mathf.Lerp(_dampedUseStrength, strength, _triggerSpeed * delta);
        }
        else
        {
            _dampedUseStrength = strength;
        }

        float progress = _strengthCurve.Evaluate(_dampedUseStrength);
        UpdateTriggerPosition(progress);
        ProcessScanning(progress);

        return progress;
    }

    private void UpdateTriggerPosition(float progress)
    {
        if (_trigger == null) return;

        float curveValue = _triggerMoveCurve.Evaluate(progress);
        float targetZ = Mathf.Lerp(_triggerStartZ, _triggerEndZ, curveValue);

        Vector3 localPos = _trigger.localPosition;
        localPos.z = targetZ;
        _trigger.localPosition = localPos;
    }

private void ProcessScanning(float progress)
    {
        if (!_canScan || _scanFinished || (_missionManager != null && !_missionManager.IsGameplayActive())) 
            return;

        if (progress >= _fireThreshold)
        {
            HandleActiveScanning();
        }
        else if (progress <= _releaseThreshold)
        {
            if (_isScanning) CancelScan();
        }
    }

    private void HandleActiveScanning()
    {
        GameObject targetToScan = FindValidTarget();

        if (targetToScan != null)
        {
            UpdateTargetState(targetToScan);
        }
        else
        {
            HandleMissingTarget();
        }
    }

    private GameObject FindValidTarget()
    {
        if (_scanOrigin == null) return null;

        GameObject targetToScan = GetValidTargetFromHits(Physics.RaycastAll(_scanOrigin.position, _scanOrigin.forward, _maxScanDistance, _scanLayerMask));

        if (targetToScan == null && _forgivenessRadius > 0f)
        {
            targetToScan = GetValidTargetFromHits(Physics.SphereCastAll(_scanOrigin.position, _forgivenessRadius, _scanOrigin.forward, _maxScanDistance, _scanLayerMask));
        }

        return targetToScan;
    }

    private GameObject GetValidTargetFromHits(RaycastHit[] hits)
    {
        if (hits.Length == 0) return null;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool isMission2Or3 = _missionManager != null && 
            (_missionManager.GetMissionState() == MissionState.Mission2 || _missionManager.GetMissionState() == MissionState.Mission3);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag(_ignoreTag)) continue;

            if (hit.collider.CompareTag(_targetTag))
            {
                GameObject hitObj = hit.collider.gameObject;

                if (isMission2Or3)
                {
                    if (hitObj.transform.parent != null)
                    {
                        if (hitObj.transform.parent.GetComponentsInChildren<PartsScript>().Length > 1)
                        {
                            return hitObj.transform.parent.gameObject;
                        }
                    }
                    return null; 
                }

                return hitObj;
            }

            break;
        }

        return null;
    }

    private void UpdateTargetState(GameObject targetToScan)
    {
        if (!_isScanning)
        {
            _isScanning = true;
            _currentTarget = targetToScan;
            _currentScanTime = 0f;
            _timeLostTarget = 0f;

            OnScanStarted?.Invoke(_currentTarget);

            if (_scanManager != null) _scanManager.StartScanEffect(_currentTarget);
        }
        else if (targetToScan == _currentTarget)
        {
            _timeLostTarget = 0f;
            _currentScanTime += Time.deltaTime;
            
            float scanPercent = Mathf.Clamp01(_currentScanTime / _scanDuration);
            OnScanProgress?.Invoke(scanPercent);

            if (_currentScanTime >= _scanDuration)
            {
                _scanFinished = true;
                _isScanning = false;

                GameObject finalTarget = _currentTarget;
                _currentTarget = null;

                ProcessScanResult(finalTarget);
            }
        }
        else
        {
            CancelScan();
        }
    }

    private void HandleMissingTarget()
    {
        if (_isScanning)
        {
            _timeLostTarget += Time.deltaTime;
            if (_timeLostTarget >= _gracePeriod)
            {
                CancelScan();
            }
        }
        else
        {
            CancelScan();
        }
    }

    private void ProcessScanResult(GameObject finalTarget)
    {
        OnScanComplete?.Invoke(finalTarget);

        if (_scanManager != null) _scanManager.StopScanEffect();

        if (_missionManager != null)
        {
            MissionState currentState = _missionManager.GetMissionState();

            switch (currentState)
            {
                case MissionState.Mission1:
                    if (_partsScreen != null) _partsScreen.ReceiveScannedObject(finalTarget);
                    break;

                case MissionState.Mission2:
                case MissionState.Mission3:
                    _missionManager.ValidateCurrentMissionScan(finalTarget);
                    break;
            }
        }
    }

    private void CancelScan()
    {
        if (_isScanning && !_scanFinished)
        {
            _isScanning = false;
            _currentScanTime = 0f;
            _currentTarget = null;
            _timeLostTarget = 0f;

            OnScanProgress?.Invoke(0f);
            OnScanCanceled?.Invoke();

            if (_scanManager != null) _scanManager.StopScanEffect();
        }
    }

    public void SetCanScan(bool value)
    {
        _canScan = value;
        if (!value) CancelScan();
    }

    public void ResetScanner()
    {
        _scanFinished = false;
        _currentScanTime = 0f;
        _isScanning = false;
        _currentTarget = null;
        _timeLostTarget = 0f;
        OnScanProgress?.Invoke(0f);

        if (_scanManager != null) _scanManager.StopScanEffect();
    }
}