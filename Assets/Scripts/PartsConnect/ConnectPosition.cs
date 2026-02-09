using UnityEngine;

[System.Serializable]
public class ConnectPosition
{
    [SerializeField] private Vector3 _pos;
    [SerializeField] private Quaternion _rot;

    public void SetPosAndRot(Vector3 pos, Quaternion r)
    {
        _pos = pos;
        _rot = r;
    }
    public Vector3 GetPos() { return _pos; }
    public Quaternion GetRot() { return _rot; }
}
