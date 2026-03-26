using UnityEngine;

public class RecentrazileCam : MonoBehaviour
{
    [Header("References")]
    public Transform playerRoot;
    public Transform playerHead;
    public Transform recenterTarget;

    void OnEnable()
    {
        if (OVRManager.display != null)
        {
            // Começa a escutar o evento nativo de recentralização
            OVRManager.display.RecenteredPose += AlignWithTarget;
        }
    }

    void OnDisable()
    {
        if (OVRManager.display != null)
        {
            // Para de escutar o evento para evitar erros de memória
            OVRManager.display.RecenteredPose -= AlignWithTarget;
        }
    }

    private void AlignWithTarget()
    {
        // 1. Alinha a rotação no eixo Y
        float angleDifference = recenterTarget.eulerAngles.y - playerHead.eulerAngles.y;
        playerRoot.Rotate(0, angleDifference, 0);

        // 2. Alinha a posição
        Vector3 positionDifference = recenterTarget.position - playerHead.position;

        // Descomente a linha abaixo se o seu jogo for jogado em pé (Floor Level)
        // positionDifference.y = 0; 

        playerRoot.position += positionDifference;

        Debug.Log("Native system recentered. Custom adjustment applied!");
    }
}
