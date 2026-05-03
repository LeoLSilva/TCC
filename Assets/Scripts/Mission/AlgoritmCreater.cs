using System.Collections;
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

    [Header("Scroll System")]
    [SerializeField] private GameObject _btnUp;
    [SerializeField] private GameObject _btnDown;
    [SerializeField] private int _limitToScroll = 8;

    private int _currentScrollIndex = 0;

    [Header("Testes")]
    [SerializeField] private PlayerActionType _testActionType;
    [SerializeField] private Sprite _testItem1;
    [SerializeField] private Sprite _testItem2;
    [SerializeField] private GameObject _testContainer;

    private void Start()
    {
        UpdateScrollPosition();
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

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ScrollUp();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ScrollDown();
        }
    }

    private float GetDynamicStep()
    {
        float step = 100f;
        if (_linePrefab != null)
        {
            RectTransform prefabRect = _linePrefab.GetComponent<RectTransform>();
            if (prefabRect != null)
            {
                step = prefabRect.rect.height;
            }
        }

        if (_diagramsContainer != null)
        {
            VerticalLayoutGroup vlg = _diagramsContainer.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                step += vlg.spacing;
            }
        }
        return step;
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
        StartCoroutine(AutoScrollToBottomRoutine());
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

                    if (_armDiagram != null && sig == _armDiagram.signature) finalResult = _armDiagram.ImgForAlgoritm;
                    else if (_headDiagram != null && sig == _headDiagram.signature) finalResult = _headDiagram.ImgForAlgoritm;
                    else if (_furbotDiagram != null && sig == _furbotDiagram.signature) finalResult = _furbotDiagram.ImgForAlgoritm;

                    if (finalResult != null) break;
                }
            }

            lineUI.SetupLine(PlayerActionType.Connect, _cont, item1, item2, finalResult);
            _cont++;
        }
        StartCoroutine(AutoScrollToBottomRoutine());
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
        StartCoroutine(AutoScrollToBottomRoutine());
    }

    public void ClearAlgorithm()
    {
        if (_diagramsContainer != null)
        {
            foreach (Transform child in _diagramsContainer)
            {
                Destroy(child.gameObject);
            }
        }
        _cont = 0;
        _currentScrollIndex = 0;
        UpdateScrollPosition();
    }

    private IEnumerator AutoScrollToBottomRoutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (_diagramsContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_diagramsContainer.GetComponent<RectTransform>());
        }

        int childCount = _diagramsContainer.childCount;
        if (childCount > _limitToScroll)
        {
            _currentScrollIndex = childCount - _limitToScroll;
        }
        else
        {
            _currentScrollIndex = 0;
        }
        UpdateScrollPosition();
    }

    public void ScrollUp()
    {
        if (_currentScrollIndex > 0)
        {
            _currentScrollIndex--;
            UpdateScrollPosition();
        }
    }

    public void ScrollDown()
    {
        int childCount = _diagramsContainer.childCount;
        int maxIndex = Mathf.Max(0, childCount - _limitToScroll);

        if (_currentScrollIndex < maxIndex)
        {
            _currentScrollIndex++;
            UpdateScrollPosition();
        }
    }

    private void UpdateScrollPosition()
    {
        if (_diagramsContainer == null) return;

        int childCount = _diagramsContainer.childCount;
        int maxIndex = Mathf.Max(0, childCount - _limitToScroll);

        RectTransform rect = _diagramsContainer.GetComponent<RectTransform>();
        if (rect != null)
        {
            float targetY = _currentScrollIndex * GetDynamicStep();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, targetY);
        }

        if (_btnUp != null && _btnDown != null)
        {
            if (childCount <= _limitToScroll)
            {
                _btnUp.SetActive(false);
                _btnDown.SetActive(false);
            }
            else
            {
                _btnUp.SetActive(_currentScrollIndex > 0);
                _btnDown.SetActive(_currentScrollIndex < maxIndex);
            }
        }
    }
}