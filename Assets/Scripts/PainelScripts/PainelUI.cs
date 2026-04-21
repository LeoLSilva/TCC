using NUnit.Framework;
using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PainelUI : MonoBehaviour
{
    [SerializeField] private TabData _currentScreen;
    [SerializeField] private MissionManager _missionManager;

    [Header("Configuração dos Itens")]
    [SerializeField] private List<TabData> _tabs = new List<TabData>();
    private Color32 _clickedColor = new Color32(255, 255, 146, 255);
    private Color32 _blockedButton = new Color32(186, 186, 186, 255);
    private Color32 _naturalColor = Color.white;
    private int _currentIndex = -1;

    private void Awake()
    {
        if (_missionManager == null)
        {
            _missionManager = FindAnyObjectByType<MissionManager>();
        }
    }

    void Start()
    {
        DisableAll();
        if (_tabs.Count > 0) SelectTab(0);
    }

    private void OnEnable()
    {
        if (_missionManager != null)
        {
            _missionManager.OnMissionChanged += SetMission;
        }
    }

    private void OnDisable()
    {
        if (_missionManager != null)
        {
            _missionManager.OnMissionChanged -= SetMission;
        }
    }

    private void SetMission(MissionState obj)
    {
        if (obj == MissionState.Mission1)
        {
            FirstMissionSetup();
        }
        else if (obj == MissionState.Mission2)
        {
            SecondMissionSetup();
        }
    }

    private void FirstMissionSetup()
    {
        for (int i = 1; i < _tabs.Count; i++)
        {
            _tabs[i].buttonImage.color = _blockedButton;

            var interactable = _tabs[i].buttonImage.GetComponent<PointableUnityEventWrapper>();
            if (interactable != null)
            {
                interactable.enabled = false;
            }
        }
        SelectTab(0);
    }

    private void SecondMissionSetup()
    {
        if (_tabs.Count > 1)
        {
            _tabs[1].buttonImage.color = _naturalColor;

            var interactable = _tabs[1].buttonImage.GetComponent<PointableUnityEventWrapper>();
            if (interactable != null)
            {
                interactable.enabled = true;
            }
        }
        SelectTab(1);
    }

    private void DisableAll()
    {
        foreach (TabData t in _tabs)
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
            if (_currentScreen.buttonImage != null)
            {
                _currentScreen.buttonImage.color = _naturalColor;
            }
        }

        _currentScreen = _tabs[index];
        _currentScreen.panel.SetActive(true);
        if (_currentScreen.buttonImage != null)
        {
            _currentScreen.buttonImage.color = _clickedColor;
        }
    }

    public void BlockButtonColor(Image buttonImage, bool block)
    {
        if (buttonImage != null)
        {
            if (block)
                buttonImage.color = _blockedButton;
            else
                buttonImage.color = _naturalColor;
        }
    }

    public void ChangeScreenSelect(PartsSoloScriptableObject part)
    {
        if (_currentScreen.panel != null)
        {
            _currentScreen.panel.SetActive(false);
        }

        SelectTab(0);

        if (_currentScreen.panel != null)
        {
            _currentScreen.panel.GetComponent<PartsScreen>().ChangeSelected(part);
        }
    }
}

[System.Serializable]
public struct TabData
{
    public Image buttonImage;
    public GameObject panel;
}