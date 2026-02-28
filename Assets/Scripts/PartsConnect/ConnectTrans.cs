using UnityEngine;

public class ConnectTrans : MonoBehaviour
{
    [SerializeField] private Transform _sonTransform;
    [SerializeField] private MeshFilter _meshFilter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _sonTransform = GetComponentInChildren<Transform>();
        _meshFilter = _sonTransform.GetComponent<MeshFilter>();
    }
}
