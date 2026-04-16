using UnityEngine;

public class DiagramDataBP : MonoBehaviour
{
    [SerializeField] private DiagramScriptableObject _diagram;
    [SerializeField] private GameObject _object;
    [SerializeField] private GameObject _diaBD;

    public DiagramScriptableObject Diagram
    {
        get { return _diagram; }
        set { _diagram = value; }
    }

    public GameObject DiagramObject
    {
        get { return _object; }
        set { _object = value; }
    }

    public GameObject DiagramBD
    {
        get { return _diaBD; }
        set { _diaBD = value; }
    }

    public void CreateDiagram(DiagramScriptableObject dia, GameObject ob, GameObject diaBD)
    {
        _diagram = dia;
        _object = ob;
        _diaBD = diaBD;
    }
}