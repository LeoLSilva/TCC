using UnityEngine;
using System.Collections.Generic;
using System.Text;
using static DiagramSerializable;

public class DiagramRegister : MonoBehaviour
{
    [SerializeField] private DiagramNode _rootPart;
    [SerializeField] private PartsSoloScriptableObject _partData;

    private void Start()
    {
        if (_rootPart == null)
        {
            _rootPart = new DiagramNode();
        }
        _rootPart.partPrefab = this.gameObject;
    }

    public PartsSoloScriptableObject GetPartData()
    {
        return _partData;
    }

    public void AddConnection(string conName, DiagramRegister node)
    {
        if (node == null) return;

        if (_rootPart.connections == null)
        {
            _rootPart.connections = new List<DiagramConnection>();
        }

        DiagramConnection c = new DiagramConnection();
        c.conName = conName;
        c.connectedPart = node.GetDiagramNode();
        _rootPart.connections.Add(c);

        ReportToContainer(true, conName, node);
    }

    public void RemoveConnection(string conName, DiagramRegister node)
    {
        if (_rootPart == null || _rootPart.connections == null || node == null) return;

        for (int i = 0; i < _rootPart.connections.Count; i++)
        {
            if (_rootPart.connections[i].conName == conName && _rootPart.connections[i].connectedPart == node.GetDiagramNode())
            {
                _rootPart.connections.RemoveAt(i);
                ReportToContainer(false, conName, node);
                break;
            }
        }
    }

    private void ReportToContainer(bool isConnect, string conName, DiagramRegister node)
    {
        DiagramContainerLog log = GetComponentInParent<DiagramContainerLog>();
        if (log != null && node != null)
        {
            string parentName = this.gameObject.name.Replace("(Clone)", "").Trim();
            string childName = node.gameObject.name.Replace("(Clone)", "").Trim();

            if (isConnect)
                log.LogConnection(childName, conName, parentName);
            else
                log.LogDisconnection(childName, conName, parentName);
        }
    }

    public DiagramNode GetDiagramNode() { return _rootPart; }

    public string GetLocalSignature()
    {
        return BuildSignatureRecursive(_rootPart);
    }

    private string BuildSignatureRecursive(DiagramNode node)
    {
        if (node == null || node.partPrefab == null) return "";

        string cleanName = node.partPrefab.name.Replace("(Clone)", "").Trim();
        cleanName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"\s*\(\d+\)", "");

        StringBuilder sig = new StringBuilder();
        sig.Append(cleanName).Append("[");

        if (node.connections != null && node.connections.Count > 0)
        {
            List<DiagramConnection> sortedConnections = new List<DiagramConnection>(node.connections);
            sortedConnections.Sort((a, b) => string.Compare(a.conName, b.conName));

            foreach (DiagramConnection conn in sortedConnections)
            {
                sig.Append(conn.conName).Append(":");
                sig.Append(BuildSignatureRecursive(conn.connectedPart)).Append(",");
            }
        }

        sig.Append("]");
        return sig.ToString();
    }
}