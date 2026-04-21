using UnityEngine;

public class PropellerSpin : MonoBehaviour
{
    [SerializeField] private float _spinSpeed = 1500f;
    [SerializeField] private Vector3 _spinAxis = Vector3.up;

    private void Update()
    {
        transform.Rotate(_spinAxis * (_spinSpeed * Time.deltaTime), Space.Self);
    }
}