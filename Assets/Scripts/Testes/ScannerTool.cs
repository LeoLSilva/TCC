using Oculus.Interaction.HandGrab;
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

    [Header("Visual Effect Settings")]
    [SerializeField] private Material _scanMaterial;

    public UnityEvent<float> OnScanProgress;
    public UnityEvent OnScanComplete;
    public UnityEvent OnScanCanceled;

    private float _dampedUseStrength = 0;
    private float _lastUseTime;
    private bool _isScanning = false;
    private float _currentScanTime = 0f;
    private bool _scanFinished = false;
    private ScanManager _currentScanManager;

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

        if (progress >= _fireThreshold)
        {
            if (!_isScanning)
            {
                _isScanning = true;
                _currentScanTime = 0f;
            }

            bool isHittingTarget = false;
            ScanManager hitScanManager = null;

            if (_scanOrigin != null)
            {
                if (Physics.Raycast(_scanOrigin.position, _scanOrigin.forward, out RaycastHit hit, _maxScanDistance, _scanLayerMask))
                {
                    if (hit.collider.CompareTag(_targetTag))
                    {
                        isHittingTarget = true;
                        Transform rootObj = hit.collider.transform.root;
                        hitScanManager = rootObj.GetComponent<ScanManager>();

                        if (hitScanManager == null)
                        {
                            hitScanManager = rootObj.gameObject.AddComponent<ScanManager>();
                            hitScanManager.scanMaterial = _scanMaterial;
                        }
                    }
                }
            }

            if (hitScanManager != _currentScanManager)
            {
                if (_currentScanManager != null)
                {
                    _currentScanManager.effectActive = false;
                }
                _currentScanManager = hitScanManager;
            }

            if (isHittingTarget)
            {
                if (_currentScanManager != null)
                {
                    _currentScanManager.effectActive = true;
                }

                _currentScanTime += Time.deltaTime;
                float scanPercent = Mathf.Clamp01(_currentScanTime / _scanDuration);
                OnScanProgress?.Invoke(scanPercent);

                if (_currentScanTime >= _scanDuration)
                {
                    _scanFinished = true;
                    _isScanning = false;

                    if (_currentScanManager != null)
                    {
                        _currentScanManager.effectActive = false;
                        _currentScanManager = null;
                    }

                    OnScanComplete?.Invoke();
                }
            }
            else
            {
                StopCurrentScanManager();
                if (_currentScanTime > 0f)
                {
                    _currentScanTime = 0f;
                    OnScanProgress?.Invoke(0f);
                }
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
        StopCurrentScanManager();

        if (_isScanning && !_scanFinished)
        {
            _isScanning = false;
            _currentScanTime = 0f;
            OnScanProgress?.Invoke(0f);
            OnScanCanceled?.Invoke();
        }
    }

    private void StopCurrentScanManager()
    {
        if (_currentScanManager != null)
        {
            _currentScanManager.effectActive = false;
            _currentScanManager = null;
        }
    }

    public void SetCanScan(bool value)
    {
        _canScan = value;
        if (!value) CancelScan();
    }

    public void ResetScanner()
    {
        StopCurrentScanManager();
        _scanFinished = false;
        _currentScanTime = 0f;
        _isScanning = false;
        OnScanProgress?.Invoke(0f);
    }
}