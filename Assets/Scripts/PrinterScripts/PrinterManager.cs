using System;
using System.Collections;
using UnityEngine;

public class PrinterManager : MonoBehaviour
{
    [SerializeField] private PartsScript _currentPart;
    [SerializeField] private GameObject _currentPartObject;
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
        if (_animator == null) _animator = GetComponent<Animator>();
    }

    public void Printer(GameObject obj)
    {
        if (!_isEmpty) return;

        if (obj.transform.parent != null && obj.transform.parent.name.Contains("DiagramContainer"))
        {
            _currentPartObject = obj.transform.parent.gameObject;
        }
        else
        {
            _currentPartObject = obj;
        }

        StartCoroutine(SpawnPointAnim());
    }

    IEnumerator SpawnPointAnim()
    {
        _wallPrinter.SetActive(true);
        _isEmpty = false;
        _animator.SetInteger("anim", 1);

        yield return new WaitForSeconds(1f);

        bool originalState = _currentPartObject.activeSelf;
        _currentPartObject.SetActive(false);

        GameObject clone = Instantiate(_currentPartObject, _spawnPoint.position, _spawnPoint.rotation);
        clone.transform.localScale = Vector3.one;

        _currentPartObject.SetActive(originalState);

        clone.SetActive(true);

        PartsScript[] allParts = clone.GetComponentsInChildren<PartsScript>();

        foreach (var p in allParts)
        {
            p.gameObject.layer = LayerMask.NameToLayer("Mask");
            p.ChangeRigid(false);

            if (p.GetStatus() == PieceStatus.root)
            {
                _currentPart = p;
            }
        }

        if (_currentPart == null && allParts.Length > 0)
        {
            _currentPart = allParts[0];
        }

        yield return new WaitForSeconds(.5f);
        _animator.SetInteger("anim", 2);
        yield return new WaitForSeconds(2.15f);
        _animator.SetInteger("anim", 0);

        if (_currentPart != null)
        {
            _currentPart.gameObject.AddComponent<TempObjPrinter>();
        }

        _wallPrinter.SetActive(false);
    }

    public void SetObjectsInPrinter(bool currentState)
    {
        _objectInPrinter = currentState;
        _isEmpty = !_objectInPrinter;
        OnPrinterStateChanged?.Invoke(_objectInPrinter);
    }
}