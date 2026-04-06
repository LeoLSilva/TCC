using UnityEngine;
using static DiagramSerializable;

public class DiagramRegister : MonoBehaviour
{
    [SerializeField] private DiagramNode _rootPart;

    private void Start()
    {
        _rootPart.partPrefab = this.gameObject;
    }

    public void AddConnection(string conName, DiagramRegister node)
    {
        DiagramConnection c = new DiagramConnection();
        c.conName = conName;
        c.connectedPart = node.GetDiagramNode();
        _rootPart.connections.Add(c);
    }

    public void RemoveConnection(string conName, DiagramRegister node)
    {
        if (_rootPart.connections == null) return;


        for (int i = 0; i < _rootPart.connections.Count; i++)
        {
            if (_rootPart.connections[i].conName == conName && _rootPart.connections[i].connectedPart == node.GetDiagramNode())
            {
                _rootPart.connections.RemoveAt(i);
                break;
            }
        }
    }

    public DiagramNode GetDiagramNode() { return _rootPart; }
}
