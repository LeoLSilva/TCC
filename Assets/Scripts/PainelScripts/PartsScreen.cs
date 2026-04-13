using NUnit.Framework;
using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartsScreen : MonoBehaviour
{
    [SerializeField] private List<PartsSoloScriptableObject> _parts = new List<PartsSoloScriptableObject>();
    [SerializeField] private GameObject _currentSelected;
    [SerializeField] private Image _diagramImage;

    [SerializeField] private GameObject _btnPrefab;
    [SerializeField] private GameObject _containerBtn;
    [SerializeField] private GameObject _printerBtn;
    [SerializeField] private PainelUI _painelUi;

    private PrinterManager _printerManager;

    private void Start()
    {
        _painelUi = FindAnyObjectByType<PainelUI>();
        _printerManager = FindAnyObjectByType<PrinterManager>();
        _printerManager.OnPrinterStateChanged += SetButtonPrintActive;
        SpawnIcon();
        ChangeSelected(_parts[0]);
    }

    private void SpawnIcon()
    {
        PointableUnityEventWrapper ev = null;
        foreach (PartsSoloScriptableObject part in _parts)
        {
            GameObject btn = Instantiate(_btnPrefab, _containerBtn.transform);
            btn.GetComponent<Image>().sprite = part.select;
            ev = btn.GetComponent<PointableUnityEventWrapper>();
            ev.WhenSelect.AddListener((_) => ChangeSelected(part));
        }
    }

    public void ChangeSelected(PartsSoloScriptableObject p)
    {
        _currentSelected = p.prefab;
        _diagramImage.sprite = p.diagram;
    }

    private void SetButtonPrintActive(bool obj)
    {
        BlockPrinterBtn(obj);
    }

    public void BlockPrinterBtn(bool block)
    {
        _printerBtn.GetComponent<RayInteractable>().enabled = !block;
        _painelUi.BlockButtonColor(_printerBtn.GetComponent<Image>(), block);
    }
    public void PrinterBtn()
    {
        _printerManager.Printer(_currentSelected);
    }
}
