using UnityEngine;

// ----------------------------------------------------------------------------
// Coloque este script nos locais onde uma peça pode ser encaixada.
// Exemplo: O "ombro" do robô, ou os pinos de uma peça de Lego.
// ----------------------------------------------------------------------------
public class RobotSnapPoint : MonoBehaviour
{
    [Header("Configurações do Ponto de Encaixe")]
    [Tooltip("Define que tipo de peça pode encaixar aqui (ex: 'Braco', 'Cabeca', 'Qualquer')")]
    public string partType = "Qualquer";

    [Tooltip("Indica se já existe uma peça encaixada aqui")]
    public bool isOccupied = false;

    private void Awake()
    {
        // Garante que o objeto tenha um collider como Trigger para detectar as peças
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning($"O SnapPoint {gameObject.name} precisa de um Collider com 'Is Trigger' ativado!");
        }
    }
}