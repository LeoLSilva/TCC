using Oculus.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.XR.OpenXR.Features.Interactions.HandInteractionProfile;

[RequireComponent(typeof(PartConnectLogic))]
public class PartsScript : MonoBehaviour
{
    private List<SnapIndicator> _connectsScriptList = new List<SnapIndicator>();
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

    [Header("TESTES")]
    public bool _isGrabbed = false;
    public UnityEvent<bool> onChangeGrabbleStatus;
    public bool test = false;
    public static event Action<PartsScript> OnPartGrabbed;

    private void OnValidate()
    {
        if (_status == _lastStatus) return;
        _lastStatus = _status;
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
    }

    private void Start()
    {
        _partsManager = FindAnyObjectByType<PartsManager>();
        _grabble = GetComponent<Grabbable>();

        if (_rigid != null)
        {
            _mass = _rigid.mass;
            _connectLogic.InitializePhysics(_rigid);
        }

        AddConnectionsToList();

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
            ConnectAnimation();
        }

        if (_partScriptTarget != null)
        {
            bool isTargetGrabbed = _partScriptTarget._isGrabbed;
            bool isTargetRoot = _partScriptTarget.GetStatus() == PieceStatus.root;
            bool amIRoot = GetStatus() == PieceStatus.root;

            bool canBreak = (_isGrabbed && isTargetGrabbed) ||
                            (_isGrabbed && isTargetRoot) ||
                            (isTargetGrabbed && amIRoot);

            _connectLogic.UpdateBreakForce(canBreak);
        }
    }

    private void OnGrabbleEvent(PointerEvent obj)
    {
        if (obj.Type == PointerEventType.Select)
        {
            _isGrabbed = true;
            OnPartGrabbed?.Invoke(this);
        }
        else if (obj.Type == PointerEventType.Unselect)
        {
            _isGrabbed = false;
            OnPartGrabbed?.Invoke(this);
            if (_snapTarget != null && _myActiveSnap != null)
            {
                if (!_snapTarget.GetIsConnect() && !_myActiveSnap.GetIsConnect())
                {
                    if (_snapTarget.GetConnectType() == ConnectType.famale)
                    {
                        _snapTarget.ChangeMesh(null, false);
                    }

                    _snapTarget.SetIsConnect(true);
                    _myActiveSnap.SetIsConnect(true);

                    ConnectAnimation();
                }
            }
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
        Debug.Log(pos + " : " + _connectsScriptList.Count + " : " + gameObject);
        return _connectsScriptList[pos];
    }

    public void AutoConnect(SnapIndicator targetSnap, SnapIndicator mySnap)
    {
        SetTarget(targetSnap, mySnap);
        ConnectAnimation();
    }

    public void SetTarget(SnapIndicator targetSnap, SnapIndicator mySnap)
    {
        _snapTarget = targetSnap;
        _myActiveSnap = mySnap;
        if (targetSnap != null)
        {
            _partScriptTarget = targetSnap.GetPartScript();
        }
    }

    public void ClearTarget()
    {
        _snapTarget = null;
        _myActiveSnap = null;
        _partScriptTarget = null;
    }

    public void ConnectAnimation()
    {
        if (_snapTarget == null) return;
        _connectLogic.StartAnimation(_snapTarget.transform, _partScriptTarget, _timeAnimate);
    }

    public void HandleConnectionBroken()
    {
        if (_snapTarget != null) _snapTarget.SetIsConnect(false);
        if (_myActiveSnap != null) _myActiveSnap.SetIsConnect(false);

        ClearTarget();
        SetStatus(PieceStatus.none);
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
            case PieceStatus.printing:
                ChangeRigid(true);
                break;
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
    root,
    printing
}