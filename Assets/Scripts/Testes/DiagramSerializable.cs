using System.Collections.Generic;
using System;
using UnityEngine;

public class DiagramSerializable : MonoBehaviour
{
    [Serializable]
    public class DiagramNode
    {
        [Tooltip("Arraste o PREFAB da peça aqui (ex: Prefab da Cabeça)")]
        public GameObject partPrefab;

        [Tooltip("Lista de peças conectadas a esta peça")]
        public List<DiagramConnection> connections = new List<DiagramConnection>();
    }

    [Serializable]
    public class DiagramConnection
    {
        [Tooltip("Nome do GameObject do conector nesta peça (ex: conAntena1)")]
        public string conName;

        [Tooltip("A peça que vai ser conectada neste buraquinho")]
        public DiagramNode connectedPart;
    }
}
