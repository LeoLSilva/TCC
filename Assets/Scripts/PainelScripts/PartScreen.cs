using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartScreen : MonoBehaviour
{
    [SerializeField] private List<PartsScriptableObject> _parts = new List<PartsScriptableObject>();

    [Header("Transforms")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _spawnStartPoint;

    [Header("UI & Spawning")]
    [SerializeField] private DiagramManager _diagramManager;
    [SerializeField] private Transform _uiButtonContainer;

    [Header("Optimization")]
    [SerializeField] private GameObject _containerDePecas3D;

    [SerializeField] private int _currentDiagram;
    private List<GameObject> _spawned3DParts = new List<GameObject>();

    private void Start()
    {
        if (_diagramManager == null)
            _diagramManager = FindAnyObjectByType<DiagramManager>();

        if (_containerDePecas3D == null)
            _containerDePecas3D = new GameObject("Container_PartScreen_3D");

        SpawnAllParts();
    }

    private void OnEnable()
    {
        if (_containerDePecas3D != null)
            _containerDePecas3D.SetActive(true);
    }

    private void OnDisable()
    {
        if (_containerDePecas3D != null)
            _containerDePecas3D.SetActive(false);
    }

    private void SpawnAllParts()
    {
        Vector3 startPos = _spawnStartPoint != null ? _spawnStartPoint.position : Vector3.zero;

        for (int i = 0; i < _parts.Count; i++)
        {
            int index = i;
            PartsScriptableObject partData = _parts[i];

            Vector3 spawnPos = startPos;
            spawnPos.x += i * 10f;

            if (partData.diagramScriptable != null)
            {
                PartsScript spawnedObj = _diagramManager.SetDiagram(partData.diagramScriptable, spawnPos);
                if (spawnedObj != null)
                {
                    spawnedObj.SetHierarchyLayerAndPhysics("Mask", true);
                    GameObject pastaDoDiagrama = spawnedObj.transform.parent.gameObject;

                    if (_containerDePecas3D != null)
                    {
                        pastaDoDiagrama.transform.SetParent(_containerDePecas3D.transform, true);
                    }

                    _spawned3DParts.Add(pastaDoDiagrama);
                }
            }

            if (partData.image2D != null && _uiButtonContainer != null)
            {
                GameObject btnObj = new GameObject("Btn_Part_" + index);
                btnObj.transform.SetParent(_uiButtonContainer, false);

                Image img = btnObj.AddComponent<Image>();
                img.sprite = partData.image2D;

                Button btn = btnObj.AddComponent<Button>();
                btn.onClick.AddListener(() => GoToPart(index));
            }
        }

        if (_parts.Count > 0)
        {
            GoToPart(0);
        }
    }

    public void GoToPart(int index)
    {
        if (index < 0 || index >= _parts.Count) return;

        _currentDiagram = index;

        if (_cameraTransform != null)
        {
            Vector3 camPos = _cameraTransform.position;
            float startX = _spawnStartPoint != null ? _spawnStartPoint.position.x : 0f;
            camPos.x = startX + (index * 10f);
            _cameraTransform.position = camPos;
        }
    }
}