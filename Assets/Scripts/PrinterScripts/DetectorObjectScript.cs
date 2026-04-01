using UnityEngine;

public class DetectorObjectScript : MonoBehaviour
{
    [Header("Configurações")]
    public string tagObject = "obj";

    public PrinterManager printerManager;

    private BoxCollider _myCollider;
    private bool _currentState = false;

    void Start()
    {
        _myCollider = GetComponent<BoxCollider>();
        _myCollider.isTrigger = true;
    }

    void FixedUpdate()
    {
        Collider[] objetosEncontrados = Physics.OverlapBox(
            _myCollider.bounds.center,
            _myCollider.bounds.extents,
            transform.rotation
        );

        bool achouAlgum = false;

        foreach (Collider col in objetosEncontrados)
        {
            if (col == _myCollider) continue;

            if (VerificarTagEHierarquia(col.gameObject))
            {
                achouAlgum = true;
                break;
            }
        }

        if (achouAlgum != _currentState)
        {
            _currentState = achouAlgum;

            if (printerManager != null)
            {
                printerManager.SetObjectsInPrinter(_currentState);
            }
        }
    }

    private bool VerificarTagEHierarquia(GameObject obj)
    {
        if (obj.CompareTag(tagObject)) return true;

        Transform pai = obj.transform.parent;
        while (pai != null)
        {
            if (pai.CompareTag(tagObject)) return true;
            pai = pai.parent;
        }

        return false;
    }
}