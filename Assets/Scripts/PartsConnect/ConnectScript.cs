using System.Collections;
using UnityEngine;

public class ConnectScript : MonoBehaviour
{
    private ConnectType _type;
    private Material _material;
    private PartsScript _partScript;
    private Color _natural;
    private Color _green = Color.green;
    private Color _red = Color.red;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        Renderer[] rs = transform.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in rs)
        {
            if (r.transform != transform)
            {
                _material = r.material;
                break;
            }
        }
        _natural = _material.color;
        _partScript = GetComponentInParent<PartsScript>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "con") return;
        ConnectType t = other.GetComponent<ConnectScript>().GetConnectType();
        if (this._type != t)
        {
            _material.color = _green;
            _partScript.SetTarget(other.transform, this);
        }
        else { _material.color = _red; }
    }

    private void OnTriggerExit(Collider other)
    {
        _material.color = _natural;
        _partScript.SetTarget(null, null);
    }
    public ConnectType GetConnectType() { return _type; }
}
