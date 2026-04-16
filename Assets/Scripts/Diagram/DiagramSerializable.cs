using System.Collections.Generic;
using System;
using UnityEngine;

public class DiagramSerializable : MonoBehaviour
{
    [Serializable]
    public class DiagramNode
    {
        public GameObject partPrefab;

        public List<DiagramConnection> connections = new List<DiagramConnection>();
    }

    [Serializable]
    public class DiagramConnection
    {
        public string conName;
        public DiagramNode connectedPart;
    }
}
