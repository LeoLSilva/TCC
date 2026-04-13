using System.Collections.Generic;
using UnityEngine;

public class DiagramContainerLog : MonoBehaviour
{
    [SerializeField] private List<string> _actionLog = new List<string>();

    public void LogConnection(string childName, string conName, string parentName)
    {
        string log = $"{childName} conectou no conector {conName} na peça {parentName}";
        _actionLog.Add(log);
        Debug.Log(log);
    }

    public void LogDisconnection(string childName, string conName, string parentName)
    {
        string log = $"{childName} desconectou do conector {conName} na peça {parentName}";
        _actionLog.Add(log);
        Debug.Log(log);
    }

    public List<string> GetFullLog()
    {
        return _actionLog;
    }

    public void MergeLog(List<string> oldLog)
    {
        if (oldLog == null || oldLog.Count == 0) return;

        _actionLog.Add("--- Histórico Antigo Herdado ---");
        _actionLog.AddRange(oldLog);
        _actionLog.Add("--------------------------------");
    }

    public void ClearLog()
    {
        _actionLog.Clear();
    }

    public string GenerateCurrentSignature()
    {
        PartsScript[] allParts = GetComponentsInChildren<PartsScript>();
        PartsScript rootPart = null;

        foreach (var p in allParts)
        {
            if (p.GetStatus() == PieceStatus.root)
            {
                rootPart = p;
                break;
            }
        }

        if (rootPart == null) return "VAZIO";

        DiagramRegister reg = rootPart.GetComponent<DiagramRegister>();
        if (reg != null)
        {
            return reg.GetLocalSignature();
        }

        return "ERRO";
    }
}