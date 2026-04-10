using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PartsManager : MonoBehaviour
{
    [SerializeField] private PartsScript _root;
    private List<PartsScript> _partsList = new List<PartsScript>();

    public void SetConection(PartsScript obj1, PartsScript target)
    {
<<<<<<< Updated upstream
        FixedJoint joint = target.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = obj1.GetComponent<Rigidbody>();
        obj1.SetStatus(PieceStatus.root);
        target.SetStatus(PieceStatus.conected);
        _partsList.Add(obj1);
        _partsList.Add(target);
    }
=======
        CreateJoin(obj1, target);
        SetList(obj1, target);
    }

    private void CreateJoin(PartsScript obj1, PartsScript target)
    {
        PartsScript rootOfTarget = target.GetStatus() == PieceStatus.root ? target : target.FindRootPart();
        DefineRoot(rootOfTarget, obj1);
    }

>>>>>>> Stashed changes
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

}

