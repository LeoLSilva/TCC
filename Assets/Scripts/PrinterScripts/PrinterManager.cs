using System.Collections;
using UnityEngine;

public class PrinterManager : MonoBehaviour
{
    [SerializeField] private PartsScript _currentPart;
    [SerializeField] private DiagramManager _diagramManager;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _isEmpty = true;
    [SerializeField] private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _diagramManager = FindAnyObjectByType<DiagramManager>();
    }
    public void Printer()
    {
        if (_isEmpty)
            StartCoroutine(SpawnPointAnim());
    }
    IEnumerator SpawnPointAnim()
    {
        _isEmpty = false;
        _animator.SetInteger("anim", 1);
        yield return new WaitForSeconds(1f);

        _currentPart = Instantiate(_diagramManager.GetPainelObject().GetComponent<PartsScript>());
        _currentPart.transform.position = _spawnPoint.transform.position;
        _currentPart.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        _currentPart.SetHierarchyLayerAndPhysics("Mask", false);

        yield return new WaitForSeconds(.5f);
        _animator.SetInteger("anim", 2);
        yield return new WaitForSeconds(2.15f);
        _animator.SetInteger("anim", 0);
        _currentPart.SetHierarchyLayerAndPhysics("Default", false);
    }
}
