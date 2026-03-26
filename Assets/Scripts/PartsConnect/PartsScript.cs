using Oculus.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Experimental.GraphView.GraphView;

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

    [SerializeField] private bool _isPanel = false;
    private PartsManager _partsManager;
    private float _timeAnimate = 0.25f;
    private float _mass;
    private Rigidbody _rigid;
    private PartConnectLogic _connectLogic;
    private PieceStatus _lastStatus;

    public bool _isGrabbed = false;
    public UnityEvent<bool> onChangeGrabbleStatus;
    public bool test = false;
    public static event Action<PartsScript> OnPartGrabbed;

    public List<SnapIndicator> ConnectsScriptList => _connectsScriptList;

    // --- SISTEMA DE TRAVA GLOBAL DE FÍSICA ---
    public static int GlobalConnectingCount = 0;
    private static HashSet<PartsScript> _allParts = new HashSet<PartsScript>();
    private void OnValidate()
    {
        if (_status == _lastStatus) return;
        SetStatus(_status);
    }

    private void Awake()
    {
        _allParts.Add(this);

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
        _partsManager = FindAnyObjectByType<PartsManager>();
        _grabble = GetComponent<Grabbable>();

        if (_grabble != null)
        {
            _grabble.WhenPointerEventRaised += OnGrabbleEvent;
        }
    }

    private void FixedUpdate()
    {
        if (!_isPanel && _rigid.isKinematic) { ChangeAllRigid(false); }
    }

    private void OnDestroy()
    {
        _allParts.Remove(this);

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

        UpdateBreakForceLogic();
    }

    private void UpdateBreakForceLogic()
    {
        if (_status == PieceStatus.conecting || _partScriptTarget == null) return;

        bool isTargetGrabbed = _partScriptTarget._isGrabbed;
        bool isTargetRoot = _partScriptTarget.GetStatus() == PieceStatus.root;
        bool amIRoot = GetStatus() == PieceStatus.root;

        bool canBreak = (_isGrabbed && isTargetGrabbed) ||
                        (_isGrabbed && isTargetRoot) ||
                        (isTargetGrabbed && amIRoot);

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

        // Tempo 0f forçará o PartConnectLogic a pular a animação e criar os joints imediatamente.
        // Dica: Se o PartConnectLogic der erro de "divisão por zero", altere o 0f para 0.001f
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
        if (_snapTarget != null) _snapTarget.SetIsConnect(false);
        if (_myActiveSnap != null) _myActiveSnap.SetIsConnect(false);

        _status = PieceStatus.none;
        ClearConnectionData();
        SetStatus(PieceStatus.none);
    }

    public void SetStatus(PieceStatus st)
    {
        if (Application.isPlaying)
        {
            if (_lastStatus != PieceStatus.conecting && st == PieceStatus.conecting)
            {
                GlobalConnectingCount++;
                if (GlobalConnectingCount == 1) RefreshAllPhysics();
            }
            else if (_lastStatus == PieceStatus.conecting && st != PieceStatus.conecting)
            {
                GlobalConnectingCount--;
                if (GlobalConnectingCount <= 0)
                {
                    GlobalConnectingCount = 0;
                    RefreshAllPhysics();
                }
            }
        }

        _status = st;
        _lastStatus = st;

        ApplyPhysicsBasedOnStatus();
    }

    public static void RefreshAllPhysics()
    {
        foreach (var part in _allParts)
        {
            if (part != null)
            {
                part.ApplyPhysicsBasedOnStatus();
            }
        }
    }

    public void ApplyPhysicsBasedOnStatus()
    {
        if (GlobalConnectingCount > 0)
        {
            ChangeRigid(true);
            return;
        }

        //switch (_status)
        //{
        //    case PieceStatus.none:
        //    case PieceStatus.conected:
        //    case PieceStatus.root:
        //        ChangeRigid(false);
        //        break;
        //    case PieceStatus.conecting:
        //        break;
        //}
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

    public void ChangeAllRigid(bool v)
    {
        PartsScript[] parts = GetComponentsInChildren<PartsScript>();
        int count = 0;
        foreach (PartsScript child in parts)
        {
            if (child != null)
            {
                count++;
                child.ChangeRigid(v);
                Debug.Log(child.name);
            }
        }
        Debug.Log(count);
        Debug.Log(v);
    }

    public PieceStatus GetStatus() { return _status; }
    public Rigidbody GetRigid() { return _rigid; }
    public float GetMass() { return _mass; }

    public void SetPainel(bool isPanel)
    {
        _isPanel = isPanel;
        ChangeAllRigid(isPanel);
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
    root
}