using UnityEngine;
using static DiagramSerializable;

public class DiagramManager : MonoBehaviour
{
    [SerializeField] private Transform _painelPoint;
    [SerializeField] private GameObject _lastPainelObject;

    public PartsScript SetDiagram(DiagramScriptableObject diagramScriptable, Vector3 targetPosition)
    {
        return SpawnDiagram(diagramScriptable, targetPosition);
    }

    private PartsScript SpawnDiagram(DiagramScriptableObject diagramScriptable, Vector3 targetPosition)
    {
        if (diagramScriptable == null || diagramScriptable.rootPart == null || diagramScriptable.rootPart.partPrefab == null)
        {
            return null;
        }

        Quaternion spawnRot = _painelPoint != null ? _painelPoint.rotation : Quaternion.identity;

        Transform diagramContainer = ContainerFactory.CreateContainer(diagramScriptable.name, targetPosition, spawnRot);

        PartsScript rootPartScript = SpawnAll(diagramScriptable.rootPart, targetPosition, spawnRot, null, "", diagramContainer);

        if (rootPartScript != null)
        {
            rootPartScript.SetStatus(PieceStatus.root);
            _lastPainelObject = rootPartScript.gameObject;
        }
        else
        {
            Destroy(diagramContainer.gameObject);
        }

        return rootPartScript;
    }

    PartsScript SpawnAll(DiagramNode node, Vector3 pos, Quaternion rot, PartsScript parentScript, string conName, Transform container)
    {
        if (node == null || node.partPrefab == null) return null;

        GameObject newPart = Instantiate(node.partPrefab, pos, rot);
        newPart.transform.SetParent(container, true);
        PartsScript newPartScript = newPart.GetComponent<PartsScript>();

        if (parentScript != null && !string.IsNullOrEmpty(conName))
        {
            SnapIndicator targetSnap = FindSnapIndicatorByName(parentScript, conName);
            SnapIndicator mySnap = FindMaleConnector(newPartScript);

            if (targetSnap != null)
            {
                targetSnap.gameObject.SetActive(true);
                if (mySnap != null) mySnap.gameObject.SetActive(true);

                newPart.transform.position = targetSnap.transform.position;
                newPart.transform.rotation = targetSnap.transform.rotation;

                newPartScript.AutoConnect(targetSnap, mySnap);
            }
        }
        else if (parentScript == null)
        {
            newPartScript.SetStatus(PieceStatus.root);
        }

        if (node.connections != null && node.connections.Count > 0)
        {
            foreach (DiagramConnection connection in node.connections)
            {
                if (string.IsNullOrWhiteSpace(connection.conName)) continue;
                SpawnAll(connection.connectedPart, Vector3.zero, Quaternion.identity, newPartScript, connection.conName, container);
            }
        }
        return newPartScript;
    }

    private SnapIndicator FindSnapIndicatorByName(PartsScript parent, string conName)
    {
        if (string.IsNullOrEmpty(conName)) return null;
        string searchName = conName.Trim();

        Transform[] allTransforms = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            if (string.Equals(t.name.Trim(), searchName, System.StringComparison.OrdinalIgnoreCase))
            {
                SnapIndicator snap = t.GetComponentInChildren<SnapIndicator>(true);
                if (snap != null && snap.GetConnectType() == ConnectType.famale) return snap;
            }
        }

        SnapIndicator[] indicators = parent.GetComponentsInChildren<SnapIndicator>(true);
        foreach (SnapIndicator indicator in indicators)
        {
            if (indicator.GetConnectType() == ConnectType.famale)
            {
                string indName = indicator.gameObject.name.Trim();
                if (string.Equals(indName, searchName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return indicator;
                }
            }
        }

        return null;
    }

    private SnapIndicator FindMaleConnector(PartsScript part)
    {
        SnapIndicator[] indicators = part.GetComponentsInChildren<SnapIndicator>(true);
        foreach (SnapIndicator ind in indicators)
        {
            if (ind.GetConnectType() == ConnectType.male)
            {
                return ind;
            }
        }
        return null;
    }

    public GameObject GetPainelObject()
    {
        return _lastPainelObject;
    }

    public PartsScript SetDiagramJson(DiagramJsonSaver.NodeSaveData nodeData, Vector3 targetPosition)
    {
        if (nodeData == null || string.IsNullOrEmpty(nodeData.prefabName)) return null;

        Quaternion spawnRot = _painelPoint != null ? _painelPoint.rotation : Quaternion.identity;

        Transform diagramContainer = ContainerFactory.CreateContainer(nodeData.prefabName, targetPosition, spawnRot);

        PartsScript rootPartScript = SpawnAllJson(nodeData, targetPosition, spawnRot, null, "", diagramContainer);

        if (rootPartScript != null)
        {
            rootPartScript.SetStatus(PieceStatus.root);
            _lastPainelObject = rootPartScript.gameObject;
        }
        else
        {
            Destroy(diagramContainer.gameObject);
        }

        return rootPartScript;
    }

    private PartsScript SpawnAllJson(DiagramJsonSaver.NodeSaveData node, Vector3 pos, Quaternion rot, PartsScript parentScript, string conName, Transform container)
    {
        if (node == null || string.IsNullOrEmpty(node.prefabName)) return null;

        GameObject prefab = Resources.Load<GameObject>(node.prefabName);
        if (prefab == null) return null;

        GameObject newPart = Instantiate(prefab, pos, rot);
        newPart.transform.SetParent(container, true);

        PartsScript newPartScript = newPart.GetComponent<PartsScript>();

        if (parentScript != null && !string.IsNullOrEmpty(conName))
        {
            SnapIndicator targetSnap = FindSnapIndicatorByName(parentScript, conName);
            SnapIndicator mySnap = FindMaleConnector(newPartScript);

            if (targetSnap != null)
            {
                targetSnap.gameObject.SetActive(true);
                if (mySnap != null) mySnap.gameObject.SetActive(true);

                newPart.transform.position = targetSnap.transform.position;
                newPart.transform.rotation = targetSnap.transform.rotation;
                newPartScript.AutoConnect(targetSnap, mySnap);
            }
        }
        else if (parentScript == null)
        {
            newPartScript.SetStatus(PieceStatus.root);
        }

        if (node.connections != null && node.connections.Count > 0)
        {
            foreach (DiagramJsonSaver.ConnectionSaveData connection in node.connections)
            {
                if (string.IsNullOrWhiteSpace(connection.conName)) continue;
                SpawnAllJson(connection.connectedPart, Vector3.zero, Quaternion.identity, newPartScript, connection.conName, container);
            }
        }
        return newPartScript;
    }
}