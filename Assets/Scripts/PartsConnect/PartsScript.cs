using NUnit.Framework;
using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartsScript : MonoBehaviour
{
    [SerializeField] private Transform _backPos;
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _connectSon;
    [SerializeField] private Grabbable _grabble;
    [SerializeField] private PartsManager _partsManager;
    private List<ConnectScript> _connectsScriptList = new List<ConnectScript>();
    public float _timeAnimate = 0.25f;
    private bool _isConnecting;




    public bool test = false;


    private void Start()
    {
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
        _target = target;
        if (con == null) { _connectSon = null; }
        else
            _connectSon = _connectsScriptList[_connectsScriptList.IndexOf(con)].transform;
    }

    public void ConnectAnimation()
    {
        if (_isConnecting) return;
        if (_target == null || _connectSon == null) return;

        ConnectPosition cp =
            _partsManager.CalculatingPosition(transform, _target, _connectSon);

        if (cp == null) return;

        _isConnecting = true;
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
        _isConnecting = false;
    }

    private void OnTriggerStay(Collider col)
    {
        Debug.Log("Stay "+ "- "+col.gameObject.tag);
        if (col.gameObject.tag.Equals("floor"))
        {
            this.transform.position = _backPos.position;
        }
    }
}

public enum ConnectType
{
    none,
    male,
    famale
}
