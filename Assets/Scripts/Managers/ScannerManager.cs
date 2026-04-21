using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ScannerManager : MonoBehaviour
{
    [SerializeField] private ScannerTool _scannerTool;
    [SerializeField] private Material _scanMaterial;
    [SerializeField] private MissionManager _missionManager;

    public UnityEvent<string> OnFirstTimeDiscovered;
    public UnityEvent OnAlreadyDiscovered;
    public UnityEvent OnScanFailedNotSolo;

    private HashSet<string> _discoveredSignatures = new HashSet<string>();
    private ScanManager _currentVisualManager;

    private void OnEnable()
    {
        if (_scannerTool != null)
        {
            _scannerTool.OnScanStarted.AddListener(HandleScanStarted);
            _scannerTool.OnScanComplete.AddListener(HandleScanComplete);
            _scannerTool.OnScanCanceled.AddListener(HandleScanCanceled);
        }
    }

    private void OnDisable()
    {
        if (_scannerTool != null)
        {
            _scannerTool.OnScanStarted.RemoveListener(HandleScanStarted);
            _scannerTool.OnScanComplete.RemoveListener(HandleScanComplete);
            _scannerTool.OnScanCanceled.RemoveListener(HandleScanCanceled);
        }
    }

    private void HandleScanStarted(GameObject target)
    {
        if (_missionManager != null && _missionManager.GetMissionState() == MissionState.Disabled) return;

        Transform rootObj = target.transform.root;
        _currentVisualManager = rootObj.GetComponent<ScanManager>();

        if (_currentVisualManager == null)
        {
            _currentVisualManager = rootObj.gameObject.AddComponent<ScanManager>();
            _currentVisualManager.scanMaterial = _scanMaterial;
        }

        _currentVisualManager.effectActive = true;
    }

    private void HandleScanComplete(GameObject target)
    {
        StopVisualEffect();

        if (_missionManager == null) return;

        switch (_missionManager.GetMissionState())
        {
            case MissionState.Mission1:
                ExecuteSaveDiagram(target, true, StorageManager.PathPecasSolos);
                break;
            case MissionState.FreeMode:
                ExecuteSaveDiagram(target, false, StorageManager.PathDiagramas);
                break;
        }
    }

    private void HandleScanCanceled()
    {
        StopVisualEffect();
    }

    private void StopVisualEffect()
    {
        if (_currentVisualManager != null)
        {
            _currentVisualManager.effectActive = false;
            _currentVisualManager = null;
        }
    }

    private void ExecuteSaveDiagram(GameObject hitObj, bool requireSolo, string savePath)
    {
        PartsScript hitPart = hitObj.GetComponentInParent<PartsScript>();
        if (hitPart != null)
        {
            PartsScript rootPart = hitPart.FindRootPart();
            if (rootPart != null)
            {
                GameObject targetToSave = null;

                if (rootPart.transform.parent != null && rootPart.transform.parent.name.Contains("DiagramContainer"))
                {
                    targetToSave = rootPart.transform.parent.gameObject;
                }
                else
                {
                    targetToSave = rootPart.gameObject;
                }

                if (targetToSave != null)
                {
                    PartsScript[] parts = targetToSave.GetComponentsInChildren<PartsScript>(true);

                    if (requireSolo && parts.Length > 1)
                    {
                        OnScanFailedNotSolo?.Invoke();
                        return;
                    }

                    foreach (PartsScript part in parts)
                    {
                        if (part.GetStatus() == PieceStatus.root || part.GetStatus() == PieceStatus.none)
                        {
                            DiagramRegister register = part.GetComponent<DiagramRegister>();
                            if (register != null)
                            {
                                string signature = register.GetLocalSignature();

                                if (!_discoveredSignatures.Contains(signature))
                                {
                                    _discoveredSignatures.Add(signature);
                                    OnFirstTimeDiscovered?.Invoke(signature);
                                }
                                else
                                {
                                    OnAlreadyDiscovered?.Invoke();
                                }
                            }

                            part.SaveDiagram(savePath);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void AddPreDiscoveredSignature(string signature)
    {
        if (!_discoveredSignatures.Contains(signature))
        {
            _discoveredSignatures.Add(signature);
        }
    }
}