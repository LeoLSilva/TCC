using UnityEngine;
using System.Collections.Generic;
using System.IO;
using static DiagramSerializable;

public static class DiagramJsonSaver
{
    [System.Serializable]
    public class NodeSaveData
    {
        public string prefabName;
        public List<ConnectionSaveData> connections = new List<ConnectionSaveData>();
    }

    [System.Serializable]
    public class ConnectionSaveData
    {
        public string conName;
        public NodeSaveData connectedPart;
    }

    private static HashSet<string> _savedSignatures = null;

    public static bool SaveDiagram(DiagramRegister rootRegister, string baseFileName)
    {
        if (rootRegister == null) return false;

        if (IsDiagramAlreadySaved(rootRegister))
        {
            return false;
        }

        NodeSaveData saveData = ConvertToSaveData(rootRegister.GetDiagramNode());
        string json = JsonUtility.ToJson(saveData, true);

        string cleanName = baseFileName.Replace("(Clone)", "").Replace("DiagramContainer_", "").Trim();
        string path = Application.persistentDataPath;
        string finalPath = Path.Combine(path, $"{cleanName}.json");
        int counter = 1;

        while (File.Exists(finalPath))
        {
            finalPath = Path.Combine(path, $"{cleanName}_{counter}.json");
            counter++;
        }

        File.WriteAllText(finalPath, json);

        if (_savedSignatures == null) LoadSignaturesCache();
        _savedSignatures.Add(GenerateSignature(saveData));

        return true;
    }

    private static NodeSaveData ConvertToSaveData(DiagramNode node)
    {
        if (node == null) return null;

        NodeSaveData data = new NodeSaveData();

        if (node.partPrefab != null)
        {
            string cleanPrefabName = node.partPrefab.name.Replace("(Clone)", "").Trim();
            if (cleanPrefabName.StartsWith("DiagramContainer_"))
            {
                cleanPrefabName = cleanPrefabName.Replace("DiagramContainer_", "");
            }
            data.prefabName = cleanPrefabName;
        }
        else
        {
            data.prefabName = "";
        }

        if (node.connections != null)
        {
            foreach (var conn in node.connections)
            {
                if (conn.connectedPart != null)
                {
                    ConnectionSaveData connData = new ConnectionSaveData();
                    connData.conName = conn.conName;
                    connData.connectedPart = ConvertToSaveData(conn.connectedPart);
                    data.connections.Add(connData);
                }
            }
        }

        return data;
    }

    public static bool IsDiagramAlreadySaved(DiagramRegister rootRegister)
    {
        if (_savedSignatures == null) LoadSignaturesCache();
        if (rootRegister == null) return false;

        NodeSaveData currentData = ConvertToSaveData(rootRegister.GetDiagramNode());
        string currentSig = GenerateSignature(currentData);

        return _savedSignatures.Contains(currentSig);
    }

    private static void LoadSignaturesCache()
    {
        _savedSignatures = new HashSet<string>();
        string path = Application.persistentDataPath;

        if (!Directory.Exists(path)) return;

        string[] files = Directory.GetFiles(path, "*.json");
        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                NodeSaveData savedData = JsonUtility.FromJson<NodeSaveData>(json);
                if (savedData != null)
                {
                    _savedSignatures.Add(GenerateSignature(savedData));
                }
            }
            catch { }
        }
    }

    private static string GenerateSignature(NodeSaveData node)
    {
        if (node == null) return "";
        string sig = (node.prefabName ?? "") + "[";

        if (node.connections != null)
        {
            List<ConnectionSaveData> sortedConns = new List<ConnectionSaveData>(node.connections);
            sortedConns.Sort((a, b) => string.Compare(a.conName ?? "", b.conName ?? ""));

            foreach (var conn in sortedConns)
            {
                sig += (conn.conName ?? "") + ":" + GenerateSignature(conn.connectedPart) + ",";
            }
        }

        return sig + "]";
    }
}