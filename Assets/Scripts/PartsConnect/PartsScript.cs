using Oculus.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static DiagramSerializable;

[RequireComponent(typeof(PartConnectLogic))]
public class PartsScript : MonoBehaviour
{
    [SerializeField] private List<SnapIndicator> _connectsScriptList = new List<SnapIndicator>();
    private static List<PartsScript> _tempPartsList = new List<PartsScript>();
    [SerializeField] private string _name;
    [SerializeField] private PieceStatus _status;
    [SerializeField] private Transform _backPos;
    [SerializeField] private SnapIndicator _snapTarget;
    [SerializeField] private SnapIndicator _myActiveSnap;
    [SerializeField] private PartsScript _partScriptTarget;
    [SerializeField] private Grabbable _grabble;

    private PartsManager _partsManager;
    private MissionManager _missionManager;
    private AlgoritmCreater _algoritmCreater;
    private float _timeAnimate = 0.25f;
    private float _mass;
    private Rigidbody _rigid;
    private PartConnectLogic _connectLogic;
    private PieceStatus _lastStatus;
    private DiagramRegister _diagramRegister;
    private bool _hasRegisteredConnection = false;
    private int _grabCount = 0;

    public bool _isGrabbed = false;
    public UnityEvent<bool> onChangeGrabbleStatus;
    public bool test = false;
    public static event Action<PartsScript> OnPartGrabbed;
    public static event Action OnPieceDetached;

    public List<SnapIndicator> ConnectsScriptList => _connectsScriptList;

    private void OnValidate()
    {
        if (_status == _lastStatus) return;
        SetStatus(_status);
    }

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody>();
        _connectLogic = GetComponent<PartConnectLogic>();

        if (_connectLogic == null)
        {
            _connectLogic = gameObject.AddComponent<PartConnectLogic>();
        }

        if (_rigid != null)
        {
            _mass = _rigid.mass;
            _connectLogic.InitializePhysics(_rigid);
        }

        AddConnectionsToList();
    }

    private void Start()
    {
        _diagramRegister = this.GetComponent<DiagramRegister>();
        _partsManager = FindAnyObjectByType<PartsManager>();
        _missionManager = FindAnyObjectByType<MissionManager>();
        _algoritmCreater = FindAnyObjectByType<AlgoritmCreater>();
        _grabble = GetComponent<Grabbable>();

        if (_grabble != null)
        {
            _grabble.WhenPointerEventRaised += OnGrabbleEvent;
        }

        if (_status == PieceStatus.conected)
        {
            Invoke(nameof(RebuildConnections), 0.2f);
        }
    }

    public void RebuildConnections()
    {
        if (_hasRegisteredConnection) return;

        PartsScript[] allPartsInCluster = transform.root.GetComponentsInChildren<PartsScript>(true);

        foreach (SnapIndicator mySnap in _connectsScriptList)
        {
            foreach (PartsScript otherPart in allPartsInCluster)
            {
                if (otherPart == this) continue;

                foreach (SnapIndicator otherSnap in otherPart.ConnectsScriptList)
                {
                    if (Vector3.Distance(mySnap.transform.position, otherSnap.transform.position) < 0.05f)
                    {
                        if (this.transform.IsChildOf(otherPart.transform))
                        {
                            _partScriptTarget = otherPart;
                            _snapTarget = otherSnap;
                            _myActiveSnap = mySnap;

                            DiagramRegister targetRegister = _partScriptTarget.GetComponent<DiagramRegister>();
                            if (targetRegister != null)
                            {
                                targetRegister.AddConnection(_snapTarget.gameObject.name, _diagramRegister);
                            }

                            _hasRegisteredConnection = true;
                            _snapTarget.SetIsConnect(true);
                            _myActiveSnap.SetIsConnect(true);
                            return;
                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) && test)
        {
            _connectLogic.BreakConnection();
        }

        if (Input.GetKeyDown(KeyCode.S) && test)
        {
            StartConnectionProcess();
        }

        if (Input.GetKeyDown(KeyCode.P) && test && _status == PieceStatus.root)
        {
            DiagramJsonSaver.SaveDiagram(_diagramRegister, this.gameObject.name, StorageManager.PathDiagramas);
        }

        if (Input.GetKeyDown(KeyCode.R) && test)
        {
            FindAnyObjectByType<DiagramCreaterManager>().RefreshDiagramList();
        }
        UpdateBreakForceLogic();
    }

    private void UpdateBreakForceLogic()
    {
        if (_status == PieceStatus.conecting || _partScriptTarget == null) return;

        bool canBreak = false;

        if (_isGrabbed)
        {
            PartsScript rootPart = FindRootPart();
            Transform container = (rootPart.transform.parent != null && rootPart.transform.parent.name.Contains("DiagramContainer"))
                                  ? rootPart.transform.parent
                                  : rootPart.transform;

            container.GetComponentsInChildren<PartsScript>(true, _tempPartsList);

            foreach (PartsScript p in _tempPartsList)
            {
                if (p != this && p._isGrabbed)
                {
                    bool isDescendant = false;
                    PartsScript ancestor = p.GetTargetPart();

                    while (ancestor != null)
                    {
                        if (ancestor == this)
                        {
                            isDescendant = true;
                            break;
                        }
                        ancestor = ancestor.GetTargetPart();
                    }

                    if (!isDescendant)
                    {
                        canBreak = true;
                        break;
                    }
                }
            }
        }

        _connectLogic.UpdateBreakForce(canBreak);
    }

    private void OnGrabbleEvent(PointerEvent obj)
    {
        if (_missionManager != null && !_missionManager.IsGameplayActive()) return;

        if (_status == PieceStatus.conecting) return;

        if (obj.Type == PointerEventType.Select)
        {
            _grabCount++;
            _isGrabbed = true;

            if (_grabCount == 1)
            {
                OnPartGrabbed?.Invoke(this);
                ApplyClusterStabilization(true);
            }
        }
        else if (obj.Type == PointerEventType.Unselect)
        {
            _grabCount--;

            if (_grabCount <= 0)
            {
                _grabCount = 0;
                _isGrabbed = false;
                OnPartGrabbed?.Invoke(this);
                ValidateAndStartConnection();
                ApplyClusterStabilization(false);
            }
        }
    }

    private void ApplyClusterStabilization(bool stabilize)
    {
        PartsScript rootPart = FindRootPart();
        Transform container = (rootPart.transform.parent != null && rootPart.transform.parent.name.Contains("DiagramContainer"))
                              ? rootPart.transform.parent
                              : rootPart.transform;

        container.GetComponentsInChildren<PartsScript>(true, _tempPartsList);

        foreach (PartsScript p in _tempPartsList)
        {
            Rigidbody r = p.GetRigid();
            if (r != null && !r.isKinematic)
            {
                r.angularDamping = stabilize ? 20f : (p.GetStatus() == PieceStatus.none ? 0.05f : 3f);
                r.linearDamping = stabilize ? 5f : (p.GetStatus() == PieceStatus.none ? 0f : 1f);
            }
        }
    }

    private void AddConnectionsToList()
    {
        if (_connectsScriptList.Count > 0) return;

        foreach (Transform child in transform)
        {
            SnapIndicator connect = child.GetComponent<SnapIndicator>();
            if (connect != null)
            {
                _connectsScriptList.Add(connect);
            }
        }
    }

    public SnapIndicator GetSnapFromList(int pos)
    {
        return _connectsScriptList[pos];
    }

    public SnapIndicator GetMaleConnector()
    {
        foreach (SnapIndicator s in _connectsScriptList)
        {
            if (s.GetConnectType() == ConnectType.male)
            {
                return s;
            }
        }
        return null;
    }

    public void SetupConnectionData(SnapIndicator targetSnap, SnapIndicator mySnap)
    {
        if (_status == PieceStatus.conecting) return;

        _snapTarget = targetSnap;
        _myActiveSnap = mySnap;

        if (targetSnap != null)
        {
            _partScriptTarget = targetSnap.GetPartScript();

            if (_partScriptTarget == null)
            {
                _partScriptTarget = targetSnap.GetComponentInParent<PartsScript>();
                if (_partScriptTarget != null)
                {
                    targetSnap.SetPartScript(_partScriptTarget);
                }
            }
        }
    }

    public void ClearConnectionData()
    {
        if (_status == PieceStatus.conecting) return;

        _snapTarget = null;
        _myActiveSnap = null;
        _partScriptTarget = null;
    }

    public void AutoConnect(SnapIndicator targetSnap, SnapIndicator mySnap)
    {
        if (_status == PieceStatus.conecting) return;
        SetupConnectionData(targetSnap, mySnap);
        StartInstantConnectionProcess();
    }

    private void StartInstantConnectionProcess()
    {
        if (_snapTarget == null || _status == PieceStatus.conecting) return;

        SetStatus(PieceStatus.conecting);

        if (_snapTarget.GetConnectType() == ConnectType.famale)
        {
            _snapTarget.ChangeMesh(null, false);
        }

        _snapTarget.SetIsConnect(true);
        _myActiveSnap.SetIsConnect(true);

        _connectLogic.StartAnimation(_snapTarget.transform, _partScriptTarget, 0f);
    }

    private void ValidateAndStartConnection()
    {
        if (_missionManager != null && !_missionManager.IsGameplayActive()) return;

        if (_status == PieceStatus.conected) return;

        if (_missionManager != null && !_missionManager.CanConnectParts())
        {
            ClearConnectionData();
            return;
        }

        if (_snapTarget == null || _myActiveSnap == null) return;
        if (_snapTarget.GetIsConnect() || _myActiveSnap.GetIsConnect()) return;

        StartConnectionProcess();
    }

    private void StartConnectionProcess()
    {
        if (_snapTarget == null || _status == PieceStatus.conecting) return;

        SetStatus(PieceStatus.conecting);

        if (_snapTarget.GetConnectType() == ConnectType.famale)
        {
            _snapTarget.ChangeMesh(null, false);
        }

        _snapTarget.SetIsConnect(true);
        _myActiveSnap.SetIsConnect(true);

        _connectLogic.StartAnimation(_snapTarget.transform, _partScriptTarget, _timeAnimate);
    }

    public void HandleConnectionBroken()
    {
        if (_hasRegisteredConnection && _snapTarget != null && _partScriptTarget != null)
        {
            DiagramRegister targetRegister = _partScriptTarget.GetComponent<DiagramRegister>();
            DiagramRegister myRegister = GetComponent<DiagramRegister>();

            if (targetRegister != null && myRegister != null)
            {
                targetRegister.RemoveConnection(_snapTarget.gameObject.name, myRegister);

                if (_algoritmCreater != null && _missionManager != null && _missionManager.GetMissionState() == MissionState.Mission3)
                {
                    Sprite s1 = targetRegister.GetPartData() != null ? targetRegister.GetPartData().select : null;
                    Sprite s2 = myRegister.GetPartData() != null ? myRegister.GetPartData().select : null;
                    _algoritmCreater.RegisterDisconnection(s1, s2);
                }
            }
        }

        if (_snapTarget != null) _snapTarget.SetIsConnect(false);
        if (_myActiveSnap != null) _myActiveSnap.SetIsConnect(false);

        ClearConnectionData();

        _hasRegisteredConnection = false;

        Transform oldContainer = this.transform.parent;

        List<PartsScript> detachedCluster = new List<PartsScript>();
        detachedCluster.Add(this);

        if (oldContainer != null)
        {
            PartsScript[] allPartsInOld = oldContainer.GetComponentsInChildren<PartsScript>();

            bool addedNew = true;
            while (addedNew)
            {
                addedNew = false;
                foreach (PartsScript p in allPartsInOld)
                {
                    if (!detachedCluster.Contains(p))
                    {
                        if (p.GetTargetPart() != null && detachedCluster.Contains(p.GetTargetPart()))
                        {
                            detachedCluster.Add(p);
                            addedNew = true;
                        }
                    }
                }
            }
        }

        if (detachedCluster.Count > 1)
        {
            Transform newContainer = ContainerFactory.CreateContainer(this.gameObject.name, this.transform.position, this.transform.rotation);

            foreach (PartsScript p in detachedCluster)
            {
                p.transform.SetParent(newContainer, true);
            }

            _status = PieceStatus.root;
            _lastStatus = PieceStatus.root;
        }
        else
        {
            this.transform.SetParent(null);

            _status = PieceStatus.none;
            _lastStatus = PieceStatus.none;
        }

        if (oldContainer != null && oldContainer.name.Contains("DiagramContainer"))
        {
            if (oldContainer.childCount == 1)
            {
                Transform remainingPart = oldContainer.GetChild(0);
                remainingPart.SetParent(null);

                PartsScript pScript = remainingPart.GetComponent<PartsScript>();
                if (pScript != null && pScript.GetStatus() == PieceStatus.root)
                {
                    pScript.SetStatus(PieceStatus.none);
                }

                Destroy(oldContainer.gameObject);
            }
            else if (oldContainer.childCount == 0)
            {
                Destroy(oldContainer.gameObject);
            }
        }

        ChangeRigid(false);
        OnPieceDetached?.Invoke();
    }

    public void SetStatus(PieceStatus st)
    {
        if (_status == PieceStatus.root && st == PieceStatus.none)
        {
            _status = st;
            _lastStatus = st;
            return;
        }

        bool wasNotConnected = (_status != PieceStatus.conected);
        _status = st;
        _lastStatus = st;

        if (_status == PieceStatus.conected && _partScriptTarget != null)
        {
            if (wasNotConnected && !_hasRegisteredConnection)
            {
                DiagramRegister targetRegister = _partScriptTarget.GetComponent<DiagramRegister>();

                if (targetRegister != null)
                {
                    targetRegister.AddConnection(_snapTarget.gameObject.name, _diagramRegister);
                }
                _hasRegisteredConnection = true;

                OrganizeHierarchy();

                if (_algoritmCreater != null && _missionManager != null && _missionManager.GetMissionState() == MissionState.Mission3)
                {
                    Sprite s1 = targetRegister != null && targetRegister.GetPartData() != null ? targetRegister.GetPartData().select : null;
                    Sprite s2 = _diagramRegister != null && _diagramRegister.GetPartData() != null ? _diagramRegister.GetPartData().select : null;

                    PartsScript rootPart = FindRootPart();
                    GameObject container = null;

                    if (rootPart != null)
                    {
                        container = rootPart.transform.parent != null && rootPart.transform.parent.name.Contains("DiagramContainer")
                            ? rootPart.transform.parent.gameObject
                            : rootPart.gameObject;
                    }

                    _algoritmCreater.RegisterConnection(s1, s2, container);
                }
            }

            gameObject.layer = _partScriptTarget.gameObject.layer;

            Rigidbody parentRigid = _partScriptTarget.GetRigid();
            if (parentRigid != null)
            {
                ChangeRigid(parentRigid.isKinematic);
            }
        }
    }

    private void OrganizeHierarchy()
    {
        if (_partScriptTarget == null) return;

        Transform targetContainer = _partScriptTarget.transform.parent;

        if (targetContainer == null || !targetContainer.name.Contains("DiagramContainer"))
        {
            targetContainer = ContainerFactory.CreateContainer(_partScriptTarget.gameObject.name, _partScriptTarget.transform.position, _partScriptTarget.transform.rotation);
            _partScriptTarget.transform.SetParent(targetContainer, true);

            PartsScript localRoot = _partScriptTarget.FindRootPart();
            if (localRoot != null && localRoot.transform.parent != targetContainer)
            {
                localRoot.transform.SetParent(targetContainer, true);
            }
        }

        PartsScript ultimateRoot = _partScriptTarget.FindRootPart();
        if (ultimateRoot != null && ultimateRoot.GetStatus() == PieceStatus.none)
        {
            ultimateRoot.SetStatus(PieceStatus.root);
        }

        Transform myContainer = this.transform.parent;

        if (myContainer != null && myContainer != targetContainer && myContainer.name.Contains("DiagramContainer"))
        {
            int childCount = myContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                myContainer.GetChild(i).SetParent(targetContainer, true);
            }

            DiagramContainerLog targetLog = targetContainer.GetComponent<DiagramContainerLog>();
            DiagramContainerLog myLog = myContainer.GetComponent<DiagramContainerLog>();

            if (targetLog != null && myLog != null && myLog.GetFullLog().Count > 0)
            {
                targetLog.MergeLog(myLog.GetFullLog());
            }

            Destroy(myContainer.gameObject);
        }
        else if (myContainer != targetContainer)
        {
            this.transform.SetParent(targetContainer, true);
        }

        int targetLayer = _partScriptTarget.gameObject.layer;
        PartsScript[] allParts = targetContainer.GetComponentsInChildren<PartsScript>();

        foreach (PartsScript part in allParts)
        {
            part.gameObject.layer = targetLayer;
            part.Invoke(nameof(part.SyncPhysicsDelayed), 0.2f);
        }
    }

    public void SyncPhysicsDelayed()
    {
        PartsScript rootPart = FindRootPart();
        if (rootPart != null && rootPart.GetRigid() != null)
        {
            ChangeRigid(rootPart.GetRigid().isKinematic);
        }
    }

    public PartsScript FindRootPart()
    {
        if (_status == PieceStatus.root)
        {
            return this;
        }

        PartsScript currentPart = this;

        while (currentPart.GetTargetPart() != null)
        {
            currentPart = currentPart.GetTargetPart();

            if (currentPart.GetStatus() == PieceStatus.root)
            {
                return currentPart;
            }
        }

        return currentPart;
    }

    public void SaveDiagram(string folderPath = null)
    {
        if (_status == PieceStatus.root)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = StorageManager.PathDiagramas;
            }

            DiagramJsonSaver.SaveDiagram(_diagramRegister, this.gameObject.name, folderPath);
        }
    }

    public void ChangeRigid(bool isKinematic)
    {
        if (_rigid != null)
        {
            _rigid.isKinematic = isKinematic;
            _rigid.useGravity = !isKinematic;
        }
    }

    public void SetHierarchyLayerAndPhysics(string layerName, bool makeKinematic)
    {
        int newLayer = LayerMask.NameToLayer(layerName);

        ApplyLayerAndPhysicsRecursive(this, newLayer, makeKinematic, new HashSet<PartsScript>());
    }

    private void ApplyLayerAndPhysicsRecursive(PartsScript currentPart, int layer, bool kinematic, HashSet<PartsScript> visited)
    {
        if (currentPart == null || visited.Contains(currentPart)) return;

        visited.Add(currentPart);

        currentPart.gameObject.layer = layer;
        currentPart.ChangeRigid(kinematic);

        PartsScript parentPart = currentPart.GetTargetPart();
        if (parentPart != null)
        {
            ApplyLayerAndPhysicsRecursive(parentPart, layer, kinematic, visited);
        }

        DiagramRegister currentRegister = currentPart.GetComponent<DiagramRegister>();
        if (currentRegister != null)
        {
            DiagramSerializable.DiagramNode node = currentRegister.GetDiagramNode();
            if (node != null && node.connections != null)
            {
                foreach (var connection in node.connections)
                {
                    if (connection.connectedPart != null && connection.connectedPart.partPrefab != null)
                    {
                        DiagramRegister[] allRegisters = FindObjectsByType<DiagramRegister>(FindObjectsSortMode.None);
                        foreach (var reg in allRegisters)
                        {
                            if (reg.GetDiagramNode() == connection.connectedPart)
                            {
                                PartsScript childScript = reg.GetComponent<PartsScript>();
                                if (childScript != null)
                                {
                                    ApplyLayerAndPhysicsRecursive(childScript, layer, kinematic, visited);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public PieceStatus GetStatus() { return _status; }
    public Rigidbody GetRigid() { return _rigid; }
    public float GetMass() { return _mass; }
    public PartsScript GetTargetPart() { return _partScriptTarget; }
}

public enum ConnectType
{
    none,
    male,
    famale
}

public enum PieceStatus
{
    none,
    conecting,
    conected,
    root
}