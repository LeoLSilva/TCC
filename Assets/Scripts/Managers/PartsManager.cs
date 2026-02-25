using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PartsManager : MonoBehaviour
{
    [SerializeField] private PartsScript _root;
    private List<PartsScript> _partsList = new List<PartsScript>();

    public void SetConection(PartsScript obj1, PartsScript target)
    {
        CreateJoin(obj1, target);
        SetList(obj1, target);

    }

    private void CreateJoin(PartsScript obj1, PartsScript target)
    {
        PartsScript root = obj1.GetRigid().mass >= target.GetRigid().mass ? obj1 : target;
        PartsScript connected = root == obj1 ? target : obj1;
        DefineRoot(root, connected);

    }

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

    private void SetList(PartsScript obj1, PartsScript target)
    {
        _partsList.Add(obj1);
        _partsList.Add(target);
    }
    private void DefineRoot(PartsScript root, PartsScript connected)
    {
        root.SetStatus(PieceStatus.root);
        connected.transform.SetParent(root.transform);
        connected.SetStatus(PieceStatus.conected);
    }


}

