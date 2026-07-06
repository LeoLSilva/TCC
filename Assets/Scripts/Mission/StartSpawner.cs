using UnityEngine;

public class StartSpawner : MonoBehaviour
{
    [SerializeField] private DiagramManager _diagramManager;
    [SerializeField] private DiagramScriptableObject _roboInicial;
    [SerializeField] private Transform _spawnStartPoint;

    private void Start()
    {
        if (_diagramManager == null)
        {
            _diagramManager = FindAnyObjectByType<DiagramManager>();
        }
    }

    public void SpawnAllDiagrams()
    {
        if (_diagramManager == null || _roboInicial == null) return;

        Vector3 spawnPos = _spawnStartPoint != null ? _spawnStartPoint.position : transform.position;

        var spawnedObj = _diagramManager.SetDiagram(_roboInicial, spawnPos);

        if (spawnedObj != null)
        {
            spawnedObj.SetHierarchyLayerAndPhysics("Default", false);

            if (spawnedObj.transform.parent != null)
            {
                int defaultLayer = LayerMask.NameToLayer("Default");
                PartsScript[] allSpawnedParts = spawnedObj.transform.parent.GetComponentsInChildren<PartsScript>();

                foreach (var part in allSpawnedParts)
                {
                    part.gameObject.layer = defaultLayer;
                    part.ChangeRigid(false);
                }
            }
        }
    }
}