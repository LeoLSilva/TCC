using System.Collections;
using UnityEngine;

public class PrinterManager : MonoBehaviour
{
    [SerializeField] private GameObject _currentObject;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _imprimindo;
    public Animator _animator;
    private string _animationName = "printing";


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
    public bool jaRetornando = false;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetCurrentObject(GameObject obj)
    {
        _currentObject = obj;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            StartCoroutine(SpawnPointAnim());
        }
    }

    IEnumerator SpawnPointAnim()
    {
        _animator.SetBool("Start", true);
        yield return new WaitForSeconds(2f);
        PartsScript g = Instantiate(_currentObject, _spawnPoint.position, Quaternion.identity).GetComponent<PartsScript>();
        _animator.SetBool("Start", false);
        g.gameObject.layer = 6;
        g.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        yield return new WaitForSeconds(.5f);
        g.SetStatus(PieceStatus.printing);
    }

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
        float zAtual = cabecoteImpressora.localPosition.z;
        float porcentagemAtual = Mathf.InverseLerp(zMinimo, zMaximo, zAtual);

        // LOG 1: Vamos ver exatamente onde a impressora parou e qual foi a conta
        Debug.Log($"[Impressora] Iniciando FinalizarImpressao. Z Atual: {zAtual} | Z Maximo: {zMaximo} | Porcentagem: {porcentagemAtual}");

        _animator.SetFloat("AlturaImpressao", porcentagemAtual);
        _animator.SetTrigger("Retornar");

        StartCoroutine(SubirParaTopo(porcentagemAtual));
    }

    private IEnumerator SubirParaTopo(float valorInicial)
    {
        float valorAtual = valorInicial;

        // LOG 2: Alerta se a matemática já bater 100% no começo
        if (valorAtual >= 1f)
        {
            Debug.LogWarning("[Impressora] ATENÇÃO: A porcentagem já começou em 1 (ou mais)! O loop 'while' não vai rodar. A impressora já passou do zMaximo?");
        }

        while (valorAtual < 1f)
        {
            valorAtual += velocidadeRetorno * Time.deltaTime;
            valorAtual = Mathf.Clamp01(valorAtual);

            _animator.SetFloat("AlturaImpressao", valorAtual);

            // LOG 3: Mostra a soma acontecendo frame a frame
            Debug.Log($"[Impressora] Somando animação... Valor atual: {valorAtual}");

            yield return null;
        }

        Debug.Log("[Impressora] Retornou ao topo com sucesso!");
        jaRetornando = false;
    }
}
