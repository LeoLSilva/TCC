using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(PartsScript))]
public class DistanceGrabController : MonoBehaviour
{
    [SerializeField] private GameObject _nearGrabObject;
    [SerializeField] private GameObject _farGrabObject;
    private string _groundTag = "floor";

    private IInteractableView[] _interactables;
    private IInteractableView[] _farInteractables;
    private bool _isOnGround;
    private bool _isCurrentlyFar;
    private PartsScript _partsScript;
    private bool _wasDistanceGrabbed;

    // Acessador público para o Root consultar o estado local
    public bool IsOnGround => _isOnGround;

    private void Start()
    {
        _interactables = GetComponentsInChildren<IInteractableView>(true);
        _partsScript = GetComponent<PartsScript>();

        if (_farGrabObject != null)
        {
            _farInteractables = _farGrabObject.GetComponentsInChildren<IInteractableView>(true);
        }

        _isOnGround = false;
        _isCurrentlyFar = false;
        _wasDistanceGrabbed = false;

        if (_nearGrabObject != null) _nearGrabObject.SetActive(true);
        if (_farGrabObject != null) _farGrabObject.SetActive(false);
    }

    private void Update()
    {
        if (_nearGrabObject == null || _farGrabObject == null) return;

        bool isGrabbed = IsBeingGrabbedOrPulled();
        bool isDistanceGrabbed = IsBeingDistanceGrabbed();

        if (isDistanceGrabbed && !_wasDistanceGrabbed)
        {
            _wasDistanceGrabbed = true;
            ToggleClusterWeight(true);
        }
        else if (!isDistanceGrabbed && _wasDistanceGrabbed)
        {
            _wasDistanceGrabbed = false;
            ToggleClusterWeight(false);
        }

        if (!isGrabbed)
        {
            bool canBeFar = false;
            if (_partsScript != null)
            {
                PieceStatus status = _partsScript.GetStatus();
                canBeFar = (status == PieceStatus.none || status == PieceStatus.root);
            }

            // Apenas o Root deve decidir ativar o farGrabObject para o grupo todo
            bool isAnyPartOnGround = CheckIfAnyPartOnGround();
            bool shouldBeFar = isAnyPartOnGround && canBeFar;

            if (shouldBeFar != _isCurrentlyFar)
            {
                _isCurrentlyFar = shouldBeFar;
                _farGrabObject.SetActive(_isCurrentlyFar);
                _nearGrabObject.SetActive(!_isCurrentlyFar);
            }
        }
    }

    // Verifica todo o grupo para ver se alguém está tocando o chão
    private bool CheckIfAnyPartOnGround()
    {
        if (_partsScript == null) return _isOnGround;
        
        PartsScript rootPart = _partsScript.FindRootPart();
        if (rootPart == null) return _isOnGround;

        Transform container = (rootPart.transform.parent != null && rootPart.transform.parent.name.Contains("DiagramContainer"))
                              ? rootPart.transform.parent
                              : rootPart.transform;

        DistanceGrabController[] allControllers = container.GetComponentsInChildren<DistanceGrabController>(true);
        
        foreach (var controller in allControllers)
        {
            if (controller.IsOnGround) return true;
        }
        
        return false;
    }

    private void ToggleClusterWeight(bool makeLight)
    {
        if (_partsScript == null) return;

        PartsScript rootPart = _partsScript.FindRootPart();
        Transform container = (rootPart.transform.parent != null && rootPart.transform.parent.name.Contains("DiagramContainer"))
                              ? rootPart.transform.parent
                              : rootPart.transform;

        PartsScript[] parts = container.GetComponentsInChildren<PartsScript>(true);
        foreach (PartsScript p in parts)
        {
            if (p != _partsScript)
            {
                Rigidbody r = p.GetRigid();
                if (r != null && !r.isKinematic)
                {
                    r.mass = makeLight ? 0.001f : p.GetMass();
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_groundTag))
        {
            _isOnGround = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag(_groundTag))
        {
            _isOnGround = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(_groundTag))
        {
            _isOnGround = false;
        }
    }

    private bool IsBeingGrabbedOrPulled()
    {
        if (_interactables == null) return false;

        foreach (var interactable in _interactables)
        {
            if (interactable.State == InteractableState.Select)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsBeingDistanceGrabbed()
    {
        if (_farInteractables == null) return false;

        foreach (var interactable in _farInteractables)
        {
            if (interactable.State == InteractableState.Select)
            {
                return true;
            }
        }
        return false;
    }
}