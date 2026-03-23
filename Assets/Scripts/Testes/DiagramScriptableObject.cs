using UnityEngine;
using static DiagramSerializable;

[CreateAssetMenu(fileName = "DiagramScriptableObject", menuName = "Scriptable Objects/DiagramScriptableObject")]
public class DiagramScriptableObject : ScriptableObject
{
    [Tooltip("A peça principal da montagem (ex: Corpo ou Cabeça)")]
    public DiagramNode rootPart;
}
