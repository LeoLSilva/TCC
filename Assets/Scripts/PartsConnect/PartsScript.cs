using Oculus.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static DiagramSerializable;

[RequireComponent(typeof(PartConnectLogic))]
public class PartsScript : MonoBehaviour
{
    [SerializeField] private List<SnapIndicator> _connectsScriptList = new List<SnapIndicator>();
    [SerializeField] private string _name;
    [SerializeField] private PieceStatus _status;
    [SerializeField] private Transform _backPos;
    [SerializeField] private SnapIndicator _snapTarget;
    [SerializeField] private SnapIndicator _myActiveSnap;
    [SerializeField] private PartsScript _partScriptTarget;
    [SerializeField] private Grabbable _grabble;

    private PartsManager _partsManager;
    private float _timeAnimate = 0.25f;
    private float _mass;
    private Rigidbody _rigid;
    private PartConnectLogic _connectLogic;
    private PieceStatus _lastStatus;
    private DiagramRegister _diagramRegister;
    private bool _hasRegisteredConnection = false;

    public bool _isGrabbed = false;
    public UnityEvent<bool> onChangeGrabbleStatus;
    public bool test = false;
    public static event Action<PartsScript> OnPartGrabbed;

    public List<SnapIndicator> ConnectsScriptList => _connectsScriptList;

    private void OnValidate()
    {
        if (_status == _lastStatus) return;
        SetStatus(_status);
    }

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody>();
        _connectLogic = GetComponent<PartConnectLogic>();

        if (_connectLogic == null)
        {
            _connectLogic = gameObject.AddComponent<PartConnectLogic>();
        }

        if (_rigid != null)
        {
            _mass = _rigid.mass;
            _connectLogic.InitializePhysics(_rigid);
        }

        AddConnectionsToList();
    }

    private void Start()
    {
        _diagramRegister = this.GetComponent<DiagramRegister>();
        _partsManager = FindAnyObjectByType<PartsManager>();
        _grabble = GetComponent<Grabbable>();

        if (_grabble != null)
        {
            _grabble.WhenPointerEventRaised += OnGrabbleEvent;
        }
    }

    private void OnDestroy()
    {
        if (_grabble != null)
        {
            _grabble.WhenPointerEventRaised -= OnGrabbleEvent;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) && test)
        {
            _connectLogic.BreakConnection();
        }

        if (Input.GetKeyDown(KeyCode.S) && test)
        {
            StartConnectionProcess();
        }

        if (Input.GetKeyDown(KeyCode.P) && test && _status == PieceStatus.root)
        {
            DiagramJsonSaver.SaveDiagram(_diagramRegister, this.gameObject.name);
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
    }

    private void OnGrabbleEvent(PointerEvent obj)
    {
        if (_status == PieceStatus.conecting) return;

        if (obj.Type == PointerEventType.Select)
        {
            _isGrabbed = true;
            OnPartGrabbed?.Invoke(this);
        }
        else if (obj.Type == PointerEventType.Unselect)
        {
            _isGrabbed = false;
            OnPartGrabbed?.Invoke(this);
            ValidateAndStartConnection();
        }
    }

    private void AddConnectionsToList()
    {
        if (_connectsScriptList.Count > 0) return;

        foreach (Transform child in transform)
        {
            SnapIndicator connect = child.GetComponent<SnapIndicator>();
            if (connect != null)
            {
                _connectsScriptList.Add(connect);
            }
        }
    }

    public SnapIndicator GetSnapFromList(int pos)
    {
        return _connectsScriptList[pos];
    }

    public SnapIndicator GetMaleConnector()
    {
        foreach (SnapIndicator s in _connectsScriptList)
        {
            if (s.GetConnectType() == ConnectType.male)
            {
                return s;
            }
        }
        return null;
    }

    public void SetupConnectionData(SnapIndicator targetSnap, SnapIndicator mySnap)
    {
        if (_status == PieceStatus.conecting) return;

        _snapTarget = targetSnap;
        _myActiveSnap = mySnap;

        if (targetSnap != null)
        {
            _partScriptTarget = targetSnap.GetPartScript();

            if (_partScriptTarget == null)
            {
                _partScriptTarget = targetSnap.GetComponentInParent<PartsScript>();
                if (_partScriptTarget != null)
                {
                    targetSnap.SetPartScript(_partScriptTarget);
                }
            }
        }
    }

    public void ClearConnectionData()
    {
        if (_status == PieceStatus.conecting) return;

        _snapTarget = null;
        _myActiveSnap = null;
        _partScriptTarget = null;
    }

    public void AutoConnect(SnapIndicator targetSnap, SnapIndicator mySnap)
    {
        if (_status == PieceStatus.conecting) return;
        SetupConnectionData(targetSnap, mySnap);
        StartInstantConnectionProcess();
    }

    private void StartInstantConnectionProcess()
    {
        if (_snapTarget == null || _status == PieceStatus.conecting) return;

        SetStatus(PieceStatus.conecting);

        if (_snapTarget.GetConnectType() == ConnectType.famale)
        {
            _snapTarget.ChangeMesh(null, false);
        }

        _snapTarget.SetIsConnect(true);
        _myActiveSnap.SetIsConnect(true);

        _connectLogic.StartAnimation(_snapTarget.transform, _partScriptTarget, 0f);
    }

    private void ValidateAndStartConnection()
    {
        if (_snapTarget == null || _myActiveSnap == null) return;
        if (_snapTarget.GetIsConnect() || _myActiveSnap.GetIsConnect()) return;

        StartConnectionProcess();
    }

    private void StartConnectionProcess()
    {
        if (_snapTarget == null || _status == PieceStatus.conecting) return;

        SetStatus(PieceStatus.conecting);

        if (_snapTarget.GetConnectType() == ConnectType.famale)
        {
            _snapTarget.ChangeMesh(null, false);
        }

        _snapTarget.SetIsConnect(true);
        _myActiveSnap.SetIsConnect(true);

        _connectLogic.StartAnimation(_snapTarget.transform, _partScriptTarget, _timeAnimate);
    }

    public void HandleConnectionBroken()
    {
        if (_hasRegisteredConnection && _snapTarget != null && _partScriptTarget != null)
        {
            DiagramRegister targetRegister = _partScriptTarget.GetComponent<DiagramRegister>();
            DiagramRegister myRegister = GetComponent<DiagramRegister>();

            if (targetRegister != null && myRegister != null)
            {
                targetRegister.RemoveConnection(_snapTarget.gameObject.name, myRegister);
            }
        }

        if (_snapTarget != null) _snapTarget.SetIsConnect(false);
        if (_myActiveSnap != null) _myActiveSnap.SetIsConnect(false);

        ClearConnectionData();

        _hasRegisteredConnection = false;

        SetStatus(PieceStatus.none);
    }

    public void SetStatus(PieceStatus st)
    {
        if (_status == PieceStatus.root && st == PieceStatus.none && transform.childCount > 0)
        {
            return;
        }

        bool wasNotConnected = (_status != PieceStatus.conected);
        _status = st;
        _lastStatus = st;

        if (_status == PieceStatus.conected && transform.parent != null)
        {
            if (wasNotConnected && !_hasRegisteredConnection)
            {
                DiagramRegister targetRegister = _partScriptTarget.GetComponent<DiagramRegister>();

                if (targetRegister != null)
                {
                    targetRegister.AddConnection(_snapTarget.gameObject.name, _diagramRegister);
                }
                _hasRegisteredConnection = true;
            }

            gameObject.layer = transform.parent.gameObject.layer;

            Rigidbody parentRigid = transform.parent.GetComponentInParent<Rigidbody>();
            if (parentRigid != null)
            {
                ChangeRigid(parentRigid.isKinematic);
            }
        }
    }

    public void ChangeRigid(bool isKinematic)
    {
        if (_rigid != null)
        {
            _rigid.isKinematic = isKinematic;
            _rigid.useGravity = !isKinematic;
        }
    }

    public void SetHierarchyLayerAndPhysics(string layerName, bool makeKinematic)
    {
        int newLayer = LayerMask.NameToLayer(layerName);

        PartsScript[] allConnectedParts = GetComponentsInChildren<PartsScript>(true);
        foreach (PartsScript part in allConnectedParts)
        {
            if (part != null)
            {
                part.gameObject.layer = newLayer;
                part.ChangeRigid(makeKinematic);
            }
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

    public PieceStatus GetStatus() { return _status; }
    public Rigidbody GetRigid() { return _rigid; }
    public float GetMass() { return _mass; }
    public PartsScript GetTargetPart() { return _partScriptTarget; }
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