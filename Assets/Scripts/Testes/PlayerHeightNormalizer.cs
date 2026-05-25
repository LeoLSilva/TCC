using UnityEngine;
using Oculus.Interaction; // Meta SDK 71+

public class PlayerHeightNormalizer : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Altura em metros que os olhos do jogador devem ficar acima do piso")]
    public float targetEyeHeight;

    [Header("Referências")]
    public OVRCameraRig cameraRig;

    private void Start()
    {
        
        StartCoroutine(NormalizeHeightNextFrame());
    }

    void Update()
    {
        NormalizeHeight();
    }

    private System.Collections.IEnumerator NormalizeHeightNextFrame()
    {
        yield return new WaitUntil(() => OVRManager.isHmdPresent);
        yield return null;
        NormalizeHeight();
    }

    public void NormalizeHeight()
    {
        float currentEyeHeight = cameraRig.centerEyeAnchor.position.y;
        float offset = targetEyeHeight - currentEyeHeight;
        Vector3 rigPosition = cameraRig.transform.position;
        rigPosition.y += offset;
        cameraRig.transform.position = rigPosition;
    }
}