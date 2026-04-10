using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PartsScriptableObject", menuName = "Scriptable Objects/PartsScriptableObject")]
public class PartsScriptableObject : ScriptableObject
{
    public Sprite image2D;
    public DiagramScriptableObject diagramScriptable;
}
