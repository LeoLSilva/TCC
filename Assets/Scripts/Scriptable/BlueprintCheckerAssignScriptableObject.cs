using UnityEngine;

[CreateAssetMenu(fileName = "NewDiagramBlueprint", menuName = "Diagram/Blueprint")]
public class BlueprintCheckerAssignScriptableObject : ScriptableObject
{
    public string blueprintName;
    [TextArea(3, 10)]
    public string targetSignature;
}
