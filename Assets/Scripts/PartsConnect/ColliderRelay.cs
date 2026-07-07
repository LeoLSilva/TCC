using System;
using UnityEngine;

public class ColliderRelay : MonoBehaviour
{
    [SerializeField] private SnapIndicator _snap;

    public event Action<Collider> OnCollision;
    public event Action<Collider> OnCollisionExit;

    public void SetSnap(SnapIndicator snap) { _snap = snap; }

    private void OnTriggerEnter(Collider other)
    {
        OnCollision?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        OnCollisionExit?.Invoke(other);
    }

    public SnapIndicator GetSnap() { return _snap; }
}