using Meta.XR.Simulator.Editor;
using NUnit.Framework;
using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartsScript : MonoBehaviour
{
    [SerializeField] private PieceStatus _status;
    [SerializeField] private Transform _backPos;
    [SerializeField] private PartsScript _partScriptTarget;
    [SerializeField] private Transform _conectorTarget;
    [SerializeField] private Grabbable _grabble;
    private Transform _connectorTransform;
    private PartsManager _partsManager;
    private List<ConnectScript> _connectsScriptList = new List<ConnectScript>();
    private float _timeAnimate = 0.25f;

    [Header("Componente Rigid")]
    [SerializeField] private float _mass;
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
        if (Input.GetKeyDown(KeyCode.S) && test && _conectorTarget.GetComponent<ConnectScript>().GetConnectType() == ConnectType.famale)
        {
            Debug.Log(name); ConnectAnimation();
        }
        if (Input.GetKeyDown(KeyCode.D) && test) { SetStatus(PieceStatus.none); }
    }

    private void OnGrabbleEvent(PointerEvent obj)
    {
        if (obj.Type == PointerEventType.Select)
        {
            Debug.Log("Selecionado");
        }
        if (obj.Type == PointerEventType.Unselect)
        {
            Debug.Log("Deselecionado " + (_conectorTarget.GetComponent<ConnectScript>().GetConnectType() == ConnectType.male));
            if (_conectorTarget != null && _conectorTarget.GetComponent<ConnectScript>().GetConnectType() != ConnectType.male)
            {
                ConnectAnimation();
            }
        }
    }

    private void Connect()
    {
        ConnectAnimation();
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

        ConnectPosition cp =
            _partsManager.CalculatingPosition(transform, _conectorTarget, _connectorTransform);

        if (cp == null) return;
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
        _partsManager.SetConection(this, _partScriptTarget);
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
        _status = st;
        switch (_status)
        {
            case PieceStatus.none:
                //RigidController(true);
                ChangeRigid(false);
                break;
            case PieceStatus.conected:
                //RigidController(false);
                ChangeRigid(true);
                break;
            case PieceStatus.root:
                ChangeRigid(false);
                break;
        }
    }

    private void ChangeRigid(bool kine)
    {
        _rigid.isKinematic = kine;
        _rigid.useGravity = !kine;
    }

    private void RigidController(bool create)
    {
        if (!create && _rigid != null) Destroy(_rigid);
        else
        {
            Rigidbody r = this.gameObject.AddComponent<Rigidbody>();            
            r.mass = _mass;
            this.gameObject.GetComponent<Grabbable>();
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
