using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay
{
    public class RailFenceHackMinigame
    {
        private const string Alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        private readonly VisualElement _root;
        private readonly Action _onSuccess;
        private readonly string _targetDecryptedText;

        private Label _encryptedText;
        private Label _answerText;
        private VisualElement _lettersPanel;
        private Button _submitButton;
        private Button _deleteButton;
        private IntegerField _startDial;
        private IntegerField _stepDialField;

        private string _currentAnswer = string.Empty;
        private int _rails = 2;
        private int _startRow = 1;
        private string _baseEncryptedText = string.Empty;
        private string _normalizedTargetText = string.Empty;
        
        private List<int> _zigzagOrder = new();
        private int _currentZigzagIndex = 0;

        public RailFenceHackMinigame(VisualElement root, string encryptedText, string targetDecryptedText, Action onSuccess)
        {
            _root = root;
            _onSuccess = onSuccess;
            _targetDecryptedText = targetDecryptedText ?? string.Empty;
            _normalizedTargetText = _targetDecryptedText.ToUpper();

            BindElements();
            SetupInitial(encryptedText);
            RegisterCallbacks();

            BuildDecryptionModel();
            ApplyHighlightForStartRow();
        }

        private void BindElements()
        {
            _encryptedText = _root.Q<Label>("EncryptedText");
            _answerText = _root.Q<Label>("AnswerText");
            _lettersPanel = _root.Q<VisualElement>("LettersPanel");
            _submitButton = _root.Q<Button>("SubmitButton");
            _deleteButton = _root.Q<Button>("DeleteButton");
            _startDial = _root.Q<IntegerField>("StartDial");
            _stepDialField = _root.Q<IntegerField>("StepDial");
        }

        private void SetupInitial(string encryptedText)
        {
            _baseEncryptedText = encryptedText ?? string.Empty;
            if (_encryptedText != null) _encryptedText.text = _baseEncryptedText;

            int textLen = Math.Max(1, encryptedText?.Length ?? 1);
            if (_stepDialField != null)
                _stepDialField.value = Mathf.Clamp(_rails, 2, textLen);

            _startRow = 1;
            if (_startDial != null)
            {
                _startDial.value = _startRow;
                _startDial.isDelayed = false;
            }

            if (_answerText != null) _answerText.text = _currentAnswer;
        }

        private void RegisterCallbacks()
        {
            _submitButton?.RegisterCallback<ClickEvent>(_ => OnSubmit());
            _deleteButton?.RegisterCallback<ClickEvent>(_ => OnDelete());
            _stepDialField?.RegisterValueChangedCallback(evt => OnStepChanged(evt.newValue));
            _startDial?.RegisterValueChangedCallback(evt => OnStartChanged(evt.newValue));
            _startDial?.RegisterCallback<FocusOutEvent>(_ => _root?.Focus());
            _stepDialField?.RegisterCallback<FocusOutEvent>(_ => _root?.Focus());
            _root?.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            if (_lettersPanel != null)
                _lettersPanel.style.display = DisplayStyle.None;

            if (_root != null)
            {
                _root.focusable = true;
                _root.Focus();
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.target is VisualElement target && IsDialInputTarget(target))
                return;

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
                OnLetterSelected(' ');
                evt.StopPropagation();
                return;
            }

            if (evt.character == '\0' || char.IsControl(evt.character)) return;

            char input = char.ToUpper(evt.character);
            if (Alphabet.IndexOf(input) < 0) return;

            OnLetterSelected(input);
            evt.StopPropagation();
        }
        
        private void OnLetterSelected(char input)
        {
            char inputUpper = char.ToUpper(input);

            if (inputUpper != ' ' && Alphabet.IndexOf(inputUpper) < 0) return;
            if (_currentAnswer.Length >= _normalizedTargetText.Length) return;

            _currentAnswer += inputUpper;
            _currentZigzagIndex = _currentAnswer.Length;
            UpdateAnswerDisplay();
        }
        
        private void UpdateAnswerDisplay()
        {
            if (_answerText != null) _answerText.text = _currentAnswer;
        }

        private void OnDelete()
        {
            if (_currentAnswer.Length == 0) return;

            _currentAnswer = _currentAnswer.Substring(0, _currentAnswer.Length - 1);
            _currentZigzagIndex = _currentAnswer.Length;
            UpdateAnswerDisplay();
        }

        private void OnSubmit()
        {
            if (string.Equals(_currentAnswer, _normalizedTargetText, StringComparison.Ordinal))
                _onSuccess?.Invoke();
            else
            {
                ResetProgress();
            }
        }

        private void OnStepChanged(int newStep)
        {
            int textLen = Math.Max(1, _baseEncryptedText.Length);
            _rails = Mathf.Clamp(newStep, 2, textLen);
            if (_stepDialField != null && _stepDialField.value != _rails)
                _stepDialField.SetValueWithoutNotify(_rails);

            _startRow = Mathf.Clamp(_startRow, 1, _rails);
            if (_startDial != null) _startDial.value = _startRow;

            BuildDecryptionModel();
            ResetProgress();
            ApplyHighlightForStartRow();
            _root?.Focus();
        }

        private void OnStartChanged(int newStart)
        {
            _startRow = Mathf.Clamp(newStart, 1, _rails);
            if (_startDial != null && _startDial.value != _startRow)
                _startDial.SetValueWithoutNotify(_startRow);

            ApplyHighlightForStartRow();
            _root?.Focus();
        }

        private void BuildDecryptionModel()
        {
            int n = _baseEncryptedText.Length;
            _zigzagOrder.Clear();

            if (n <= 0) return;

            if (_rails <= 1)
            {
                for (int i = 0; i < n; i++)
                {
                    _zigzagOrder.Add(0);
                }

                _currentZigzagIndex = 0;
                return;
            }

            int row = 0;
            int dir = 1;
            for (int i = 0; i < n; i++)
            {
                _zigzagOrder.Add(row);
                row += dir;
                if (row >= _rails)
                {
                    row = _rails - 2;
                    dir = -1;
                }
                else if (row < 0)
                {
                    row = 1;
                    dir = 1;
                }
            }

            _currentZigzagIndex = 0;
        }

        private void ResetProgress()
        {
            _currentAnswer = string.Empty;
            _currentZigzagIndex = 0;
            UpdateAnswerDisplay();
        }

        private void ApplyHighlightForStartRow()
        {
            if (_encryptedText == null) return;
            var text = _baseEncryptedText ?? string.Empty;
            var indices = ComputeRailIndicesForCipher(text.Length, _rails, _startRow);
            _encryptedText.text = BuildHighlightedText(text, indices);
        }

        private static string BuildHighlightedText(string text, List<int> indices)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var targets = new HashSet<int>(indices);
            var sb = new StringBuilder(text.Length * 2);
            const string color = "#FFFF00";

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (targets.Contains(i) && !char.IsWhiteSpace(c))
                    sb.Append("<color=").Append(color).Append(">").Append(c).Append("</color>");
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private static List<int> ComputeRailIndicesForCipher(int length, int rails, int start)
        {
            var result = new List<int>();
            if (length <= 0 || rails < 1) return result;

            if (rails == 1)
            {
                for (int i = 0; i < length; i++)
                    result.Add(i);
                return result;
            }

            int targetRail = Mathf.Clamp(start - 1, 0, rails - 1);

            int[] rowCounts = new int[rails];
            int row = 0;
            int dir = 1;

            for (int i = 0; i < length; i++)
            {
                rowCounts[row]++;
                row += dir;
                if (row >= rails)
                {
                    row = rails - 2;
                    dir = -1;
                }
                else if (row < 0)
                {
                    row = 1;
                    dir = 1;
                }
            }

            int startIndex = 0;
            for (int r = 0; r < targetRail; r++)
                startIndex += rowCounts[r];

            int endIndex = startIndex + rowCounts[targetRail];

            for (int i = startIndex; i < endIndex; i++)
                result.Add(i);

            return result;
        }

        private bool IsDialInputTarget(VisualElement target)
        {
            return IsInsideElement(target, _startDial) || IsInsideElement(target, _stepDialField);
        }

        private static bool IsInsideElement(VisualElement target, VisualElement container)
        {
            if (target == null || container == null) return false;

            var current = target;
            while (current != null)
            {
                if (current == container) return true;
                current = current.parent;
            }

            return false;
        }
    }
}

