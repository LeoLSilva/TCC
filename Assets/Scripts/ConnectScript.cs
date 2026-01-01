using System.Collections;
using UnityEngine;

public class ConnectScript : MonoBehaviour
{
    [SerializeField] private ConnectType _type;
    [SerializeField] private PartsScript _partScript;
    [SerializeField] private Collider _collider;

    private void Start()
    {
        _partScript = GetComponentInParent<PartsScript>();
    }
    private void OnTriggerEnter(Collider other)
    {
        ConnectType t = other.GetComponent<ConnectScript>().GetConnectType();
        Debug.Log("Tipo: " + _type.ToString() + "\nOutro Tipo: " + t);
        if (this._type != t)
        {
            _partScript.SetTarget(other.transform, this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _partScript.SetTarget(null, null);
    }
    public ConnectType GetConnectType() { return _type; }
}
