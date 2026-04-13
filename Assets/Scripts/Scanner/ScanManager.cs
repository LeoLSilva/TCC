using UnityEngine;
using System.Collections.Generic;

public class ScanManager : MonoBehaviour
{
    public Material scanMaterial;
    public bool effectActive = false;


    private List<MeshRenderer> targets = new List<MeshRenderer>();
    private float minY, maxY;

    void Update()
    {
        if (!effectActive)
        {
            RemoveAll();
            return;
        }

        CalculateBounds();
        UpdateShaderParams();
        ApplyToChildren();
    }

    void CalculateBounds()
    {
        minY = float.MaxValue;
        maxY = float.MinValue;
        targets.Clear();

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var r in renderers)
        {
            if (r.GetComponent<PartsScript>() != null)
            {
                targets.Add(r);
                minY = Mathf.Min(minY, r.bounds.min.y);
                maxY = Mathf.Max(maxY, r.bounds.max.y);
            }
        }
    }

    void UpdateShaderParams()
    {
        scanMaterial.SetFloat("_MinY", minY);
        scanMaterial.SetFloat("_MaxY", maxY);
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
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            List<Material> mats = new List<Material>(r.sharedMaterials);
            if (mats.Contains(scanMaterial))
            {
                mats.Remove(scanMaterial);
                r.materials = mats.ToArray();
            }
        }
    }
}