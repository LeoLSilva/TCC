using UnityEngine;
using System.Collections.Generic;
using Oculus.Interaction;
using System;
using System.Collections;

public class ScanManager : MonoBehaviour
{
    public Material scanMaterial;
    public bool effectActive = false;

    private Grabbable _grab;
    private GameObject _currentTarget;
    private List<MeshRenderer> targets = new List<MeshRenderer>();
    private float minY, maxY;

        [Header("Scanner Point")]
    [SerializeField]private Transform _ResetScannerPosObj;
    private Coroutine _returnScannerCoroutine;
    public float movementTime = 2.0f;

    void Start()
    {
        _grab = GetComponent<Grabbable>();
        _grab.WhenPointerEventRaised += OnPointerEventRaised;
    }

    void Update()
    {
        if (!effectActive || !_currentTarget)
        {
            if (targets.Count > 0) RemoveAll();
            return;
        }

        CalculateBounds();
        UpdateShaderParams();
        ApplyToChildren();
    }

    public void StartScanEffect(GameObject target)
    {
        _currentTarget = target;
        effectActive = true;
    }

    public void StopScanEffect()
    {
        effectActive = false;
        RemoveAll();
        _currentTarget = null;
    }

    public void StopScanEffect(GameObject target)
    {
        StopScanEffect();
    }

    void CalculateBounds()
    {
        minY = float.MaxValue;
        maxY = float.MinValue;
        targets.Clear();

        if (_currentTarget == null) return;

        MeshRenderer[] renderers = _currentTarget.GetComponentsInChildren<MeshRenderer>();

        foreach (var r in renderers)
        {
            if (r.GetComponent<PartsScript>() != null || r.GetComponentInParent<ScannablePart>() != null)
            {
                targets.Add(r);
                minY = Mathf.Min(minY, r.bounds.min.y);
                maxY = Mathf.Max(maxY, r.bounds.max.y);
            }
        }
    }

    void UpdateShaderParams()
    {
        if (scanMaterial != null)
        {
            scanMaterial.SetFloat("_MinY", minY);
            scanMaterial.SetFloat("_MaxY", maxY);
        }
    }

    void ApplyToChildren()
    {
        foreach (var r in targets)
        {
            List<Material> mats = new List<Material>(r.sharedMaterials);
            if (!mats.Contains(scanMaterial))
            {
                mats.Add(scanMaterial);
                r.materials = mats.ToArray();
            }
        }
    }

    void RemoveAll()
    {
        foreach (var r in targets)
        {
            if (r == null) continue;

            List<Material> mats = new List<Material>(r.sharedMaterials);
            if (mats.Contains(scanMaterial))
            {
                mats.Remove(scanMaterial);
                r.materials = mats.ToArray();
            }
        }
        targets.Clear();
    }

    private void OnPointerEventRaised(PointerEvent obj)
    {
         if (obj.Type == PointerEventType.Select)
        {
            if(_returnScannerCoroutine != null)
            {
                StopCoroutine(_returnScannerCoroutine);
                _returnScannerCoroutine = null;
            }
        } else if (obj.Type == PointerEventType.Unselect)
        {
            _returnScannerCoroutine = StartCoroutine(ReturnScannerToBaseCoroutine());
        }
    }

    private IEnumerator ReturnScannerToBaseCoroutine()
    {
        yield return new WaitForSeconds(5f);
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < movementTime)
        {
            float t = elapsedTime / movementTime;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPosition, _ResetScannerPosObj.position, t);
            transform.rotation = Quaternion.Slerp(startRotation, _ResetScannerPosObj.rotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = _ResetScannerPosObj.position;
        transform.rotation = _ResetScannerPosObj.rotation;
    }
    
}