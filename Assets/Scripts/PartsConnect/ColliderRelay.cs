using System;
using UnityEngine;

public class ColliderRelay : MonoBehaviour
{
    [SerializeField] private SnapIndicator _snap;
    public event Action<Collider> OnCollision;

    public void SetSnap(SnapIndicator snap) { _snap = snap; }
    private void OnTriggerEnter(Collider other)
    {
        OnCollision?.Invoke(other);
    }

    public SnapIndicator GetSnap() { return _snap; }
}
