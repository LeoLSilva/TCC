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
        if (_rootPart == null || _rootPart.partPrefab == null)
        {
            _rootPart = new DiagramNode();
        }
        _rootPart.partPrefab = this.gameObject;
    }

    public PartsSoloScriptableObject GetPartData()
    {
        return _partData;
    }

    public void InjectSavedData(DiagramNode savedNode)
    {
        if (savedNode != null)
        {
            _rootPart = DeepCopyNode(savedNode);
            _rootPart.partPrefab = this.gameObject;
        }
    }

    private DiagramNode DeepCopyNode(DiagramNode original)
    {
        if (original == null) return null;

        DiagramNode copy = new DiagramNode();
        copy.partPrefab = original.partPrefab;

        if (original.connections != null)
        {
            copy.connections = new List<DiagramConnection>();
            foreach (var conn in original.connections)
            {
                DiagramConnection newConn = new DiagramConnection();
                newConn.conName = conn.conName;
                newConn.connectedPart = DeepCopyNode(conn.connectedPart);
                copy.connections.Add(newConn);
            }
        }
        return copy;
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
        if (_rootPart != null && _rootPart.partPrefab == null)
        {
            _rootPart.partPrefab = this.gameObject;
        }

        string sigFinal = BuildSignatureRecursive(_rootPart);
        return sigFinal;
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
            Dictionary<string, DiagramConnection> uniqueConnections = new Dictionary<string, DiagramConnection>();

            foreach (DiagramConnection conn in node.connections)
            {
                if (conn.connectedPart != null && conn.connectedPart.partPrefab != null)
                {
                    uniqueConnections[conn.conName] = conn;
                }
            }

            List<string> sortedKeys = new List<string>(uniqueConnections.Keys);
            sortedKeys.Sort();

            foreach (string key in sortedKeys)
            {
                string childSig = BuildSignatureRecursive(uniqueConnections[key].connectedPart);
                if (!string.IsNullOrEmpty(childSig))
                {
                    sig.Append(key).Append(":");
                    sig.Append(childSig).Append(",");
                }
            }
        }

        sig.Append("]");
        return sig.ToString();
    }
}