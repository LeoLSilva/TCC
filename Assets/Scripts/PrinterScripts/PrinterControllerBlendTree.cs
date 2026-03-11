using UnityEngine;
using System.Collections;

public class PrinterControllerBlendTree : MonoBehaviour
{
    public Animator animator;

    [Header("Configurações de Altura (Eixo Z)")]
    [Tooltip("A posição Z exata quando a impressora está na BASE.")]
    public float zMinimo = -0.01763416f;

    [Tooltip("A posição Z exata quando a impressora está no TOPO.")]
    public float zMaximo = 0.01875172f;

    [Tooltip("Velocidade que a impressora volta para o topo (em porcentagem por segundo).")]
    public float velocidadeRetorno = 0.5f;

    [Header("Referência")]
    [Tooltip("Arraste o objeto que se move (o cabeçote da impressora) aqui.")]
    public Transform cabecoteImpressora;
    public string tagDaPeca = "obj";
    private bool jaRetornando = false;


    // Chame essa função quando a sua peça terminar de imprimir!
    private void OnTriggerExit(Collider other)
    {
        // Verifica se o objeto tem a Tag certa e se já não estamos retornando
        if (other.CompareTag(tagDaPeca) && !jaRetornando)
        {
            Debug.Log($"[Impressora] Saiu do contato com a peça ({other.name})! Iniciando retorno...");
            jaRetornando = true;
            FinalizarImpressao();
        }
    }

    public void FinalizarImpressao()
    {
        float porcentagemAtual = Mathf.InverseLerp(zMinimo, zMaximo, cabecoteImpressora.localPosition.z);

        animator.SetFloat("AlturaImpressao", porcentagemAtual);
        animator.SetTrigger("Retornar");

        StartCoroutine(SubirParaTopo(porcentagemAtual));
    }

    private IEnumerator SubirParaTopo(float valorInicial)
    {
        float valorAtual = valorInicial;

        while (valorAtual < 1f)
        {
            valorAtual += velocidadeRetorno * Time.deltaTime;
            valorAtual = Mathf.Clamp01(valorAtual);

            animator.SetFloat("AlturaImpressao", valorAtual);

            yield return null;
        }

        Debug.Log("[Impressora] Retornou ao topo com sucesso via Blend Tree!");
    }
}