using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PrinterManager : MonoBehaviour
{
    [SerializeField] private GameObject _currentPartObject;
    [SerializeField] private DiagramScreen _diagramScreen;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _isEmpty = true;
    [SerializeField] private Animator _animator;

    [SerializeField] private GameObject _wallPrinter;
    [Header("Collider")]
    [SerializeField] private bool _objectInPrinter;

    public event Action<bool> OnPrinterStateChanged;

    private DiagramManager _diagramManager;
    private bool _isPrintingDiagram = false;
    private DiagramScriptableObject _currentDiagramSO;

    private void Start()
    {
        _wallPrinter.SetActive(false);
        _animator = GetComponent<Animator>();
        _diagramScreen = FindAnyObjectByType<DiagramScreen>();
        _diagramManager = FindAnyObjectByType<DiagramManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Printer(_currentPartObject);
        }
    }

    public void Printer(GameObject obj)
    {
        if (_isEmpty)
        {
            if (obj.transform.parent)
            {
                _currentPartObject = obj.transform.parent.gameObject;
            }
            else
            {
                _currentPartObject = obj;
            }

            _isPrintingDiagram = false;
            StartCoroutine(SpawnPointAnim());
        }
    }

    public void PrinterDiagram(DiagramScriptableObject diagramSO)
    {
        if (_isEmpty && diagramSO != null)
        {
            _currentDiagramSO = diagramSO;
            _isPrintingDiagram = true;
            StartCoroutine(SpawnPointAnim());
        }
    }

    IEnumerator SpawnPointAnim()
    {
        _wallPrinter.SetActive(true);
        _isEmpty = false;
        _animator.SetInteger("anim", 1);
        yield return new WaitForSeconds(1f);

        if (_isPrintingDiagram && _currentDiagramSO != null && _diagramManager != null)
        {
            PartsScript spawnedPart = _diagramManager.SetDiagram(_currentDiagramSO, _spawnPoint.position);

            if (spawnedPart != null)
            {
                DiagramRegister reg = spawnedPart.GetComponent<DiagramRegister>();

                if (spawnedPart.transform.parent != null)
                {
                    _currentPartObject = spawnedPart.transform.parent.gameObject;
                }
                else
                {
                    _currentPartObject = spawnedPart.gameObject;
                }

                _currentPartObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
        else if (_currentPartObject != null)
        {
            _currentPartObject = Instantiate(_currentPartObject);
            _currentPartObject.transform.position = _spawnPoint.transform.position;
            _currentPartObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        if (_currentPartObject != null)
        {
            PartsScript[] allSpawnedParts = _currentPartObject.GetComponentsInChildren<PartsScript>();
            int maskLayer = LayerMask.NameToLayer("Mask");

            foreach (var part in allSpawnedParts)
            {
                part.gameObject.layer = maskLayer;
                part.ChangeRigid(false);

                Rigidbody rb = part.GetRigid();
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints.FreezePositionX |
                                     RigidbodyConstraints.FreezePositionZ |
                                     RigidbodyConstraints.FreezeRotation;
                }
            }
        }

        yield return new WaitForSeconds(.5f);
        _animator.SetInteger("anim", 2);
        yield return new WaitForSeconds(3.15f);
        _animator.SetInteger("anim", 0);

        if (_currentPartObject != null)
        {
            PartsScript[] allSpawnedParts = _currentPartObject.GetComponentsInChildren<PartsScript>();
            foreach (var part in allSpawnedParts)
            {
                Rigidbody rb = part.GetRigid();
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints.None;
                }

                if (part.gameObject.GetComponent<TempObjPrinter>() == null)
                {
                    part.gameObject.AddComponent<TempObjPrinter>();
                }
            }
        }

        _wallPrinter.SetActive(false);
        _isPrintingDiagram = false;
        _currentDiagramSO = null;
    }

    public void SetObjectsInPrinter(bool currentState)
    {
        _objectInPrinter = currentState;
        _isEmpty = !_objectInPrinter;
        OnPrinterStateChanged?.Invoke(_objectInPrinter);
    }
}