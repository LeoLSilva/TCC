using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
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
        [SerializeReference]
        public NodeSaveData connectedPart;
    }

    public static event Action OnDiagramSaved;

    public static void SaveDiagram(DiagramRegister rootRegister, string fileName, string folderPath)
    {
        if (rootRegister == null) return;
        if (string.IsNullOrEmpty(folderPath)) return;

        NodeSaveData saveData = ConvertToSaveData(rootRegister.GetDiagramNode(), new HashSet<DiagramNode>());
        string currentSignature = GenerateSignature(saveData);

        Debug.Log($"[DiagramJsonSaver] ASSINATURA FINAL GERADA: {currentSignature}");

        if (Directory.Exists(folderPath))
        {
            string[] allFiles = Directory.GetFiles(folderPath, "*.json");
            foreach (string file in allFiles)
            {
                try
                {
                    string existingJson = File.ReadAllText(file);
                    NodeSaveData existingData = JsonUtility.FromJson<NodeSaveData>(existingJson);
                    if (GenerateSignature(existingData) == currentSignature)
                    {
                        Debug.LogWarning($"[DiagramJsonSaver] Estrutura já existe no arquivo {Path.GetFileName(file)}. Ignorando duplicata.");
                        return;
                    }
                }
                catch { }
            }
        }
        else
        {
            Directory.CreateDirectory(folderPath);
        }

        string cleanName = fileName.Replace("(Clone)", "").Replace("DiagramContainer_", "").Trim();
        string[] existingMatchingFiles = Directory.GetFiles(folderPath, $"{cleanName}*.json");

        int maxIndex = 0;
        bool baseFileExists = false;

        foreach (string file in existingMatchingFiles)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(file);

            if (nameWithoutExt == cleanName)
            {
                baseFileExists = true;
                continue;
            }

            string prefix = cleanName + "_";
            if (nameWithoutExt.StartsWith(prefix))
            {
                string numStr = nameWithoutExt.Substring(prefix.Length);
                if (int.TryParse(numStr, out int parsedNum))
                {
                    if (parsedNum > maxIndex) maxIndex = parsedNum;
                }
            }
        }

        string finalFileName = cleanName;
        if (baseFileExists || maxIndex > 0)
        {
            int nextIndex = Mathf.Max(1, maxIndex + 1);
            finalFileName = $"{cleanName}_{nextIndex}";
        }

        string finalPath = Path.Combine(folderPath, $"{finalFileName}.json");
        File.WriteAllText(finalPath, JsonUtility.ToJson(saveData, true));

        Debug.Log($"[DiagramJsonSaver] Novo diagrama salvo com sucesso em: {finalPath}");
        OnDiagramSaved?.Invoke();
    }

    private static string GenerateSignature(NodeSaveData node)
    {
        if (node == null) return "";

        StringBuilder sb = new StringBuilder();
        sb.Append(node.prefabName).Append("{");

        if (node.connections != null && node.connections.Count > 0)
        {
            List<ConnectionSaveData> sortedConns = new List<ConnectionSaveData>(node.connections);
            sortedConns.Sort((a, b) => string.Compare(a.conName, b.conName));

            foreach (var conn in sortedConns)
            {
                sb.Append(conn.conName).Append(":").Append(GenerateSignature(conn.connectedPart)).Append(",");
            }
        }

        sb.Append("}");
        return sb.ToString();
    }

    private static NodeSaveData ConvertToSaveData(DiagramNode node, HashSet<DiagramNode> visited)
    {
        if (node == null || visited.Contains(node)) return null;

        visited.Add(node);

        NodeSaveData data = new NodeSaveData();
        data.prefabName = node.partPrefab != null ? node.partPrefab.name.Replace("(Clone)", "").Trim() : "";

        if (node.connections != null)
        {
            foreach (var conn in node.connections)
            {
                if (conn.connectedPart != null)
                {
                    NodeSaveData childData = ConvertToSaveData(conn.connectedPart, visited);

                    if (childData != null)
                    {
                        ConnectionSaveData connData = new ConnectionSaveData();
                        connData.conName = conn.conName;
                        connData.connectedPart = childData;
                        data.connections.Add(connData);
                    }
                }
            }
        }

        return data;
    }
}