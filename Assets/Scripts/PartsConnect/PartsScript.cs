using NUnit.Framework;
using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartsScript : MonoBehaviour
{
    [SerializeField] private PieceStatus _status;
    [SerializeField] private Transform _backPos;
    [SerializeField] private PartsScript _connectedPieceScript;
    [SerializeField] private Transform _target;
    [SerializeField] private Grabbable _grabble;
    private Transform _connectSon;
    private PartsManager _partsManager;
    private List<ConnectScript> _connectsScriptList = new List<ConnectScript>();
    private float _timeAnimate = 0.25f;
    private Rigidbody _rigid;


    [Header("Somentes testes no inspetor")]
    // teste editor
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
        AddConectionsOnList();
        _grabble.WhenPointerEventRaised += OnGrabbleEvent;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && test && _target.GetComponent<ConnectScript>().GetConnectType() == ConnectType.famale)
        {
            Debug.Log(name); ConnectAnimation();
        }
<<<<<<< Updated upstream
=======

        if (Input.GetKeyDown(KeyCode.S) && test)
        {
            StartConnectionProcess();
        }

        if (Input.GetKeyDown(KeyCode.P) && test && _status == PieceStatus.root)
        {

        }

        if (Input.GetKeyDown(KeyCode.R) && test)
        {
            FindAnyObjectByType<DiagramCreaterManager>().RefreshDiagramList();
        }
        UpdateBreakForceLogic();
    }

    private void UpdateBreakForceLogic()
    {
        if (_status == PieceStatus.conecting || _partScriptTarget == null) return;

        bool canBreak = false;

        if (_isGrabbed)
        {
            PartsScript currentAncestor = _partScriptTarget;
            while (currentAncestor != null)
            {
                if (currentAncestor._isGrabbed)
                {
                    canBreak = true;
                    break;
                }
                currentAncestor = currentAncestor.GetTargetPart();
            }
        }

        _connectLogic.UpdateBreakForce(canBreak);
>>>>>>> Stashed changes
    }

    private void OnGrabbleEvent(PointerEvent obj)
    {
        if (obj.Type == PointerEventType.Select)
        {
            Debug.Log("Selecionado");
        }
        else if (obj.Type == PointerEventType.Unselect)
        {
            if (_target != null && _target.GetComponent<ConnectScript>().GetConnectType() == ConnectType.male)
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
        if (target != null)
        {
            _target = target;
            _connectedPieceScript = _target.parent.GetComponent<PartsScript>();
        }
        if (con == null) { _connectSon = null; }
        else
            _connectSon = _connectsScriptList[_connectsScriptList.IndexOf(con)].transform;
    }

    public void ConnectAnimation()
    {
        if (_status == PieceStatus.conecting || _status == PieceStatus.conected) return;
        if (_target == null || _connectSon == null) return;

        ConnectPosition cp =
            _partsManager.CalculatingPosition(transform, _target, _connectSon);

        if (cp == null) return;
        SetStatus(PieceStatus.conecting);
        _connectedPieceScript.SetStatus(PieceStatus.conecting);
        StartCoroutine(AnimationMoveCoroutine(cp));
    }

    private IEnumerator AnimationMoveCoroutine(ConnectPosition cp)
    {
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
        _partsManager.SetConection(this, _connectedPieceScript);
    }

    private void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag.Equals("floor"))
        {
            this.transform.position = _backPos.position;
        }
    }

    public void SetStatus(PieceStatus st)
    {
<<<<<<< Updated upstream
=======
        if (_status == st) return;

        if (_status == PieceStatus.root && st == PieceStatus.none)
        {
            return;
        }

        bool wasNotConnected = (_status != PieceStatus.conected);
>>>>>>> Stashed changes
        _status = st;
        switch (_status)
        {
<<<<<<< Updated upstream
            case PieceStatus.none:
                ChangeRigid(false);
                break;
            case PieceStatus.conecting:
                ChangeRigid(true);
                break;
            case PieceStatus.conected:
                ChangeRigid(true);
                break;
            case PieceStatus.root:
                ChangeRigid(false);
                break;
=======
            if (wasNotConnected && !_hasRegisteredConnection)
            {
                DiagramRegister targetRegister = _partScriptTarget.GetComponent<DiagramRegister>();

                if (targetRegister != null)
                {
                    targetRegister.AddConnection(_snapTarget.gameObject.name, _diagramRegister);
                }
                _hasRegisteredConnection = true;

                OrganizeHierarchy();
            }

            gameObject.layer = _partScriptTarget.gameObject.layer;
            ChangeRigid(false);
        }
    }

    private void OrganizeHierarchy()
    {
        PartsScript rootPart = this;
        while (rootPart.GetTargetPart() != null)
        {
            rootPart = rootPart.GetTargetPart();
        }

        Transform targetContainer = rootPart.transform.parent;
        if (targetContainer == null || targetContainer.GetComponent<PartsScript>() != null || !targetContainer.name.Contains("DiagramContainer"))
        {
            GameObject newContainer = new GameObject($"DiagramContainer_{rootPart.gameObject.name}");
            newContainer.transform.position = rootPart.transform.position;
            newContainer.transform.rotation = rootPart.transform.rotation;
            targetContainer = newContainer.transform;
            rootPart.transform.SetParent(targetContainer, true);
        }

        Transform myContainer = transform.parent;

        if (myContainer != targetContainer)
        {
            if (myContainer != null && myContainer.name.Contains("DiagramContainer"))
            {
                int childCount = myContainer.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    myContainer.GetChild(i).SetParent(targetContainer, true);
                }

                if (myContainer.childCount == 0)
                {
                    Destroy(myContainer.gameObject);
                }
            }
            else
            {
                transform.SetParent(targetContainer, true);
            }
>>>>>>> Stashed changes
        }
    }

    private void ChangeRigid(bool kine)
    {
        _rigid.isKinematic = kine;
        _rigid.useGravity = !kine;
    }
<<<<<<< Updated upstream
=======

    public void SetHierarchyLayerAndPhysics(string layerName, bool makeKinematic)
    {
        int newLayer = LayerMask.NameToLayer(layerName);
        PartsScript[] allParts;

        if (transform.parent != null && transform.parent.name.Contains("DiagramContainer"))
        {
            allParts = transform.parent.GetComponentsInChildren<PartsScript>(true);
        }
        else
        {
            allParts = GetComponentsInChildren<PartsScript>(true);
        }

        foreach (var part in allParts)
        {
            part.gameObject.layer = newLayer;
            part.ChangeRigid(makeKinematic);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("floor"))
        {
            if (_rigid != null)
            {
                _rigid.isKinematic = true;
                transform.position = _backPos.position;
                transform.rotation = _backPos.rotation;
                _rigid.linearVelocity = Vector3.zero;
                _rigid.angularVelocity = Vector3.zero;
                _rigid.isKinematic = false;
            }
            else
            {
                transform.position = _backPos.position;
            }
        }
    }

    public void SaveDiagram()
    {
        if (_status == PieceStatus.root)
            DiagramJsonSaver.SaveDiagram(_diagramRegister, this.gameObject.name);
    }
    public PartsScript FindRootPart()
    {
        if (_status == PieceStatus.root)
        {
            return this;
        }

        PartsScript currentPart = this;

        while (currentPart.GetTargetPart() != null)
        {
            currentPart = currentPart.GetTargetPart();

            if (currentPart.GetStatus() == PieceStatus.root)
            {
                return currentPart;
            }
        }

        return currentPart;
    }
    public PieceStatus GetStatus() { return _status; }
    public Rigidbody GetRigid() { return _rigid; }
    public float GetMass() { return _mass; }
    public PartsScript GetTargetPart() { return _partScriptTarget; }
>>>>>>> Stashed changes
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
