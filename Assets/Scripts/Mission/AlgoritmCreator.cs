using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlgoritmCreator : MonoBehaviour
{
    [SerializeField] private List<AlgoritmDic> TypesAlg = new List<AlgoritmDic>();
    public ScrollRect minhaScroll;
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            minhaScroll.verticalNormalizedPosition += 0.01f;
        }


    }

}




    [Serializable]
    public struct AlgoritmDic
    {
        public Sprite sprite1;
        public Sprite sprite2;
        public algType algType;
    }

    public enum algType
    {
        Part,
        Diagram,
        Con
    }