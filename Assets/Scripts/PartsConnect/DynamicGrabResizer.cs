using UnityEngine;

public class DynamicGrabResizer : MonoBehaviour
{
    [Header("Configurações de Distância")]
    public float detectionRadius = 0.40f; 
    
    [Header("Tamanhos e Direção")]
    public float normalSizeMultiplier = 1f; 
    public float expandedSizeMultiplier = 2f; 
    public Vector3 expandDirection = new Vector3(0, 0, -1);
    public float smoothSpeed = 15f;

    private BoxCollider _boxCol;
    private Vector3 _originalBoxSize;
    private Vector3 _originalCenter;

    private Transform _leftHand;
    private Transform _rightHand;

    private float _currentMultiplier = 1f;

    private void Start()
    {
        _boxCol = GetComponent<BoxCollider>();
        if (_boxCol != null) 
        {
            _originalBoxSize = _boxCol.size;
            _originalCenter = _boxCol.center;
        }
    }

    private void Update()
    {
        if (_boxCol == null) return;

        // Tenta achar as mãos apenas se ainda não achou
        if (_leftHand == null)
        {
            GameObject left = GameObject.FindGameObjectWithTag("LeftHand");
            if (left != null) _leftHand = left.transform;
        }
        if (_rightHand == null)
        {
            GameObject right = GameObject.FindGameObjectWithTag("RightHand");
            if (right != null) _rightHand = right.transform;
        }

        bool isHandNear = false;
        float sqrDetection = detectionRadius * detectionRadius;

        if (_leftHand != null && (transform.position - _leftHand.position).sqrMagnitude < sqrDetection)
            isHandNear = true;

        if (_rightHand != null && (transform.position - _rightHand.position).sqrMagnitude < sqrDetection)
            isHandNear = true;

        // Interpola o multiplicador
        float targetMult = isHandNear ? expandedSizeMultiplier : normalSizeMultiplier;
        _currentMultiplier = Mathf.Lerp(_currentMultiplier, targetMult, Time.deltaTime * smoothSpeed);

        // Aplica a matemática travada de expansão
        Vector3 dir = expandDirection.normalized; 
        Vector3 absoluteAxis = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z));
        Vector3 sizeIncrease = Vector3.Scale(_originalBoxSize, absoluteAxis) * (_currentMultiplier - 1f);
        
        Vector3 centerOffset = new Vector3(
            sizeIncrease.x * (dir.x != 0 ? Mathf.Sign(dir.x) : 0),
            sizeIncrease.y * (dir.y != 0 ? Mathf.Sign(dir.y) : 0),
            sizeIncrease.z * (dir.z != 0 ? Mathf.Sign(dir.z) : 0)
        ) / 2f;

        _boxCol.size = _originalBoxSize + sizeIncrease;
        _boxCol.center = _originalCenter + centerOffset;
    }
}