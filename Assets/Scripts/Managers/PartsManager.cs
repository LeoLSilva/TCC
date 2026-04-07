using System;
using System.Collections.Generic;
using UnityEngine;

public class PartsManager : MonoBehaviour
{
    [SerializeField] private PartsScript _root;
    [SerializeField] private PartsScript _grabble;
    [SerializeField] private AutoReleaseOnForce _auto;
    private List<PartsScript> _partsList = new List<PartsScript>();

    private void OnEnable()
    {
        PartsScript.OnPartGrabbed += HandlePartGrabbed;
    }
    private void OnDisable()
    {
        PartsScript.OnPartGrabbed -= HandlePartGrabbed;
    }
    private void HandlePartGrabbed(PartsScript part)
    {
        if (part._isGrabbed)
        {
            _grabble = part;
        }
        else
            _grabble = null;
    }

    public void SetConection(PartsScript obj1, PartsScript target)
    {
        CreateJoin(obj1, target);
        SetList(obj1, target);

    }

    private void CreateJoin(PartsScript obj1, PartsScript target)
    {
        PartsScript root;

        if (obj1.GetStatus() == PieceStatus.root) { root = obj1; }
        else if (target.GetStatus() == PieceStatus.root) { root = target; }
        else { root = obj1.GetRigid().mass >= target.GetRigid().mass ? obj1 : target; }

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
        connected.SetStatus(PieceStatus.conected);
    }
}

[Serializable]
public struct PartSnapPair
{
    public GameObject part;
    public GameObject target;
    public int snap;
}