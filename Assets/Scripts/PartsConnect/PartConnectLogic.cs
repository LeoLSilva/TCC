using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartConnectLogic : MonoBehaviour
{
    [SerializeField] private float _detachForce = 3f;
    private float _safeDistanceToRestore = 1.3f;

    [Header("Story Mode Settings")]
    [SerializeField] private bool _bypassCollisionRules = false;

    private PartsScript _partsScript;
    private PartsScript _targetPartScript;
    private ConfigurableJoint _currentJoint;
    private float _originalDrag;
    private float _originalAngularDrag;

    private static PartsManager _partsManager;

    private void Awake()
    {
        _partsScript = GetComponent<PartsScript>();
    }

    private void FixedUpdate()
    {
        if (_currentJoint != null && _partsScript != null && _targetPartScript != null)
        {
            if (_partsScript._isGrabbed || _targetPartScript._isGrabbed)
            {
                if (_currentJoint.currentForce.sqrMagnitude > 22500f)
                {
                    Debug.LogWarning("Segurança Ativada: Tensão excessiva! Soltando as peças da mão.");

                    if (_partsScript._isGrabbed) _partsScript.ForceDrop();
                    if (_targetPartScript._isGrabbed) _targetPartScript.ForceDrop();
                }
            }
        }
    }

    public void InitializePhysics(Rigidbody rigid)
    {
        _originalDrag = rigid.linearDamping;
        _originalAngularDrag = rigid.angularDamping;

        rigid.solverIterations = 10;
        rigid.solverVelocityIterations = 10;
        rigid.maxAngularVelocity = 15f;

        // MUDANÇA: Proíbe coices muito fortes da física (de 3f para 0.5f)
        rigid.maxDepenetrationVelocity = 0.5f;
    }

    public void UpdateBreakForce(bool canBreak)
    {
        if (_currentJoint != null)
        {
            _currentJoint.breakForce = canBreak ? _detachForce : Mathf.Infinity;
        }
    }

    public void SetBypassCollisionRules(bool bypass)
    {
        _bypassCollisionRules = bypass;
    }

    public void StartAnimation(Transform targetTransform, PartsScript targetScript, float timeAnimate)
    {
        _targetPartScript = targetScript;
        StartCoroutine(AnimationCoroutine(targetTransform, timeAnimate));
    }

    private IEnumerator AnimationCoroutine(Transform targetTransform, float timeAnimate)
    {
        _partsScript.ChangeRigid(true);

        if (timeAnimate > 0f)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / timeAnimate;
                transform.position = Vector3.Lerp(startPos, targetTransform.position, t);
                transform.rotation = Quaternion.Slerp(startRot, targetTransform.rotation, t);
                yield return null;
            }
        }

        transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
        CreatePhysicsJoint();
    }

    private void CreatePhysicsJoint()
    {
        if (_targetPartScript != null && _currentJoint == null)
        {
            Rigidbody targetRb = _targetPartScript.GetRigid();
            if (targetRb != null)
            {
                if (_partsManager == null)
                {
                    _partsManager = FindAnyObjectByType<PartsManager>();
                }

                if (_partsManager != null)
                {
                    _partsManager.SetConection(_partsScript, _targetPartScript);
                }

                Transform rootCluster = _targetPartScript.transform.root;
                Collider[] allTargetColliders = rootCluster.GetComponentsInChildren<Collider>();
                UpdateCollisionMatrix(allTargetColliders, true);

                _currentJoint = gameObject.AddComponent<ConfigurableJoint>();
                _currentJoint.connectedBody = targetRb;

                _currentJoint.breakForce = Mathf.Infinity;
                _currentJoint.breakTorque = Mathf.Infinity;

                _currentJoint.enablePreprocessing = false;
                _currentJoint.enableCollision = false;

                _currentJoint.massScale = 1f;
                _currentJoint.connectedMassScale = 1f;

                _currentJoint.projectionMode = JointProjectionMode.None;
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
        if (_currentJoint != null)
        {
            Destroy(_currentJoint);
            _currentJoint = null;
        }

        if (_targetPartScript != null)
        {
            Transform rootCluster = _targetPartScript.transform.root;
            Transform oldTargetTransform = _targetPartScript.transform;

            StartCoroutine(SafeRestoreCollisions(rootCluster, oldTargetTransform));
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

    private IEnumerator SafeRestoreCollisions(Transform rootCluster, Transform oldTarget)
    {
        if (_bypassCollisionRules) yield break;
        if (rootCluster == null) yield break;

        Collider[] myCurrentColliders = GetComponentsInChildren<Collider>(true);
        Collider[] clusterColliders = rootCluster.GetComponentsInChildren<Collider>(true);
        System.Collections.Generic.HashSet<Collider> myCollidersSet = new System.Collections.Generic.HashSet<Collider>(myCurrentColliders);

        foreach (Collider myCol in myCurrentColliders)
        {
            if (myCol == null) continue;
            foreach (Collider targetCol in clusterColliders)
            {
                if (targetCol == null || myCollidersSet.Contains(targetCol)) continue;

                if (myCol.isTrigger || targetCol.isTrigger)
                {
                    Physics.IgnoreCollision(myCol, targetCol, false);
                }
            }
        }

        if (!_bypassCollisionRules)
        {
            float sqrSafeDistance = _safeDistanceToRestore * _safeDistanceToRestore;
            float timeoutTimer = 0f;

            while (oldTarget != null && timeoutTimer < 5f)
            {
                if ((transform.position - oldTarget.position).sqrMagnitude >= sqrSafeDistance)
                {
                    break;
                }

                timeoutTimer += Time.deltaTime;
                yield return null;
            }
        }

        foreach (Collider myCol in myCurrentColliders)
        {
            if (myCol == null) continue;
            foreach (Collider targetCol in clusterColliders)
            {
                if (targetCol == null || myCollidersSet.Contains(targetCol)) continue;

                if (!myCol.isTrigger && !targetCol.isTrigger)
                {
                    Physics.IgnoreCollision(myCol, targetCol, false);
                }
            }
        }
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