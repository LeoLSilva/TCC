using System;
using UnityEngine;

public class SnapIndicator : MonoBehaviour
{
    [SerializeField] private MeshRenderer _indicator;
    [SerializeField] private ConnectType _type;

    // Propriedades Públicas para comunicação clara entre as peças
    public PartsScript ParentPart { get; private set; }
    public bool IsConnected { get; set; } = false; // Trava a peça se ela já estiver em uso
    public SnapIndicator CurrentHover { get; private set; } // Sabe em quem está encostando agora

    private void Start()
    {
        _indicator = GetComponent<MeshRenderer>();
        if (_indicator != null) _indicator.enabled = false;

        ParentPart = GetComponentInParent<PartsScript>();

        // Lógica do Relay para evitar o Bug do Meta SDK
        if (_type == ConnectType.male)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            ColliderRelay relay = transform.parent.GetComponent<ColliderRelay>();
            if (relay == null) { relay = transform.parent.gameObject.AddComponent<ColliderRelay>(); }
            relay.SetSnap(this);

            // Assinando os dois eventos do Relay
            relay.OnCollision += HandleTriggerEnter;
            relay.OnCollisionExit += HandleTriggerExit;
        }
        else
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
    }

    public void HandleTriggerEnter(Collider other)
    {
        // Se já estou conectado ou bati em algo que não é peça, ignora.
        if (IsConnected || other.CompareTag("obj") == false) return;

        SnapIndicator otherSnap = other.GetComponent<SnapIndicator>();
        if (otherSnap == null)
        {
            ColliderRelay relay = other.GetComponentInParent<ColliderRelay>();
            if (relay != null) otherSnap = relay.GetSnap();
        }

        // Ignora se não achou o snap, se o alvo já está ocupado, ou se os gêneros são iguais
        if (otherSnap == null || otherSnap.IsConnected || otherSnap.GetConnectType() == this._type) return;

        CurrentHover = otherSnap;

        // Se eu sou o MACHO e o jogador ESTÁ me segurando
        if (_type == ConnectType.male && ParentPart != null && ParentPart._isGrabbed)
        {
            ParentPart.SetTarget(otherSnap, this); // Aviso meu script pai quem é o meu alvo
        }
        // Se eu sou a FÊMEA (não importa se estou na mão ou solta)
        else if (_type == ConnectType.famale)
        {
            ChangeMesh(other.gameObject, true); // Mostro o holograma
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleTriggerEnter(other);
    }

    public void HandleTriggerExit(Collider other)
    {
        if (CurrentHover == null) return;

        SnapIndicator otherSnap = other.GetComponent<SnapIndicator>();
        if (otherSnap == null)
        {
            ColliderRelay relay = other.GetComponentInParent<ColliderRelay>();
            if (relay != null) otherSnap = relay.GetSnap();
        }

        // Se quem saiu foi exatamente quem eu estava encostando
        if (otherSnap == CurrentHover)
        {
            if (_type == ConnectType.male && !IsConnected && ParentPart != null)
            {
                ParentPart.ClearTarget(); // Perdi o alvo, limpo o script pai
            }
            else if (_type == ConnectType.famale && !IsConnected)
            {
                ChangeMesh(null, false); // Apago o holograma
            }
            CurrentHover = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        HandleTriggerExit(other);
    }

    // Função pública para que o PartsScript possa forçar o desligamento do holograma ao conectar
    public void ChangeMesh(GameObject obj, bool show)
    {
        if (_indicator == null || IsConnected) return;

        if (show && obj != null)
        {
            Vector3 escalaAlvo = obj.transform.lossyScale;
            if (transform.parent != null)
            {
                Vector3 escalaDoPai = transform.parent.lossyScale;
                transform.localScale = new Vector3(
                    escalaAlvo.x / escalaDoPai.x,
                    escalaAlvo.y / escalaDoPai.y,
                    escalaAlvo.z / escalaDoPai.z
                );
            }
            else
            {
                transform.localScale = escalaAlvo;
            }
            _indicator.enabled = true;
        }
        else
        {
            _indicator.enabled = false;
        }
    }

    public ConnectType GetConnectType() { return _type; }
}