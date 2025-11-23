using UnityEngine;

public class ObjectsFurbotScript : MonoBehaviour
{
    [SerializeField] private GameObject _gameObject;
}

public enum Type {
    None,
    enter,
    exit
}
