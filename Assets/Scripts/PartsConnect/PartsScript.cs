using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartsScript : MonoBehaviour
{
    [SerializeField] private List<ConnectScript> _connectsScriptList = new List<ConnectScript>();
    [SerializeField] private PieceStatus _status;
    [SerializeField] private Transform _backPos;

    // Referências do encaixe
    [SerializeField] private PartsScript _partScriptTarget;
    [SerializeField] private Transform _conectorTarget;
    private Transform _connectorTransform;

    [SerializeField] private Grabbable _grabble;
    private PartsManager _partsManager;
    private float _timeAnimate = 0.25f;

    [Header("Componente Rigid")]
    [SerializeField] private float _mass = 1f;
    private Rigidbody _rigid;

    [Header("Nova Mecânica de Física (Configurable Joint)")]
    [Tooltip("Força que a mão precisa fazer para desencaixar")]
    [SerializeField] private float _detachForce = 100f;
    private ConfigurableJoint _currentJoint;
    private Collider[] _myColliders;

    public bool _isGrabbed = false; // Deteta se a mão está a segurar esta peça

    [Header("Testes no inspetor")]
    public bool test = false;
    private PieceStatus _lastStatus;

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
        _myColliders = GetComponentsInChildren<Collider>(); // Guarda colisores para ignorar durante o encaixe

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
        if (Input.GetKeyDown(KeyCode.S) && test && _conectorTarget != null && _conectorTarget.GetComponent<ConnectScript>().GetConnectType() == ConnectType.famale)
        {
            ConnectAnimation();
        }
        if (Input.GetKeyDown(KeyCode.D) && test)
        {
            BreakPhysicalConnection(); // Força a quebra para teste
        }

        // ====================================================================
        // NOVA LÓGICA: Proteção Inteligente contra Quebra (Mesa/Chão e Inércia)
        // ====================================================================
        if (_currentJoint != null && _partScriptTarget != null)
        {
            bool isTargetGrabbed = _partScriptTarget._isGrabbed;
            bool isTargetRoot = _partScriptTarget.GetStatus() == PieceStatus.root;
            bool amIRoot = this.GetStatus() == PieceStatus.root;

            // A junta SÓ se torna quebrável se:
            // 1. O jogador estiver a segurar as DUAS peças ao mesmo tempo (puxar uma de cada lado).
            // 2. OU o jogador estiver a segurar uma peça e a outra for o 'root' (base fixa).
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
            _isGrabbed = true; // Estou a segurar a peça
        }
        else if (obj.Type == PointerEventType.Unselect)
        {
            _isGrabbed = false; // Soltei a peça

            // Tenta encaixar se houver um alvo detetado pelos ConnectScripts
            if (_conectorTarget != null && _conectorTarget.GetComponent<ConnectScript>().GetConnectType() != ConnectType.male)
            {
                ConnectAnimation();
            }
        }
    }

    private void AddConectionsOnList()
    {
        foreach (Transform child in transform)
        {
            ConnectScript connect = child.GetComponent<ConnectScript>();
            if (connect != null)
            {
                _connectsScriptList.Add(connect);
            }
        }
    }

    public void SetTarget(Transform target, ConnectScript con)
    {
        _conectorTarget = target;
        _connectorTransform = null;
        if (target != null && con != null)
        {
            _partScriptTarget = _conectorTarget.parent.GetComponent<PartsScript>();
            _connectorTransform = _connectsScriptList[_connectsScriptList.IndexOf(con)].transform;
        }
    }

    public void ConnectAnimation()
    {
        if (_status == PieceStatus.conecting || _status == PieceStatus.conected) return;
        if (_conectorTarget == null || _connectorTransform == null) return;

        SetStatus(PieceStatus.conecting);

        // Mantém a sua excelente lógica de cálculo de posição
        ConnectPosition cp = _partsManager.CalculatingPosition(transform, _conectorTarget, _connectorTransform);
        if (cp == null) return;

        StartCoroutine(AnimationMoveCoroutine(cp));
    }

    private IEnumerator AnimationMoveCoroutine(ConnectPosition cp)
    {
        // 1. Durante a animação, remove a gravidade e física para ela flutuar suavemente
        ChangeRigid(true);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 targetPos = cp.GetPos();
        Quaternion targetRot = cp.GetRot();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / _timeAnimate;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.SetPositionAndRotation(targetPos, targetRot);

        // ====================================================================
        // SUBSTITUIÇÃO DA LÓGICA ANTIGA (Ponto Chave!)
        // ====================================================================
        // Removido: _partsManager.SetConection(this, _partScriptTarget);
        // Não usamos mais parenting ou managers para "travar" a peça.
        // A partir de agora, o ConfigurableJoint é a ÚNICA coisa que as une.
        CreatePhysicsJoint();
    }

    // ========================================================================
    // LÓGICA NOVA: JOINT FÍSICO (Como nos "Dois Cubos")
    // ========================================================================
    private void CreatePhysicsJoint()
    {
        if (_partScriptTarget != null)
        {
            Rigidbody targetRb = _partScriptTarget.GetRigid();
            if (targetRb != null)
            {
                // Ignora colisão entre as duas peças para não repelirem
                Collider[] targetColliders = _partScriptTarget.GetComponentsInChildren<Collider>();
                IgnoreCollisionsWith(targetColliders, true);

                // Cria a Solda Física
                _currentJoint = gameObject.AddComponent<ConfigurableJoint>();
                _currentJoint.connectedBody = targetRb;

                // Forças e estabilidade
                _currentJoint.breakForce = Mathf.Infinity; // Começa infinito (proteção de impacto)
                _currentJoint.breakTorque = Mathf.Infinity; // Torque infinito para não soltar ao girar

                _currentJoint.enablePreprocessing = false;
                _currentJoint.enableCollision = false;

                // Trava todos os movimentos da junta, imitando um Lego
                _currentJoint.xMotion = ConfigurableJointMotion.Locked;
                _currentJoint.yMotion = ConfigurableJointMotion.Locked;
                _currentJoint.zMotion = ConfigurableJointMotion.Locked;
                _currentJoint.angularXMotion = ConfigurableJointMotion.Locked;
                _currentJoint.angularYMotion = ConfigurableJointMotion.Locked;
                _currentJoint.angularZMotion = ConfigurableJointMotion.Locked;
            }
        }

        // Finaliza o encaixe, reativando a física para o corpo agir como um só
        SetStatus(PieceStatus.conected);
    }

    // Chamado automaticamente pela Unity quando a força _detachForce é superada pelas mãos do jogador
    private void OnJointBreak(float breakForce)
    {
        Debug.Log($"A peça desencaixou! Força aplicada: {breakForce}");
        BreakPhysicalConnection();
    }

    private void BreakPhysicalConnection()
    {
        if (_currentJoint != null)
        {
            Destroy(_currentJoint);
            _currentJoint = null;
        }

        if (_partScriptTarget != null)
        {
            // Restaura as colisões quando separadas
            Collider[] targetColliders = _partScriptTarget.GetComponentsInChildren<Collider>();
            IgnoreCollisionsWith(targetColliders, false);
        }

        _partScriptTarget = null;
        _conectorTarget = null;
        _connectorTransform = null;

        SetStatus(PieceStatus.none);
    }

    private void IgnoreCollisionsWith(Collider[] targetColliders, bool ignore)
    {
        foreach (Collider myCol in _myColliders)
        {
            if (myCol == null || myCol.isTrigger) continue; // Ignora BoxColliders dos conectores
            foreach (Collider targetCol in targetColliders)
            {
                if (targetCol == null || targetCol.isTrigger) continue;
                Physics.IgnoreCollision(myCol, targetCol, ignore);
            }
        }
    }

    // ========================================================================
    // STATUS E FÍSICA
    // ========================================================================
    public void SetStatus(PieceStatus st)
    {
        _status = st;
        switch (_status)
        {
            case PieceStatus.none:
                ChangeRigid(false); // Solta, física normal
                break;
            case PieceStatus.conecting:
                ChangeRigid(true); // Kinematic durante a animação para não cair
                break;
            case PieceStatus.conected:
                ChangeRigid(false); // NOVA LÓGICA: Peça conectada usa a física normal para mexer o conjunto todo!
                break;
            case PieceStatus.root:
                ChangeRigid(false); // Raiz livre para ser movida
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

    private void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag("floor"))
        {
            this.transform.position = _backPos.position;
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