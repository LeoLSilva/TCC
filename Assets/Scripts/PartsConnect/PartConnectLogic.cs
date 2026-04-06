using System.Collections;
using UnityEngine;

public class PartConnectLogic : MonoBehaviour
{
    [SerializeField] private float _detachForce = 10f;

    private PartsScript _partsScript;
    private PartsScript _targetPartScript;
    private ConfigurableJoint _currentJoint;
    private float _originalDrag;
    private float _originalAngularDrag;

    private void Awake()
    {
        _partsScript = GetComponent<PartsScript>();
    }

    public void InitializePhysics(Rigidbody rigid)
    {
        _originalDrag = rigid.linearDamping;
        _originalAngularDrag = rigid.angularDamping;

        rigid.solverIterations = 20;
        rigid.solverVelocityIterations = 20;
        rigid.maxAngularVelocity = 20f;
    }

    public void UpdateBreakForce(bool canBreak)
    {
        if (_currentJoint != null)
        {
            _currentJoint.breakForce = canBreak ? _detachForce : Mathf.Infinity;
        }
    }

    public void StartAnimation(Transform targetTransform, PartsScript targetScript, float timeAnimate)
    {
        _targetPartScript = targetScript;
        StartCoroutine(AnimationCoroutine(targetTransform, timeAnimate));
    }

    private IEnumerator AnimationCoroutine(Transform targetTransform, float timeAnimate)
    {
        _partsScript.ChangeRigid(true);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = targetTransform.position;
        Quaternion targetRot = targetTransform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / timeAnimate;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.SetPositionAndRotation(targetPos, targetRot);
        CreatePhysicsJoint();
    }

    private void CreatePhysicsJoint()
    {
        if (_targetPartScript != null && _currentJoint == null)
        {
            Rigidbody targetRb = _targetPartScript.GetRigid();
            if (targetRb != null)
            {
                PartsManager manager = FindAnyObjectByType<PartsManager>();
                if (manager != null)
                {
                    manager.SetConection(_partsScript, _targetPartScript);
                }

                Collider[] allRobotColliders = transform.root.GetComponentsInChildren<Collider>();
                UpdateCollisionMatrix(allRobotColliders, true);

                _currentJoint = gameObject.AddComponent<ConfigurableJoint>();
                _currentJoint.connectedBody = targetRb;

                _currentJoint.breakForce = Mathf.Infinity;
                _currentJoint.breakTorque = Mathf.Infinity;

                _currentJoint.enablePreprocessing = true;
                _currentJoint.enableCollision = false;

                _currentJoint.massScale = 1f;
                _currentJoint.connectedMassScale = 1f;

                _currentJoint.projectionMode = JointProjectionMode.PositionAndRotation;
                _currentJoint.projectionDistance = 0.001f;
                _currentJoint.projectionAngle = 0.1f;

                _currentJoint.xMotion = ConfigurableJointMotion.Locked;
                _currentJoint.yMotion = ConfigurableJointMotion.Locked;
                _currentJoint.zMotion = ConfigurableJointMotion.Locked;
                _currentJoint.angularXMotion = ConfigurableJointMotion.Locked;
                _currentJoint.angularYMotion = ConfigurableJointMotion.Locked;
                _currentJoint.angularZMotion = ConfigurableJointMotion.Locked;

                Rigidbody rigid = _partsScript.GetRigid();
                if (rigid != null)
                {
                    rigid.linearDamping = 1f;
                    rigid.angularDamping = 3f;
                }
            }
        }
    }

    private void OnJointBreak(float breakForce)
    {
        Debug.Log($"Break force: {breakForce}");
        BreakConnection();
    }

    public void BreakConnection()
    {
        Transform oldRoot = null;
        if (_targetPartScript != null)
        {
            oldRoot = _targetPartScript.transform.root;
        }

        if (_currentJoint != null)
        {
            Destroy(_currentJoint);
            _currentJoint = null;
        }

        transform.SetParent(null, true);

        if (oldRoot != null)
        {
            Collider[] oldRootColliders = oldRoot.GetComponentsInChildren<Collider>();
            UpdateCollisionMatrix(oldRootColliders, false);
        }

        Rigidbody rigid = _partsScript.GetRigid();
        if (rigid != null)
        {
            rigid.linearDamping = _originalDrag;
            rigid.angularDamping = _originalAngularDrag;
        }

        _targetPartScript = null;
        _partsScript.HandleConnectionBroken();
    }

    private void UpdateCollisionMatrix(Collider[] targetColliders, bool ignore)
    {
        Collider[] myCurrentColliders = GetComponentsInChildren<Collider>();

        foreach (Collider myCol in myCurrentColliders)
        {
            if (myCol == null || myCol.isTrigger) continue;
            foreach (Collider targetCol in targetColliders)
            {
                if (targetCol == null || targetCol.isTrigger || myCol == targetCol) continue;
                Physics.IgnoreCollision(myCol, targetCol, ignore);
            }
        }
    }
}