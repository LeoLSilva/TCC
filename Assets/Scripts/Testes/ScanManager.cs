using UnityEngine;
using System.Collections.Generic;

public class ScanManager : MonoBehaviour
{
    public Material scanMaterial;
    public bool effectActive = false;

    // VARIÁVEIS REMOVIDAS DAQUI: speed e lineWidth

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
            // Só processa se tiver o seu script
            if (r.GetComponent<PartsScript>() != null)
            {
                targets.Add(r);
                // Pega os limites reais no mundo
                minY = Mathf.Min(minY, r.bounds.min.y);
                maxY = Mathf.Max(maxY, r.bounds.max.y);
            }
        }
    }

    void UpdateShaderParams()
    {
        // Envia APENAS o tamanho do robô para o shader saber onde começa e termina.
        // O Line Width e o Speed agora são lidos direto do Material!
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