using Oculus.Interaction;
using UnityEngine;

public class TempObjPrinter : MonoBehaviour
{
    private Grabbable _grabbable;
    private PartsScript _partScripts;
    private void Awake()
    {
        _grabbable = GetComponent<Grabbable>();
        _partScripts = GetComponent<PartsScript>();
        _partScripts.SetHierarchyLayerAndPhysics("Default",true);
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised += HandlePointerEvent;
        }
    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        _partScripts.SetHierarchyLayerAndPhysics("Default", false);
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised -= HandlePointerEvent;
        }
    }
}
