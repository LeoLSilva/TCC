using UnityEngine;
using System.Collections.Generic;
using Oculus.Interaction;
using System;
using System.Collections;

public class ScanManager : MonoBehaviour
{
    public Material scanMaterial;
    public bool effectActive = false;

    private GameObject _currentTarget;
    private List<MeshRenderer> targets = new List<MeshRenderer>();
    private float minY, maxY;




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
}