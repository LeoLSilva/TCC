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
        obj1.SetStatus(PieceStatus.root);
        target.SetStatus(PieceStatus.conected);
        _partsList.Add(obj1);
        _partsList.Add(target);
    }
    private void CreateJoin(PartsScript obj1, PartsScript target)
    {
        // Pega os Rigidbodies
        Rigidbody rb1 = obj1.GetComponent<Rigidbody>();
        Rigidbody rbTarget = target.GetComponent<Rigidbody>();

        // Configura a Joint no Target
        var j1 = target.gameObject.AddComponent<FixedJoint>();
        j1.connectedBody = rb1;
        j1.breakForce = 100f;

        // CONFIGURA��O DE "CORPO �NICO"
        j1.enablePreprocessing = false; // Trava a posi��o sem c�lculos extras
        j1.massScale = 1;
        j1.connectedMassScale = 1;

        // Repete no Obj1 (conforme sua prefer�ncia de refor�o)
        var j2 = obj1.gameObject.AddComponent<FixedJoint>();
        j2.connectedBody = rbTarget;
        j2.breakForce = 100f;
        j2.enablePreprocessing = false;

        // DICA DE OURO: Garante que o arrasto (Drag) seja igual para n�o balan�ar no ar
        rb1.linearDamping = 0.5f;
        rbTarget.linearDamping = 0.5f;
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

}

