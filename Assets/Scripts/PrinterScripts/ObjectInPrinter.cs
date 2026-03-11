using UnityEngine;

public class ObjectInPrinter : MonoBehaviour
{
    private PartsScript _rigid;
    private void OnEnable()
    {
        _rigid = this.GetComponent<PartsScript>();
        _rigid.ChangeRigid(true);
    }
}
