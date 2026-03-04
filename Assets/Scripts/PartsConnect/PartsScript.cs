using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PartsScript : MonoBehaviour
{
    [SerializeField] private List<SnapIndicator> _connectsScriptList = new List<SnapIndicator>();
    [SerializeField] private PieceStatus _status;
    [SerializeField] private Transform _backPos;

    // Referências do encaixe limpas e diretas
    [SerializeField] private SnapIndicator _snapTarget; // A fêmea (alvo)
    [SerializeField] private SnapIndicator _myActiveSnap; // O macho (nossa peça)
    [SerializeField] private PartsScript _partScriptTarget;

    [SerializeField] private Grabbable _grabble;
    private PartsManager _partsManager;
    private float _timeAnimate = 0.25f;

    [Header("Componente Rigid")]
    [SerializeField] private float _mass;
    private Rigidbody _rigid;

    [Header("Nova Mecânica de Física (Configurable Joint)")]
    [Tooltip("Força que a mão precisa fazer para desencaixar")]
    private float _detachForce = 10f; // Aumentado para VR
    private ConfigurableJoint _currentJoint;

    public bool _isGrabbed = false;
    public UnityEvent<bool> onChangeGrabbleStatus;

    [Header("Testes no inspetor")]
    public bool test = false;
    private PieceStatus _lastStatus;

    private float _originalDrag;
    private float _originalAngularDrag;

    private void OnValidate()
    {
        if (_status == _lastStatus) return;
        _lastStatus = _status;
        SetStatus(_status);
    }

    private void Start()
    {
        _rigid = GetComponent<Rigidbody>();
        _partsManager = FindAnyObjectByType<PartsManager>();
        _grabble = this.GetComponent<Grabbable>();
        _mass = _rigid.mass;

        if (_rigid != null)
        {
            _originalDrag = _rigid.linearDamping;
            _originalAngularDrag = _rigid.angularDamping;

            // Configurações extremas para impedir que o robô trema como gelatina
            _rigid.solverIterations = 20;
            _rigid.solverVelocityIterations = 20;
            _rigid.maxAngularVelocity = 20f;
        }

        AddConectionsOnList();

        if (_grabble != null)
            _grabble.WhenPointerEventRaised += OnGrabbleEvent;
    }

    private void OnDestroy()
    {
        if (_grabble != null)
            _grabble.WhenPointerEventRaised -= OnGrabbleEvent;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) && test)
        {
            BreakPhysicalConnection();
        }

        // Proteção contra quebra acidental (Só quebra se puxar as duas partes ou a base)
        if (_currentJoint != null && _partScriptTarget != null)
        {
            bool isTargetGrabbed = _partScriptTarget._isGrabbed;
            bool isTargetRoot = _partScriptTarget.GetStatus() == PieceStatus.root;
            bool amIRoot = this.GetStatus() == PieceStatus.root;

            bool canBreak = (_isGrabbed && isTargetGrabbed) ||
                            (_isGrabbed && isTargetRoot) ||
                            (isTargetGrabbed && amIRoot);

            _currentJoint.breakForce = canBreak ? _detachForce : Mathf.Infinity;
        }
    }

    private void OnGrabbleEvent(PointerEvent obj)
    {
        if (obj.Type == PointerEventType.Select)
        {
            _isGrabbed = true;
            onChangeGrabbleStatus?.Invoke(true);
        }
        else if (obj.Type == PointerEventType.Unselect)
        {
            _isGrabbed = false;
            onChangeGrabbleStatus?.Invoke(false);

            // SISTEMA DE TRAVA (LOCK): Garante que ninguém roube a fêmea no mesmo milissegundo
            if (_snapTarget != null && _myActiveSnap != null)
            {
                if (!_snapTarget.IsConnected && !_myActiveSnap.IsConnected)
                {
                    // CORREÇÃO: Desliga o holograma ANTES de travar, senão o script ignora o comando!
                    if (_snapTarget.GetConnectType() == ConnectType.famale)
                        _snapTarget.ChangeMesh(null, false);

                    // Trancamos os dois conectores logo em seguida.
                    _snapTarget.IsConnected = true;
                    _myActiveSnap.IsConnected = true;

                    ConnectAnimation();
                }
            }
        }
    }

    private void AddConectionsOnList()
    {
        foreach (Transform child in transform)
        {
            SnapIndicator connect = child.GetComponent<SnapIndicator>();
            if (connect != null)
            {
                _connectsScriptList.Add(connect);
            }
        }
    }

    // NOVA ASSINATURA: Chamada pelo SnapIndicator Macho
    public void SetTarget(SnapIndicator targetSnap, SnapIndicator mySnap)
    {
        _snapTarget = targetSnap;
        _myActiveSnap = mySnap;
        if (targetSnap != null)
        {
            _partScriptTarget = targetSnap.ParentPart;
        }
    }

    // NOVA FUNÇÃO: Limpa o alvo caso o jogador afaste a mão antes de soltar
    public void ClearTarget()
    {
        _snapTarget = null;
        _myActiveSnap = null;
        _partScriptTarget = null;
    }

    public void ConnectAnimation()
    {
        if (_snapTarget == null) return;
        StartCoroutine(AnimationMoveCoroutine2());
    }

    private IEnumerator AnimationMoveCoroutine2()
    {
        ChangeRigid(true);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // Pega a posição e rotação baseadas diretamente no Snap fêmea
        Vector3 targetPos = _snapTarget.transform.position;
        Quaternion targetRot = _snapTarget.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / _timeAnimate;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.SetPositionAndRotation(targetPos, targetRot);
        CreatePhysicsJoint();
    }

    private void CreatePhysicsJoint()
    {
        if (_partScriptTarget != null && _currentJoint == null)
        {
            Rigidbody targetRb = _partScriptTarget.GetRigid();
            if (targetRb != null)
            {
                // Organiza a hierarquia para a Unity entender que é um robô só
                transform.SetParent(_partScriptTarget.transform, true);

                // Ignora colisão com o robô INTEIRO
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

                // Força o snap sem efeito mola
                _currentJoint.projectionMode = JointProjectionMode.PositionAndRotation;
                _currentJoint.projectionDistance = 0.001f;
                _currentJoint.projectionAngle = 0.1f;

                _currentJoint.xMotion = ConfigurableJointMotion.Locked;
                _currentJoint.yMotion = ConfigurableJointMotion.Locked;
                _currentJoint.zMotion = ConfigurableJointMotion.Locked;
                _currentJoint.angularXMotion = ConfigurableJointMotion.Locked;
                _currentJoint.angularYMotion = ConfigurableJointMotion.Locked;
                _currentJoint.angularZMotion = ConfigurableJointMotion.Locked;

                // Aumenta o atrito para não tremer
                if (_rigid != null)
                {
                    _rigid.linearDamping = 1f;
                    _rigid.angularDamping = 3f;
                }
            }
        }

        SetStatus(PieceStatus.conected);
    }

    private void OnJointBreak(float breakForce)
    {
        Debug.Log($"A peça desencaixou! Força aplicada: {breakForce}");
        BreakPhysicalConnection();
    }

    private void BreakPhysicalConnection()
    {
        Transform oldRoot = null;
        if (_partScriptTarget != null)
        {
            oldRoot = _partScriptTarget.transform.root;
        }

        if (_currentJoint != null)
        {
            Destroy(_currentJoint);
            _currentJoint = null;
        }

        // Volta a ser uma peça solta no mundo
        transform.SetParent(null, true);

        // Devolve colisão normal
        if (oldRoot != null)
        {
            Collider[] oldRootColliders = oldRoot.GetComponentsInChildren<Collider>();
            UpdateCollisionMatrix(oldRootColliders, false);
        }

        // Restaura a física normal
        if (_rigid != null)
        {
            _rigid.linearDamping = _originalDrag;
            _rigid.angularDamping = _originalAngularDrag;
        }

        // LIBERA OS CONECTORES PARA SEREM USADOS NOVAMENTE
        if (_snapTarget != null) _snapTarget.IsConnected = false;
        if (_myActiveSnap != null) _myActiveSnap.IsConnected = false;

        ClearTarget();
        SetStatus(PieceStatus.none);
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

    public void SetStatus(PieceStatus st)
    {
        _status = st;
        switch (_status)
        {
            case PieceStatus.none:
            case PieceStatus.conected:
            case PieceStatus.root:
                ChangeRigid(false);
                break;
            case PieceStatus.conecting:
                ChangeRigid(true);
                break;
        }
    }

    private void ChangeRigid(bool isKinematic)
    {
        if (_rigid != null)
        {
            _rigid.isKinematic = isKinematic;
            _rigid.useGravity = !isKinematic;
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        // Reseta totalmente a inércia se cair no chão
        if (col.gameObject.CompareTag("floor"))
        {
            if (_rigid != null)
            {
                _rigid.isKinematic = true;
                this.transform.position = _backPos.position;
                this.transform.rotation = _backPos.rotation;
                _rigid.linearVelocity = Vector3.zero;
                _rigid.angularVelocity = Vector3.zero;
                _rigid.isKinematic = false;
            }
            else
            {
                this.transform.position = _backPos.position;
            }
        }
    }

    #region GetSet
    public PieceStatus GetStatus() { return _status; }
    public Rigidbody GetRigid() { return _rigid; }
    public float GetMass() { return _mass; }
    #endregion
}

public enum ConnectType
{
    none,
    male,
    famale
}



public enum PieceStatus
{
    none,
    conecting,
    conected,
    root
}