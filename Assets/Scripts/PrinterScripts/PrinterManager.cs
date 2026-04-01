using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PrinterManager : MonoBehaviour
{
    [SerializeField] private PartsScript _currentPart;
    [SerializeField] private DiagramScreen _diagramScreen;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _isEmpty = true;
    [SerializeField] private Animator _animator;

    [SerializeField] private GameObject _wallPrinter;
    [Header("Collider")]
    [SerializeField] private bool _objectInPrinter;

    public event Action<bool> OnPrinterStateChanged;

    private void Start()
    {
        _wallPrinter.SetActive(false);
        _animator = GetComponent<Animator>();
        _diagramScreen = FindAnyObjectByType<DiagramScreen>();
    }
    public void Printer(GameObject obj)
    {
        if (_isEmpty)
        {
            _currentPart = obj.GetComponent<PartsScript>();
            StartCoroutine(SpawnPointAnim());
        }
    }
    IEnumerator SpawnPointAnim()
    {
        _wallPrinter.SetActive(true);
        _isEmpty = false;
        _animator.SetInteger("anim", 1);
        yield return new WaitForSeconds(1f);

        _currentPart = Instantiate(_currentPart);
        _currentPart.transform.position = _spawnPoint.transform.position;
        _currentPart.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        _currentPart.SetHierarchyLayerAndPhysics("Mask", false);

        yield return new WaitForSeconds(.5f);
        _animator.SetInteger("anim", 2);
        yield return new WaitForSeconds(2.15f);
        _animator.SetInteger("anim", 0);
        _currentPart.AddComponent<TempObjPrinter>();
        _wallPrinter.SetActive(false);
    }


    public void SetObjectsInPrinter(bool currentState)
    {
        _objectInPrinter = currentState;
        _isEmpty = !_objectInPrinter;
        OnPrinterStateChanged?.Invoke(_objectInPrinter);
    }
}
