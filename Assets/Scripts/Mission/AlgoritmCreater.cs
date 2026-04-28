using UnityEngine;
using UnityEngine.UI;

public enum PlayerActionType
{
    Print,
    Connect,
    Disconnect
}

public class AlgoritmCreater : MonoBehaviour
{
    [SerializeField] private Transform _diagramsContainer;
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private int _cont = 0;

    [Header("Diagramas Validos")]
    [SerializeField] private DiagramScriptableObject _armDiagram;
    [SerializeField] private DiagramScriptableObject _headDiagram;
    [SerializeField] private DiagramScriptableObject _furbotDiagram;

    [Header("Manual Scroll System")]
    [SerializeField] private GameObject _btnUp;
    [SerializeField] private GameObject _btnDown;
    [SerializeField] private int _limitToScroll = 9;
    private int _startIndex = 0;

    [Header("Testes")]
    [SerializeField] private PlayerActionType _testActionType;
    [SerializeField] private Sprite _testItem1;
    [SerializeField] private Sprite _testItem2;
    [SerializeField] private GameObject _testContainer;

    private void Start()
    {
        UpdateManualScroll();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            switch (_testActionType)
            {
                case PlayerActionType.Print:
                    RegisterPrint(_testItem1);
                    break;
                case PlayerActionType.Connect:
                    RegisterConnection(_testItem1, _testItem2, _testContainer);
                    break;
                case PlayerActionType.Disconnect:
                    RegisterDisconnection(_testItem1, _testItem2);
                    break;
            }
        }
    }

    public void RegisterPrint(Sprite printedItem)
    {
        GameObject newLine = Instantiate(_linePrefab, _diagramsContainer);
        ActionLineUI lineUI = newLine.GetComponent<ActionLineUI>();

        if (lineUI != null)
        {
            lineUI.SetupLine(PlayerActionType.Print, _cont, printedItem);
            _cont++;
        }
        AutoScrollToBottom();
    }

    public void RegisterConnection(Sprite item1, Sprite item2, GameObject container)
    {
        GameObject newLine = Instantiate(_linePrefab, _diagramsContainer);
        ActionLineUI lineUI = newLine.GetComponent<ActionLineUI>();

        if (lineUI != null)
        {
            Sprite finalResult = null;

            if (container != null)
            {
                DiagramRegister[] registers = container.GetComponentsInChildren<DiagramRegister>();
                foreach (var reg in registers)
                {
                    string sig = reg.GetLocalSignature();

                    if (_armDiagram != null && sig == _armDiagram.signature) finalResult = _armDiagram.diagramImage;
                    else if (_headDiagram != null && sig == _headDiagram.signature) finalResult = _headDiagram.diagramImage;
                    else if (_furbotDiagram != null && sig == _furbotDiagram.signature) finalResult = _furbotDiagram.diagramImage;

                    if (finalResult != null) break;
                }
            }

            lineUI.SetupLine(PlayerActionType.Connect, _cont, item1, item2, finalResult);
            _cont++;
        }
        AutoScrollToBottom();
    }

    public void RegisterDisconnection(Sprite item1, Sprite item2)
    {
        GameObject newLine = Instantiate(_linePrefab, _diagramsContainer);
        ActionLineUI lineUI = newLine.GetComponent<ActionLineUI>();

        if (lineUI != null)
        {
            lineUI.SetupLine(PlayerActionType.Disconnect, _cont, item1, item2);
            _cont++;
        }
        AutoScrollToBottom();
    }

    private void AutoScrollToBottom()
    {
        int childCount = _diagramsContainer.childCount;
        if (childCount > _limitToScroll)
        {
            _startIndex = childCount - _limitToScroll;
        }
        UpdateManualScroll();
    }

    public void ScrollUp()
    {
        if (_startIndex > 0)
        {
            _startIndex--;
            UpdateManualScroll();
        }
    }

    public void ScrollDown()
    {
        int childCount = _diagramsContainer.childCount;
        if (_startIndex + _limitToScroll < childCount)
        {
            _startIndex++;
            UpdateManualScroll();
        }
    }

    private void UpdateManualScroll()
    {
        if (_btnUp == null || _btnDown == null) return;

        int childCount = _diagramsContainer.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = _diagramsContainer.GetChild(i);
            bool shouldShow = (i >= _startIndex && i < _startIndex + _limitToScroll);
            child.gameObject.SetActive(shouldShow);
        }

        if (childCount <= _limitToScroll)
        {
            _btnUp.SetActive(false);
            _btnDown.SetActive(false);
        }
        else
        {
            _btnUp.SetActive(_startIndex > 0);
            _btnDown.SetActive(_startIndex + _limitToScroll < childCount);
        }
    }
}