using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Events; // Necessário para usar UnityEvents no Inspector

// Adicione este script ao objeto que o jogador segura no VR
public class AutoReleaseOnForce : MonoBehaviour
{
    [Header("Configurações de Força")]
    [Tooltip("Quanta força física é necessária para forçar o jogador a soltar o item. Ajuste testando no jogo!")]
    public float forceLimit = 50f;

    [Tooltip("Se o objeto usa um Joint, arraste-o aqui. Se ficar vazio, o script procura por impactos de colisão livre.")]
    public Joint jointToCheck;

    [Header("Ações")]
    [Tooltip("O que deve acontecer quando a força passar do limite? (Coloque a função Drop/Release do seu SDK de VR aqui)")]
    public UnityEvent onForceExceeded;

    private bool hasReleased = false;

    public HandGrabInteractor handInteractor;

    void Start()
    {
        // Tenta pegar o Joint automaticamente se você esquecer de arrastar no Inspector
        if (jointToCheck == null)
        {
            jointToCheck = GetComponent<Joint>();
        }
    }

    public void CancelGrab()
    {
        if (handInteractor.HasSelectedInteractable)
        {
            handInteractor.ForceRelease(); // Immediately drops the object
        }
    }

    void FixedUpdate()
    {
        if (hasReleased) return;

        // VERIFICAÇÃO 1: Se o objeto está preso por um Joint (Configurable, Hinge, etc)
        if (jointToCheck != null)
        {
            // Pega a magnitude da força que está puxando o Joint
            if (jointToCheck.currentForce.magnitude > forceLimit)
            {
                TriggerRelease("Força excessiva no Joint!");
            }
        }
    }

    // VERIFICAÇÃO 2: Se o objeto está solto e sofrendo impactos violentos (batendo muito na parede ou outros objetos)
    void OnCollisionStay(Collision collision)
    {
        if (hasReleased) return;

        // O 'impulse' é o "tranco" da batida. Dividimos pelo FixedDeltaTime para ter uma estimativa da força bruta contínua.
        float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;

        if (collisionForce > forceLimit)
        {
            TriggerRelease("Colisão muito violenta detectada!");
        }
    }

    // Função que executa a liberação
    private void TriggerRelease(string reason)
    {
        hasReleased = true;
        Debug.Log($"<color=orange>Item solto automaticamente: {reason}</color>");

        // Dispara o evento que vai chamar a função de soltar do seu script de VR
        onForceExceeded.Invoke();
    }

    // Função pública para você chamar quando o jogador "agarrar" o item de novo (reseta a checagem)
    public void ResetReleaseFlag()
    {
        hasReleased = false;
    }
}