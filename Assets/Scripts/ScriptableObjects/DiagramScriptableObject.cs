using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NovoDiagrama", menuName = "Montagem/Diagrama")]
public class DiagramScriptableObject : ScriptableObject
{
    [SerializeField] private List<DiagramMontage> _objectsPregabList = new List<DiagramMontage>();

    public List<DiagramMontage> getList() { return _objectsPregabList; }
}

[System.Serializable]
public class DiagramMontage
{
    public string nomePeca;
    public GameObject objectPrefab;
    public GameObject objectTransparentPrefab;
    public Vector3 position;
    public Vector3 rotation;
}
