using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class s223Dialogue : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _prevButton;

    [Header("Settings")]
    [SerializeField] private float _typingSpeed = 0.05f;
    [SerializeField] private float _timeToRead = 2f;

    private string[] _currentMessages;
    private int _currentIndex = 0;
    private bool _skipRequested = false;
    private bool _isReviewMode = false;
    private bool _isTyping = false;

    private void Awake()
    {
        if (_nextButton != null) _nextButton.onClick.AddListener(NextDialogue);
        if (_prevButton != null) _prevButton.onClick.AddListener(PreviousDialogue);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextDialogue();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousDialogue();
        }
    }

    public void HideDialogue()
    {
        if (_dialoguePanel != null) _dialoguePanel.SetActive(false);
        if (_dialogueText != null) _dialogueText.text = "";
    }

    public IEnumerator PlayDialogueRoutine(string[] messages)
    {
        _currentMessages = messages;
        _currentIndex = 0;
        _isReviewMode = false;
        _skipRequested = false;

        if (_dialoguePanel != null) _dialoguePanel.SetActive(true);
        UpdateButtons();

        while (_currentIndex < _currentMessages.Length)
        {
            _isTyping = true;
            _skipRequested = false;
            if (_dialogueText != null) _dialogueText.text = "";

            foreach (char letter in _currentMessages[_currentIndex].ToCharArray())
            {
                if (_skipRequested) break;
                if (_dialogueText != null) _dialogueText.text += letter;
                yield return new WaitForSeconds(_typingSpeed);
            }

            if (_dialogueText != null) _dialogueText.text = _currentMessages[_currentIndex];
            _isTyping = false;
            _skipRequested = false;

            float timer = 0f;
            while (timer < _timeToRead)
            {
                if (_skipRequested) break;
                timer += Time.deltaTime;
                yield return null;
            }

            _currentIndex++;
        }

        _currentIndex = _currentMessages.Length - 1;
        _isReviewMode = true;
        UpdateButtons();
    }

    public void NextDialogue()
    {
        if (!_isReviewMode)
        {
            _skipRequested = true;
            return;
        }

        if (_currentMessages == null || _currentMessages.Length == 0) return;

        if (_currentIndex < _currentMessages.Length - 1)
        {
            _currentIndex++;
            if (_dialogueText != null) _dialogueText.text = _currentMessages[_currentIndex];
            UpdateButtons();
        }
    }

    public void PreviousDialogue()
    {
        if (!_isReviewMode) return;

        if (_currentMessages == null || _currentMessages.Length == 0) return;

        if (_currentIndex > 0)
        {
            _currentIndex--;
            if (_dialogueText != null) _dialogueText.text = _currentMessages[_currentIndex];
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        if (!_isReviewMode)
        {
            if (_prevButton != null) _prevButton.gameObject.SetActive(false);
            if (_nextButton != null) _nextButton.gameObject.SetActive(true);
        }
        else
        {
            if (_prevButton != null) _prevButton.gameObject.SetActive(_currentIndex > 0);
            if (_nextButton != null) _nextButton.gameObject.SetActive(_currentIndex < _currentMessages.Length - 1);
        }
    }
}