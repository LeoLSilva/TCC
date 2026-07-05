using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DiagramCreaterManager : MonoBehaviour
{
    [Serializable]
    public class LoadedDiagram
    {
        public string diagramName;
        public DiagramJsonSaver.NodeSaveData rootNode;
    }

    [SerializeField] private List<LoadedDiagram> _loadedDiagrams = new List<LoadedDiagram>();

    void Start()
    {
        RefreshDiagramList();
    }

    [ContextMenu("Atualizar Lista de Diagramas")]
    public void RefreshDiagramList()
    {
        _loadedDiagrams.Clear();

        string path = StorageManager.PathCriacoes;

        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path, "*.json");

            foreach (string file in files)
            {
                try
                {
                    string jsonContent = File.ReadAllText(file);
                    DiagramJsonSaver.NodeSaveData data = JsonUtility.FromJson<DiagramJsonSaver.NodeSaveData>(jsonContent);

                    if (data != null && !string.IsNullOrEmpty(data.prefabName))
                    {
                        LoadedDiagram loaded = new LoadedDiagram();
                        loaded.diagramName = Path.GetFileNameWithoutExtension(file);
                        loaded.rootNode = data;

                        _loadedDiagrams.Add(loaded);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
    }

    public List<LoadedDiagram> GetLoadedDiagrams() => _loadedDiagrams;
}