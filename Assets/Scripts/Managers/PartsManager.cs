using UnityEngine;

public class PartsManager : MonoBehaviour
{
    [SerializeField] private bool _inConnecting = false;


    public ConnectPosition CalculatingPosition(Transform father, Transform target, Transform conSon)
    {
        if (father == null || target == null || conSon == null)
            return null;

        Quaternion rot =
            target.rotation * Quaternion.Inverse(conSon.localRotation);

        Vector3 scaledLocal =
            Vector3.Scale(conSon.localPosition, father.lossyScale);

        Vector3 pos =
            target.position - (rot * scaledLocal);

        ConnectPosition p = new ConnectPosition();
        p.SetPosAndRot(pos, rot);
        return p;
    }




    public void SetInConnecting(bool inConnecting) { _inConnecting = inConnecting; }
    public bool GetInConnecting() { return _inConnecting; }
}

