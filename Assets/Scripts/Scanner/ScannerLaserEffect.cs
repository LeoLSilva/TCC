using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ScannerLaserEffect : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private float _maxDistance = 2f;
    [SerializeField] private LayerMask _hitMask = ~0;

    [SerializeField] private float _baseWidth = 0.02f;
    [SerializeField] private float _pulseAmount = 0.01f;
    [SerializeField] private float _pulseSpeed = 15f;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.positionCount = 2;
    }

    public void UpdateScanEffect(float progress)
    {
        if (progress > 0f && progress < 1f)
        {
            _lineRenderer.enabled = true;

            float currentWidth = _baseWidth + Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount;
            _lineRenderer.widthMultiplier = currentWidth;

            float currentDistance = _maxDistance;

            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _maxDistance, _hitMask))
            {
                currentDistance = hit.distance;
            }

            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, new Vector3(0, 0, currentDistance));
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }

    public void ForceStop()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
    }
}