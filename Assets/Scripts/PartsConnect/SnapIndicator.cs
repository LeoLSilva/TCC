using System;
using Unity.VisualScripting;
using UnityEngine;

public class SnapIndicator : MonoBehaviour
{
    [SerializeField] private MeshRenderer _indicator;
    [SerializeField] private ConnectType _type;
    private PartsScript _partScript;
    //private Mesh _indicatorMesh;
    //private MeshFilter _indicatorMeshFilter;
    private void Start()
    {
        _indicator = GetComponent<MeshRenderer>();
        _indicator.enabled = false;
        /*_indicatorMeshFilter = _indicator.GetComponent<MeshFilter>();
        _indicatorMesh = _indicatorMeshFilter.mesh;*/
        _partScript = GetComponentInParent<PartsScript>();
        if (_type == ConnectType.male)
        {
            GetComponent<Collider>().enabled = false;
            ColliderRelay relay = transform.parent.GetComponent<ColliderRelay>();
            if (relay == null) { relay = transform.parent.gameObject.AddComponent<ColliderRelay>(); }
            relay.SetSnap(this);
            relay.OnCollision += OnTriggerEnter;
        }
        else
            GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "obj" || !this._partScript._isGrabbed) return;
        SnapIndicator otherSnap = other.GetComponent<SnapIndicator>();
        if (otherSnap == null) { otherSnap = other.GetComponent<ColliderRelay>().GetSnap(); }
        if (otherSnap.GetConnectType() == GetConnectType()) return;
        if (this._type == ConnectType.male)
        {
            _partScript.SetTarget(other.transform, this, (_type == ConnectType.male));
        }
        else
            ChangeMesh(other.gameObject);
    }

    public ConnectType GetConnectType() { return _type; }

    private void OnTriggerExit(Collider other)
    {
        _partScript.SetTarget(null, null, false);
        _indicator.enabled = false;
    }

    private void ChangeMesh(GameObject obj)
    {
        Vector3 escalaAlvo = obj.transform.lossyScale;
        if (transform.parent != null)
        {
            Vector3 escalaDoPai = transform.parent.lossyScale;

            transform.localScale = new Vector3(
                escalaAlvo.x / escalaDoPai.x,
                escalaAlvo.y / escalaDoPai.y,
                escalaAlvo.z / escalaDoPai.z
            );
        }
        else
        {
            transform.localScale = escalaAlvo;
        }
        _indicator.enabled = true;
        //_indicatorMesh = mesh;
        //_indicatorMeshFilter.mesh = _indicatorMesh;
    }
}
