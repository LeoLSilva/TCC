using System;
using UnityEngine;

public class SnapIndicator : MonoBehaviour
{
    [SerializeField] private MeshRenderer _indicator;
    [SerializeField] private ConnectType _type;

    private PartsScript _parentPart;
    private bool _isConnected = false;
    private SnapIndicator _currentHover;

    private void Start()
    {
        _indicator = GetComponent<MeshRenderer>();
        if (_indicator != null) _indicator.enabled = false;

        _parentPart = GetComponentInParent<PartsScript>();

        InitializeColliderRelay();
    }

    private void InitializeColliderRelay()
    {
        if (_type == ConnectType.male)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            ColliderRelay relay = transform.parent.GetComponent<ColliderRelay>();
            if (relay == null)
            {
                relay = transform.parent.gameObject.AddComponent<ColliderRelay>();
            }
            relay.SetSnap(this);

            relay.OnCollision += HandleTriggerEnter;
            relay.OnCollisionExit += HandleTriggerExit;
        }
        else
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
    }

    public void HandleTriggerEnter(Collider other)
    {
        if (_isConnected || (!other.CompareTag("obj") && !other.CompareTag("objSnap"))) return;
        if (_parentPart != null && _parentPart.GetStatus() == PieceStatus.conecting) return;

        SnapIndicator otherSnap = ResolveSnapIndicator(other);
        if (otherSnap == null || otherSnap._isConnected || otherSnap.GetConnectType() == _type) return;

        _currentHover = otherSnap;

        if (_type == ConnectType.male && _parentPart != null && _parentPart._isGrabbed)
        {
            _parentPart.SetupConnectionData(otherSnap, this);
        }
        else if (_type == ConnectType.famale && otherSnap._parentPart != null && otherSnap._parentPart._isGrabbed)
        {
            ChangeMesh(other.gameObject, true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleTriggerEnter(other);
    }

    public void HandleTriggerExit(Collider other)
    {
        if (_currentHover == null) return;
        if (_parentPart != null && _parentPart.GetStatus() == PieceStatus.conecting) return;

        SnapIndicator otherSnap = ResolveSnapIndicator(other);

        if (otherSnap == _currentHover)
        {
            if (_type == ConnectType.male && !_isConnected && _parentPart != null)
            {
                _parentPart.ClearConnectionData();
            }
            else if (_type == ConnectType.famale && !_isConnected)
            {
                ChangeMesh(null, false);
            }
            _currentHover = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        HandleTriggerExit(other);
    }

    private SnapIndicator ResolveSnapIndicator(Collider col)
    {
        SnapIndicator snap = col.GetComponent<SnapIndicator>();
        if (snap == null)
        {
            ColliderRelay relay = col.GetComponentInParent<ColliderRelay>();
            if (relay != null) snap = relay.GetSnap();
        }
        return snap;
    }

    public void ChangeMesh(GameObject obj, bool show)
    {
        if (_indicator == null || _isConnected) return;

        if (show && obj != null)
        {
            Vector3 targetScale = obj.transform.lossyScale;
            if (transform.parent != null)
            {
                Vector3 parentScale = transform.parent.lossyScale;
                transform.localScale = new Vector3(
                    targetScale.x / parentScale.x,
                    targetScale.y / parentScale.y,
                    targetScale.z / parentScale.z
                );
            }
            else
            {
                transform.localScale = targetScale;
            }
            _indicator.enabled = true;
        }
        else
        {
            _indicator.enabled = false;
        }
    }

    public ConnectType GetConnectType() { return _type; }
    public bool GetIsConnect() { return _isConnected; }
    public void SetIsConnect(bool con)
    {
        _isConnected = con;
        this.gameObject.SetActive(!con);
    }
    public PartsScript GetPartScript() { return _parentPart; }
    public void SetPartScript(PartsScript part) { _parentPart = part; }
}