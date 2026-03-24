using System;
using UnityEngine;
using static DiagramSerializable;

public class DiagramManager : MonoBehaviour
{
    [SerializeField] private DiagramScriptableObject _diagramScriptable;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) { SpawnDiagram(); }
    }

    private void SpawnDiagram()
    {

        if (_diagramScriptable == null || _diagramScriptable.rootPart == null || _diagramScriptable.rootPart.partPrefab == null) { return; }

        SpawnAll(_diagramScriptable.rootPart, transform.position, transform.rotation, null, "");



    }

    PartsScript SpawnAll(DiagramNode node, Vector3 pos, Quaternion rot, PartsScript parentScript, string conName)
    {
        if (node == null || node.partPrefab == null) return null;
        GameObject newPart = Instantiate(node.partPrefab, pos, rot);
        PartsScript newPartScript = newPart.GetComponent<PartsScript>();
        if (parentScript != null && !string.IsNullOrEmpty(conName))
        {
            SnapIndicator targetSnap = FindSnapIndicatorByName(parentScript, conName);
            SnapIndicator mySnap = FindMaleConnector(newPartScript);
            if (targetSnap != null)
            {
                newPart.transform.position = targetSnap.transform.position;
                newPart.transform.rotation = targetSnap.transform.rotation;

                newPartScript.AutoConnect(targetSnap, mySnap);
            }
            else
            {
                if (targetSnap == null) Debug.LogWarning($"Falha: O conector (fêmea) '{conName}' não foi encontrado dentro do Prefab '{parentScript.gameObject.name}'!");
                if (mySnap == null) Debug.LogWarning($"Falha: A peça '{newPart.name}' não possui nenhum SnapIndicator configurado como 'male'!");
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
                SpawnAll(connection.connectedPart, Vector3.zero, Quaternion.identity, newPartScript, connection.conName);
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
        string nomesEncontrados = "";
        foreach (SnapIndicator indicator in indicators)
        {
            if (indicator.GetConnectType() == ConnectType.famale)
            {
                string indName = indicator.gameObject.name.Trim();
                nomesEncontrados += $"[{indName}] ";
                if (string.Equals(indName, searchName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return indicator;
                }
            }
        }

        Debug.LogWarning($"Conector '{conName}' não encontrado na peça '{parent.gameObject.name}'! Conectores fêmea disponíveis nesta peça são: {nomesEncontrados}");
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
}
