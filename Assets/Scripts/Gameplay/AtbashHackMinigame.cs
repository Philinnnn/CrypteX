using System;
using UnityEngine;
using UnityEngine.UIElements;
using UI.Util;

namespace Gameplay
{
    public class AtbashHackMinigame
    {
        private const string Alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        
        private VisualElement _root;
        private AtbashUIElements _uiElements;
        private UniversalHighlighter _highlighter;
        private Action _onSuccess;
        private string _targetDecryptedText;
        private string _currentAnswer = "";

        public AtbashHackMinigame(VisualElement root, string encryptedText, string targetDecryptedText, Action onSuccess)
        {
            _root = root;
            _targetDecryptedText = targetDecryptedText;
            _onSuccess = onSuccess;
            
            _uiElements = new AtbashUIElements(_root);
            _highlighter = new UniversalHighlighter(_uiElements.TopAlphabet, _uiElements.BottomAlphabet, UniversalHighlighter.CyrillicAlphabet, UniversalHighlighter.AtbashCyrillicTop, UniversalHighlighter.AtbashCyrillicBottom);
            
            SetupGameMode();
            DisplayText(encryptedText);
            RegisterCallbacks();
        }
        
        private void SetupGameMode()
        {
            HideElement(_uiElements.DemoLetterPairContainer);
            HideElement(_uiElements.DecryptedText);
            HideElement(_uiElements.LettersPanel);
            
            ShowElement(_uiElements.InputLabel);
            ShowElement(_uiElements.AnswerText);
            ShowElement(_uiElements.ButtonsContainer);
        }

        private void HideElement(VisualElement element)
        {
            if (element != null) element.style.display = DisplayStyle.None;
        }

        private void ShowElement(VisualElement element)
        {
            if (element != null) element.style.display = DisplayStyle.Flex;
        }

        private void DisplayText(string text)
        {
            if (_uiElements.EncryptedText != null)
                _uiElements.EncryptedText.text = text;
        }

        private void OnLetterSelected(char letter)
        {
            int index = Alphabet.IndexOf(letter);
            if (index >= 0)
            {
                _highlighter.HighlightAtIndex(32 - index, 32 - index);
                _currentAnswer += letter;
                UpdateAnswerDisplay();
            }
        }

        private void UpdateAnswerDisplay()
        {
            if (_uiElements.AnswerText != null)
                _uiElements.AnswerText.text = _currentAnswer;
        }

        private void RegisterCallbacks()
        {
            _uiElements.SubmitButton?.RegisterCallback<ClickEvent>(evt => OnSubmit());
            _uiElements.DeleteButton?.RegisterCallback<ClickEvent>(evt => OnDelete());
            _root?.RegisterCallback<KeyDownEvent>(OnKeyDown);

            if (_root != null)
            {
                _root.focusable = true;
                _root.Focus();
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Backspace || evt.keyCode == KeyCode.Delete)
            {
                OnDelete();
                evt.StopPropagation();
                return;
            }

            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                OnSubmit();
                evt.StopPropagation();
                return;
            }

            if (evt.keyCode == KeyCode.Space)
            {
                _currentAnswer += " ";
                UpdateAnswerDisplay();
                evt.StopPropagation();
                return;
            }

            if (evt.character == '\0' || char.IsControl(evt.character)) return;

            char input = char.ToUpper(evt.character);
            if (Alphabet.IndexOf(input) < 0) return;

            OnLetterSelected(input);
            evt.StopPropagation();
        }
        
        private void OnDelete()
        {
            if (_currentAnswer.Length > 0)
            {
                _currentAnswer = _currentAnswer.Substring(0, _currentAnswer.Length - 1);
                UpdateAnswerDisplay();
            }
        }


        private void OnSubmit()
        {
            Debug.Log($"OnSubmit: User entered '{_currentAnswer}', expected '{_targetDecryptedText}'");
            
            if (_currentAnswer == _targetDecryptedText)
            {
                Debug.Log($"✓ Правильно! '{_currentAnswer}' == '{_targetDecryptedText}'");
                _onSuccess?.Invoke();
            }
            else
            {
                Debug.Log($"✗ Неверно! Получено: '{_currentAnswer}', нужно: '{_targetDecryptedText}'");
                _currentAnswer = "";
                UpdateAnswerDisplay();
                _highlighter.Reset();
            }
        }
    }
}

