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

    public static void SaveDiagram(DiagramRegister rootRegister, string fileName)
    {
        if (rootRegister == null) return;

        NodeSaveData saveData = ConvertToSaveData(rootRegister.GetDiagramNode());
        string json = JsonUtility.ToJson(saveData, true);

        string path = Path.Combine(Application.persistentDataPath, $"{fileName}.json");
        File.WriteAllText(path, json);

        Debug.Log(path);
    }

    private static NodeSaveData ConvertToSaveData(DiagramNode node)
    {
        if (node == null) return null;

        NodeSaveData data = new NodeSaveData();

        data.prefabName = node.partPrefab != null ? node.partPrefab.name.Replace("(Clone)", "").Trim() : "";

        if (node.connections != null)
        {
            foreach (var conn in node.connections)
            {
                ConnectionSaveData connData = new ConnectionSaveData();
                connData.conName = conn.conName;
                connData.connectedPart = ConvertToSaveData(conn.connectedPart);
                data.connections.Add(connData);
            }
        }

        return data;
    }
}