using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

// ----------------------------------------------------------------------------
// Classe auxiliar baseada no seu script para armazenar a posição/rotação
// ----------------------------------------------------------------------------
public class PartConnectPosition
{
    public Vector3 position;
    public Quaternion rotation;

    public void SetPosAndRot(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}

// ----------------------------------------------------------------------------
// Coloque este script em todas as peças de montar (os braços, pernas, etc.)
// ----------------------------------------------------------------------------
[RequireComponent(typeof(Rigidbody))]
public class RobotSnappablePart : MonoBehaviour
{
    [Header("Configurações da Peça")]
    [Tooltip("O tipo dessa peça para combinar com o SnapPoint (ex: 'Braco')")]
    public string myPartType = "Qualquer";

    [Tooltip("O Transform filho que representa exatamente a ponta/conector desta peça. Se vazio, usa o centro da peça.")]
    public Transform myConnectorPoint;

    [Tooltip("Distância máxima para o encaixe acontecer")]
    public float snapDistance = 0.15f;

    [Tooltip("Força necessária para quebrar o encaixe ao puxar as peças para lados opostos")]
    public float detachForce = 1f; // REDUZIDO

    private Rigidbody rb;
    private Grabbable grabbable;
    private Collider[] myColliders;
    private ConfigurableJoint currentJoint;

    private bool isGrabbed = false;
    private bool isSnapped = false;
    private RobotSnapPoint currentSnapPoint = null;

    private List<RobotSnapPoint> nearbySnapPoints = new List<RobotSnapPoint>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();
        myColliders = GetComponentsInChildren<Collider>();

        if (myConnectorPoint == null)
        {
            myConnectorPoint = transform;
        }
    }

    private void OnEnable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += HandlePointerEvent;
        }
    }

    private void OnDisable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= HandlePointerEvent;
        }
    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            OnPartGrabbed();
        }
        else if (evt.Type == PointerEventType.Unselect)
        {
            OnPartReleased();
        }
    }

    public void OnPartGrabbed()
    {
        isGrabbed = true;
    }

    public void OnPartReleased()
    {
        isGrabbed = false;

        if (!isSnapped)
        {
            RobotSnapPoint bestSnapPoint = FindBestSnapPoint();

            if (bestSnapPoint != null)
            {
                SnapTo(bestSnapPoint);
            }
        }
    }

    // ------------------------------------------------------------------------
    // CÁLCULO DE POSIÇÃO PERSONALIZADO
    // ------------------------------------------------------------------------

    public PartConnectPosition CalculatingPosition(Transform father, Transform target, Transform conSon)
    {
        if (father == null || target == null || conSon == null)
            return null;

        Quaternion rot = target.rotation * Quaternion.Inverse(conSon.localRotation);

        Vector3 scaledLocal = Vector3.Scale(conSon.localPosition, father.lossyScale);

        Vector3 pos = target.position - (rot * scaledLocal);

        PartConnectPosition p = new PartConnectPosition();
        p.SetPosAndRot(pos, rot);
        return p;
    }

    // ------------------------------------------------------------------------
    // LÓGICA DE FÍSICA E QUEBRA (FixedJoint)
    // ------------------------------------------------------------------------

    private void OnJointBreak(float breakForce)
    {
        Debug.Log($"A peça desencaixou! Força aplicada pelas mãos: {breakForce}");
        UnsnapPart();
    }

    // ------------------------------------------------------------------------
    // LÓGICA DE DETECÇÃO (TRIGGERS)
    // ------------------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        RobotSnapPoint snapPoint = other.GetComponent<RobotSnapPoint>();
        if (snapPoint != null && !snapPoint.isOccupied && !nearbySnapPoints.Contains(snapPoint))
        {
            if (snapPoint.partType == "Qualquer" || snapPoint.partType == myPartType)
            {
                nearbySnapPoints.Add(snapPoint);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        RobotSnapPoint snapPoint = other.GetComponent<RobotSnapPoint>();
        if (snapPoint != null && nearbySnapPoints.Contains(snapPoint))
        {
            nearbySnapPoints.Remove(snapPoint);
        }
    }

    // ------------------------------------------------------------------------
    // LÓGICA DE ENCAIXE E DESENCAIXE
    // ------------------------------------------------------------------------

    private RobotSnapPoint FindBestSnapPoint()
    {
        RobotSnapPoint bestPoint = null;
        float closestDistance = snapDistance;

        nearbySnapPoints.RemoveAll(item => item == null);

        foreach (var point in nearbySnapPoints)
        {
            if (point.isOccupied) continue;

            float distance = Vector3.Distance(myConnectorPoint.position, point.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestPoint = point;
            }
        }

        return bestPoint;
    }

    private void SnapTo(RobotSnapPoint targetPoint)
    {
        isSnapped = true;
        currentSnapPoint = targetPoint;
        targetPoint.isOccupied = true;

        Collider[] baseColliders = targetPoint.GetComponentsInParent<Collider>();
        foreach (Collider myCol in myColliders)
        {
            if (myCol.isTrigger) continue;
            foreach (Collider baseCol in baseColliders)
            {
                if (baseCol.isTrigger) continue;
                Physics.IgnoreCollision(myCol, baseCol, true);
            }
        }

        PartConnectPosition newPos = CalculatingPosition(transform, targetPoint.transform, myConnectorPoint);
        if (newPos != null)
        {
            transform.position = newPos.position;
            transform.rotation = newPos.rotation;
        }

        Rigidbody baseRb = targetPoint.GetComponentInParent<Rigidbody>();
        if (baseRb != null)
        {
            currentJoint = gameObject.AddComponent<ConfigurableJoint>();
            currentJoint.connectedBody = baseRb;

            currentJoint.breakForce = detachForce;
            currentJoint.breakTorque = detachForce;

            currentJoint.enablePreprocessing = false;
            currentJoint.enableCollision = false;

            currentJoint.xMotion = ConfigurableJointMotion.Locked;
            currentJoint.yMotion = ConfigurableJointMotion.Locked;
            currentJoint.zMotion = ConfigurableJointMotion.Locked;
            currentJoint.angularXMotion = ConfigurableJointMotion.Locked;
            currentJoint.angularYMotion = ConfigurableJointMotion.Locked;
            currentJoint.angularZMotion = ConfigurableJointMotion.Locked;
        }

        nearbySnapPoints.Clear();
        Debug.Log($"Peça FIXADA com ConfigurableJoint no ponto: {targetPoint.gameObject.name}");
    }

    private void UnsnapPart()
    {
        isSnapped = false;

        if (currentJoint != null)
        {
            Destroy(currentJoint);
            currentJoint = null;
        }

        if (currentSnapPoint != null)
        {
            Collider[] baseColliders = currentSnapPoint.GetComponentsInParent<Collider>();
            foreach (Collider myCol in myColliders)
            {
                if (myCol.isTrigger) continue;
                foreach (Collider baseCol in baseColliders)
                {
                    if (baseCol.isTrigger) continue;
                    Physics.IgnoreCollision(myCol, baseCol, false);
                }
            }

            currentSnapPoint.isOccupied = false;
            currentSnapPoint = null;
        }
    }
}