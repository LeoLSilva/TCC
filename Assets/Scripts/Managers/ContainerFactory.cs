using UnityEngine;

public static class ContainerFactory
{
    public static Transform CreateContainer(string baseName, Vector3 pos, Quaternion rot)
    {
        GameObject container = new GameObject($"DiagramContainer_{baseName}");
        container.transform.position = pos;
        container.transform.rotation = rot;
        container.tag = "Container";

        container.AddComponent<DiagramContainerLog>();

        return container.transform;
    }
}