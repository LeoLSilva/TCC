using UnityEngine;
using static DiagramSerializable;

[CreateAssetMenu(fileName = "DiagramScriptableObject", menuName = "Scriptable Objects/DiagramScriptableObject")]
public class DiagramScriptableObject : ScriptableObject
{
    public string signature;
    public DiagramNode rootPart;
    public GameObject _Diagram;
    public Sprite diagramImage;
}
