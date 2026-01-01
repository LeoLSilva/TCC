using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartsScript : MonoBehaviour
{
    private List<ConnectScript> _connectsScriptList = new List<ConnectScript>();
    private float _speedAnim = 1;
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _connectSon;

    private void Start()
    {
        AddConectionsOnList();
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

    private void Update()
    {
        if (_target != null && Input.GetKeyDown(KeyCode.S)) { ConnectAnimation(); }
    }

    public void SetTarget(Transform target, ConnectScript con)
    {
        _target = target;
        _connectSon = _connectsScriptList[_connectsScriptList.IndexOf(con)].transform;
    }

    public void ConnectAnimation()
    {
        StartCoroutine(AnimationMoveCoroutine());
    }
    private IEnumerator AnimationMoveCoroutine()
    {
        while (
            Vector3.Distance(_connectSon.position, _target.position) > 0.001f ||
            Quaternion.Angle(_connectSon.rotation, _target.rotation) > 0.5f
        )
        {
            Vector3 posDelta = _target.position - _connectSon.position;
            transform.position += posDelta.normalized * _speedAnim * Time.deltaTime;

            Quaternion rotDelta =
                _target.rotation * Quaternion.Inverse(_connectSon.rotation);

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    rotDelta * transform.rotation,
                    _speedAnim * 100f * Time.deltaTime
                );

            yield return null;
        }

        Quaternion finalRot =
            _target.rotation * Quaternion.Inverse(_connectSon.rotation);

        transform.rotation = finalRot * transform.rotation;
        transform.position += _target.position - _connectSon.position;
    }

}

public enum ConnectType
{
    none,
    male,
    famale
}
