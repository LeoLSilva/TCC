using System.Collections;
using UnityEngine;

public class PartScript : MonoBehaviour
{
    [SerializeField] private Parts _part;
    [SerializeField] private GameObject _spawn;
    [SerializeField] private float _timeInFloor = 5f;
    private float _timer;

    
    
    
    private void ResetPosition()
    {
        this.transform.position = _spawn.transform.position;
    }



    void OnCollisionStay(Collision col)
    {
        if (col.gameObject.name == "Chao")
        {
            _timer += Time.deltaTime;

            if (_timer >= _timeInFloor)
            {
                _timer = 0;
            }
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.name == "Chao")
            _timer = 0;
    }


    public Parts GetPart()
    {
        return _part;
    }
}
