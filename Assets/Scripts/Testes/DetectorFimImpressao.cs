using UnityEngine;

public class DetectorFimImpressao : MonoBehaviour
{
    [Tooltip("Arraste o objeto Pai (que tem o script principal da impressora) para cá.")]
    public PrinterManager controladorPrincipal;

    [Tooltip("A Tag da peça que está sendo impressa.")]
    public string tagDaPeca = "PecaImpressa";

    private bool jaRetornando = false;

    private void OnTriggerExit(Collider other)
    {
        // Verifica se quem saiu foi a peça e se já não mandamos o aviso
        if (other.CompareTag(tagDaPeca) && !jaRetornando)
        {
            Debug.Log($"[Detector Cubo] A peça {other.name} saiu! Avisando a impressora para subir.");
            jaRetornando = true;

            // Chama a função lá no script do pai
            if (controladorPrincipal != null)
            {
                controladorPrincipal.FinalizarImpressao();
            }
            else
            {
                Debug.LogError("[Detector Cubo] O Controlador Principal não foi referenciado no Inspector!");
            }
        }
    }

    // Função opcional caso você precise resetar o detector para uma nova impressão
    public void ResetarDetector()
    {
        jaRetornando = false;
    }
}