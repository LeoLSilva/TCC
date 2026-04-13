using NUnit.Framework;
using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PainelUI : MonoBehaviour
{

    [SerializeField] private TabData _currentScreen;

    [System.Serializable]
    public struct TabData
    {
        public Image buttonImage;
        public GameObject panel;
    }

    [Header("Configura��o dos Itens")]
    [SerializeField] private List<TabData> _tabs;
    private Color32 _clickedColor = new Color32(255, 255, 146, 255);
    private Color32 _blockedButton = new Color32(186, 186, 186, 255);
    private Color32 _naturalColor = Color.white;
    private int _currentIndex = -1;


    void Start()
    {
        DisableAll();
        if (_tabs.Count > 0) SelectTab(0);
    }

    private void DisableAll()
    {
        foreach(TabData t in _tabs)
        {
            t.panel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectTab(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectTab(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectTab(2);
        }
    }
    public void SelectTab(int index)
    {
        if (index < 0 || index >= _tabs.Count) return;

        if (_currentScreen.panel != null)
        {
            _currentScreen.panel.SetActive(false);
            _currentScreen.buttonImage.color = _naturalColor;
        }
        _currentScreen = _tabs[index];
        _currentScreen.panel.SetActive(true);
        _currentScreen.buttonImage.color = _clickedColor;
    }

    public void BlockButtonColor(Image buttonImage, bool block)
    {
        if (block)
            buttonImage.color = _blockedButton;
        else
            buttonImage.color = _naturalColor;
    }

    public void ChangeScreenSelect(PartsSoloScriptableObject part){
        _currentScreen.panel.SetActive(false);
        SelectTab(0);
        _currentScreen.panel.GetComponent<PartsScreen>().ChangeSelected(part);
    }
}
