using UnityEngine;

[System.Serializable]
public class DiagramDataBP
{
    [SerializeField] private DiagramScriptableObject _diagram;
    [SerializeField] private GameObject _object;
    [SerializeField] private GameObject _diaBD;

    public DiagramScriptableObject GetDiagram()
    {
        return _diagram;
    }

    public void SetDiagram(DiagramScriptableObject diagram)
    {
        _diagram = diagram;
    }

    public GameObject GetDiagramObject()
    {
        return _object;
    }

    public void SetDiagramObject(GameObject obj)
    {
        _object = obj;
    }

    public GameObject GetDiagramBD()
    {
        return _diaBD;
    }

    public void SetDiagramBD(GameObject diaBD)
    {
        _diaBD = diaBD;
    }

    public void CreateDiagram(DiagramScriptableObject dia, GameObject ob, GameObject diaBD)
    {
        _diagram = dia;
        _object = ob;
        _diaBD = diaBD;
    }
}