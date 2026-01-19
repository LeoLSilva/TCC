using UnityEngine;

public class TableGridGenerator : MonoBehaviour
{
    public GameObject cuboPrefab;

    void Start()
    {
        float xMin = -1.2f;
        float xMax = 1.2f;
        float zMin = -0.4f;
        float zMax = 0.4f;

        Vector3 tamanho = new Vector3(0.5f, 0.005f, 0.5f);

        int qtdX = Mathf.FloorToInt((xMax - xMin) / tamanho.x);
        int qtdZ = Mathf.FloorToInt((zMax - zMin) / tamanho.z);

        float startX = xMin + tamanho.x / 2f;
        float startZ = zMin + tamanho.z / 2f;

        for (int x = 0; x < qtdX; x++)
        {
            for (int z = 0; z < qtdZ; z++)
            {
                GameObject c = Instantiate(cuboPrefab, transform);

                c.transform.localPosition = new Vector3(
                    startX + x * tamanho.x,
                    tamanho.y,
                    startZ + z * tamanho.z
                );

                c.transform.localRotation = Quaternion.identity;
                c.transform.localScale = tamanho;
            }
        }
    }
}
