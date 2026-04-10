using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Events;

public class ScannerTool : MonoBehaviour, IHandGrabUseDelegate
{
    [SerializeField] private PartsScript _RootPart;
    [SerializeField] private CreatedDiagramScreen _createScreen;
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
    [SerializeField] private LineRenderer _laserRenderer;

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

    private void Start()
    {
        _createScreen = FindAnyObjectByType<CreatedDiagramScreen>();
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
        if (!_canScan)
        {
            if (_laserRenderer != null) _laserRenderer.enabled = false;
            return;
        }

        if (progress >= _fireThreshold)
        {
            if (_laserRenderer != null && _scanOrigin != null)
            {
                _laserRenderer.enabled = true;
                _laserRenderer.SetPosition(0, _scanOrigin.position);
            }

            bool isHittingTarget = false;
            ScanManager hitScanManager = null;

            if (_scanOrigin != null)
            {
                if (Physics.Raycast(_scanOrigin.position, _scanOrigin.forward, out RaycastHit hit, _maxScanDistance, _scanLayerMask))
                {
                    if (_laserRenderer != null)
                    {
                        _laserRenderer.SetPosition(1, hit.point);
                    }

                    if (hit.collider.CompareTag(_targetTag))
                    {
                        PartsScript hitPart = hit.collider.GetComponentInParent<PartsScript>();

                        if (hitPart != null)
                        {
                            _RootPart = hitPart.FindRootPart();
                            DiagramRegister rootReg = _RootPart.GetComponent<DiagramRegister>();

                            if (rootReg != null && !DiagramJsonSaver.IsDiagramAlreadySaved(rootReg))
                            {
                                isHittingTarget = true;
                                Transform visualRoot = _RootPart.transform.parent;

                                if (visualRoot == null)
                                {
                                    visualRoot = _RootPart.transform;
                                }

                                hitScanManager = visualRoot.GetComponent<ScanManager>();
                                if (hitScanManager == null)
                                {
                                    hitScanManager = visualRoot.gameObject.AddComponent<ScanManager>();
                                    hitScanManager.scanMaterial = _scanMaterial;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_laserRenderer != null)
                    {
                        _laserRenderer.SetPosition(1, _scanOrigin.position + (_scanOrigin.forward * _maxScanDistance));
                    }
                }
            }

            if (_scanFinished) return;

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
                if (!_isScanning)
                {
                    _isScanning = true;
                    _currentScanTime = 0f;
                }

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
            CancelScan();
        }
    }

    private void CancelScan()
    {
        StopCurrentScanManager();

        if (_laserRenderer != null)
        {
            _laserRenderer.enabled = false;
        }

        _isScanning = false;
        _currentScanTime = 0f;
        _scanFinished = false;
        OnScanProgress?.Invoke(0f);
        OnScanCanceled?.Invoke();
    }

    private void StopCurrentScanManager()
    {
        if (_currentScanManager != null)
        {
            _currentScanManager.effectActive = false;
            _currentScanManager = null;
        }
    }

    public void SaveDiagram()
    {
        if (_RootPart != null)
        {
            DiagramRegister rootReg = _RootPart.GetComponent<DiagramRegister>();
            if (rootReg != null)
            {
                string cleanName = _RootPart.gameObject.name.Replace("(Clone)", "").Replace("DiagramContainer_", "").Trim();
                bool saved = DiagramJsonSaver.SaveDiagram(rootReg, cleanName);

                if (saved)
                {
                    if (_createScreen == null) _createScreen = FindAnyObjectByType<CreatedDiagramScreen>();
                    if (_createScreen != null) _createScreen.UpdateDiagramList();
                }
            }
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
        _RootPart = null;
        _scanFinished = false;
        _currentScanTime = 0f;
        _isScanning = false;

        if (_laserRenderer != null)
        {
            _laserRenderer.enabled = false;
        }

        OnScanProgress?.Invoke(0f);
    }
}