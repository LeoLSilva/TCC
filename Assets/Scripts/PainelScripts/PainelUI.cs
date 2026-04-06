using NUnit.Framework;
using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PainelUI : MonoBehaviour
{


    [System.Serializable]
    public struct TabData
    {
        public Image buttonImage;
        public GameObject panel;
    }

    [Header("Configuração dos Itens")]
    [SerializeField] private List<TabData> _tabs;
    private Color32 _clickedColor = new Color32(255, 255, 146, 255);
    private Color32 _blockedButton = new Color32(186, 186, 186, 255);
    private Color32 _naturalColor = Color.white;
    private int _currentIndex = -1;


    void Start()
    {
        if (_tabs.Count > 0) SelectTab(0);
    }
    public void SelectTab(int index)
    {
        if (index < 0 || index >= _tabs.Count) return;

        if (_currentIndex != -1)
        {
            _tabs[_currentIndex].panel.SetActive(false);
            _tabs[_currentIndex].buttonImage.color = _naturalColor;
        }

        _tabs[index].panel.SetActive(true);
        _tabs[index].buttonImage.color = _clickedColor;

        _currentIndex = index;
    }

    public void BlockButtonColor(Image buttonImage, bool block)
    {
        if (block)
            buttonImage.color = _blockedButton;
        else
            buttonImage.color = _naturalColor;
    }
}
