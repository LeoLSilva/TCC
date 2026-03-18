using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ObjectIdentify
{
    public GameObject _thisObject {  get; private set; }
    public GameObject _target {  get; private set; }
    public SnapInteractor _thisSnap {  get; private set; }
    public SnapInteractor _targetSnap { get; private set; }
    public string _snapName { get; private set; }

    public void SetObjectIdentify(GameObject thisObj, GameObject target, SnapInteractor thisSnap, SnapInteractor snapTarget, string snapName)
    {
        _thisObject = thisObj;
        _target = target;
        _thisSnap = thisSnap;
        _targetSnap = snapTarget;
        _snapName = snapName;
    }
}
