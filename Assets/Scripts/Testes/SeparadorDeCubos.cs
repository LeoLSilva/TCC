using UnityEngine;
using Oculus.Interaction;

[RequireComponent(typeof(Grabbable))]
public class SeparadorDeCubos : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Distância (em metros) que as mãos precisam se afastar para quebrar a ligação.")]
    public float forceDistance = 0.3f;

    [Header("Debug (Veja no Inspector se funciona)")]
    [Tooltip("Fica TRUE quando você segura ESTE cubo.")]
    public bool estouSendoSegurado = false;

    [Tooltip("Fica TRUE quando você segura o cubo PAI.")]
    public bool paiSendoSegurado = false;

    // Componentes deste cubo
    private Grabbable meuGrabbable;
    private GrabFreePhysicsTransformer meuTransformer;

    // Componente do cubo pai (se houver)
    private Grabbable parentGrabbable;

    private bool ambosSeguradosAnteriormente = false;
    private float distanciaInicialMaos = 0f;

    void Start()
    {
        // Pega automaticamente os componentes deste cubo
        meuGrabbable = GetComponent<Grabbable>();
        meuTransformer = GetComponent<GrabFreePhysicsTransformer>();

        VerificarStatusDeFilho();
    }

    void Update()
    {
        // Se este cubo NÃO tem um pai na hierarquia, ele é um objeto solto. 
        // Não precisamos fazer a lógica de separação.
        if (transform.parent == null)
        {
            ambosSeguradosAnteriormente = false;
            estouSendoSegurado = false;
            paiSendoSegurado = false;
            return;
        }

        // Se tem pai, mas ainda não achamos o Grabbable dele, tentamos achar agora
        if (parentGrabbable == null)
        {
            VerificarStatusDeFilho();
            if (parentGrabbable == null) return; // Se o pai não for segurável, ignora
        }

        // ATUALIZA AS VARIÁVEIS DE DEBUG (Você pode ver isso mudando em tempo real na Unity)
        estouSendoSegurado = meuGrabbable.SelectingPoints.Count > 0;
        paiSendoSegurado = parentGrabbable.SelectingPoints.Count > 0;

        // Verifica se as duas mãos estão segurando (uma neste cubo, outra no pai)
        if (estouSendoSegurado && paiSendoSegurado)
        {
            // Pega a posição das mãos
            Vector3 minhaMao = meuGrabbable.SelectingPoints[0].position;
            Vector3 maoPai = parentGrabbable.SelectingPoints[0].position;

            float distanciaAtual = Vector3.Distance(minhaMao, maoPai);

            if (!ambosSeguradosAnteriormente)
            {
                // Guarda a distância inicial no momento em que agarra os dois
                distanciaInicialMaos = distanciaAtual;
                ambosSeguradosAnteriormente = true;
            }
            else
            {
                // Verifica a força do puxão
                float forcaPuxao = distanciaAtual - distanciaInicialMaos;

                if (forcaPuxao >= forceDistance)
                {
                    SepararDoPai();
                }
            }
        }
        else
        {
            // Se soltar uma das mãos, zera o contador do puxão
            ambosSeguradosAnteriormente = false;
        }
    }

    private void VerificarStatusDeFilho()
    {
        if (transform.parent != null)
        {
            // Procura o Grabbable no pai ou em algum objeto acima na hierarquia
            parentGrabbable = transform.parent.GetComponentInParent<Grabbable>();

            // Como sou filho, desativo meu próprio transformer para não brigar com a física do pai
            if (meuTransformer != null)
            {
                meuTransformer.enabled = false;
            }
        }
    }

    private void SepararDoPai()
    {
        // 1. Tira do pai (transforma em objeto solto)
        transform.SetParent(null);
        parentGrabbable = null; // Esquece o pai
        ambosSeguradosAnteriormente = false;

        // 2. Garante que tem um Rigidbody para cair e ter física
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.useGravity = true;

        // 3. Ativa o transformer para poder ser movido sozinho
        if (meuTransformer != null)
        {
            meuTransformer.enabled = true;
        }
        else
        {
            // Se não tinha transformer, cria um e injeta no grabbable
            meuTransformer = gameObject.AddComponent<GrabFreePhysicsTransformer>();
            meuGrabbable.InjectOptionalOneGrabTransformer(meuTransformer);
            meuGrabbable.InjectOptionalTwoGrabTransformer(meuTransformer);
        }

        Debug.Log(gameObject.name + " foi separado e agora é um objeto solto!");
    }
}