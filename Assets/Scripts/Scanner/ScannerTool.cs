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

    [Header("Raycast Settings")]
    [SerializeField] private Transform _scanOrigin;
    [SerializeField] private float _maxScanDistance = 2f;
    [SerializeField] private LayerMask _scanLayerMask = ~0;
    [SerializeField] private string _targetTag = "obj";

    public UnityEvent<GameObject> OnScanStarted;
    public UnityEvent<float> OnScanProgress;
    public UnityEvent<GameObject> OnScanComplete;
    public UnityEvent OnScanCanceled;

    private float _dampedUseStrength = 0;
    private float _lastUseTime;
    private bool _isScanning = false;
    private float _currentScanTime = 0f;
    private bool _scanFinished = false;
    private GameObject _currentTarget;

    [Header("Testes")]
    public GameObject itenScannerText;

    private PartsScreen _partsScreen;
    private MissionManager _missionManager;
    private ScanManager _scanManager;

    private void Awake()
    {
        _partsScreen = FindAnyObjectByType<PartsScreen>();
        _missionManager = FindAnyObjectByType<MissionManager>();
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
        if (!_canScan || _scanFinished) return;

        if (_missionManager != null && !_missionManager.IsGameplayActive()) return;

        if (progress >= _fireThreshold)
        {
            if (_scanOrigin != null && Physics.Raycast(_scanOrigin.position, _scanOrigin.forward, out RaycastHit hit, _maxScanDistance, _scanLayerMask))
            {
                if (hit.collider.CompareTag(_targetTag))
                {
                    GameObject hitObj = hit.collider.gameObject;
                    GameObject targetToScan = hitObj;

                    if (_missionManager != null && _missionManager.GetMissionState() == MissionState.Mission2)
                    {
                        if (hitObj.transform.parent != null)
                        {
                            PartsScript[] pecasNoContainer = hitObj.transform.parent.GetComponentsInChildren<PartsScript>();

                            if (pecasNoContainer.Length > 1)
                            {
                                targetToScan = hitObj.transform.parent.gameObject;
                            }
                            else
                            {
                                CancelScan();
                                return;
                            }
                        }
                        else
                        {
                            CancelScan();
                            return;
                        }
                    }

                    if (!_isScanning)
                    {
                        _isScanning = true;
                        _currentTarget = targetToScan;
                        _currentScanTime = 0f;

                        OnScanStarted?.Invoke(_currentTarget);

                        if (_scanManager != null) _scanManager.StartScanEffect(_currentTarget);
                    }
                    else if (targetToScan != _currentTarget)
                    {
                        CancelScan();
                        return;
                    }

                    _currentScanTime += Time.deltaTime;
                    float scanPercent = Mathf.Clamp01(_currentScanTime / _scanDuration);
                    OnScanProgress?.Invoke(scanPercent);

                    if (_currentScanTime >= _scanDuration)
                    {
                        _scanFinished = true;
                        _isScanning = false;

                        GameObject finalTarget = _currentTarget;
                        _currentTarget = null;

                        OnScanComplete?.Invoke(finalTarget);

                        if (_scanManager != null) _scanManager.StopScanEffect();

                        if (_missionManager != null)
                        {
                            if (_missionManager.GetMissionState() == MissionState.Mission1)
                            {
                                if (_partsScreen != null) _partsScreen.ReceiveScannedObject(finalTarget);
                            }
                            else if (_missionManager.GetMissionState() == MissionState.Mission2)
                            {
                                _missionManager.ValidateMission2Scan(finalTarget);
                            }
                        }
                    }
                }
                else
                {
                    CancelScan();
                }
            }
            else
            {
                CancelScan();
            }
        }
        else if (progress <= _releaseThreshold)
        {
            if (_isScanning)
            {
                CancelScan();
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
        OnScanProgress?.Invoke(0f);

        if (_scanManager != null) _scanManager.StopScanEffect();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (itenScannerText != null)
            {
                GameObject targetToScan = itenScannerText;

                if (_missionManager != null && _missionManager.GetMissionState() == MissionState.Mission2)
                {
                    if (itenScannerText.transform.parent != null)
                    {
                        if (itenScannerText.transform.parent.GetComponentsInChildren<PartsScript>().Length > 1)
                        {
                            targetToScan = itenScannerText.transform.parent.gameObject;
                        }
                    }
                }

                OnScanStarted?.Invoke(targetToScan);

                if (_scanManager != null) _scanManager.StartScanEffect(targetToScan);

                OnScanComplete?.Invoke(targetToScan);

                if (_scanManager != null) _scanManager.StopScanEffect();

                if (_missionManager != null)
                {
                    if (_missionManager.GetMissionState() == MissionState.Mission1)
                    {
                        if (_partsScreen != null) _partsScreen.ReceiveScannedObject(targetToScan);
                    }
                    else if (_missionManager.GetMissionState() == MissionState.Mission2)
                    {
                        _missionManager.ValidateMission2Scan(targetToScan);
                    }
                }
            }
        }
    }
}