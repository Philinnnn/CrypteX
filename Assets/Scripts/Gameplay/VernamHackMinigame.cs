using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay
{
    public class VernamHackMinigame
    {
        private const string RussianAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        private const string LatinAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private readonly VisualElement _root;
        private readonly Action _onSuccess;
        private readonly string _targetNormalized;
        private readonly string _keyNormalized;
        private readonly string _encryptedSource;

        private Label _answerLabel;
        private VisualElement _tapeContainer;
        private VisualElement _cheatSheetOverlay;
        private Button _submitButton;
        private Button _deleteButton;
        private Button _infoButton;
        private Button _closeInfoButton;

        private string _currentAnswer = string.Empty;

        public VernamHackMinigame(VisualElement root, string encryptedText, string targetDecryptedText, string key, Action onSuccess)
        {
            _root = root;
            _onSuccess = onSuccess;
            _targetNormalized = NormalizeForCompare(targetDecryptedText);
            _keyNormalized = NormalizeKey(key);
            _encryptedSource = encryptedText ?? string.Empty;

            BindElements();
            RegisterCallbacks();
            RebuildTape();
            UpdateAnswerDisplay();
        }

        private void BindElements()
        {
            _answerLabel = _root.Q<Label>("AnswerText");
            _tapeContainer = _root.Q<VisualElement>("TapeContainer");
            _cheatSheetOverlay = _root.Q<VisualElement>("CheatSheetOverlay");
            _submitButton = _root.Q<Button>("SubmitButton");
            _deleteButton = _root.Q<Button>("DeleteButton");
            _infoButton = _root.Q<Button>("InfoButton");
            _closeInfoButton = _root.Q<Button>("CloseInfoButton");
        }

        private void RegisterCallbacks()
        {
            _submitButton?.RegisterCallback<ClickEvent>(_ => OnSubmit());
            _deleteButton?.RegisterCallback<ClickEvent>(_ => OnDelete());
            _infoButton?.RegisterCallback<ClickEvent>(_ => ToggleCheatSheet(true));
            _closeInfoButton?.RegisterCallback<ClickEvent>(_ => ToggleCheatSheet(false));
            _root?.RegisterCallback<KeyDownEvent>(OnKeyDown);

            if (_root != null)
            {
                _root.focusable = true;
                _root.Focus();
            }

            ToggleCheatSheet(false);
        }

        private void ToggleCheatSheet(bool show)
        {
            if (_cheatSheetOverlay != null)
                _cheatSheetOverlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            if (!show && _root != null)
                _root.Focus();
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
                OnLetterSelected(' ');
                evt.StopPropagation();
                return;
            }

            if (evt.character == '\0' || char.IsControl(evt.character)) return;

            char input = char.ToUpper(evt.character);
            if (!IsAllowedLetter(input)) return;

            OnLetterSelected(input);
            evt.StopPropagation();
        }

        private void OnLetterSelected(char letter)
        {
            if (_currentAnswer.Length >= _targetNormalized.Length) return;

            char normalized = letter == ' ' ? ' ' : char.ToUpper(letter);
            if (normalized != ' ' && !IsAllowedLetter(normalized)) return;

            _currentAnswer += normalized;
            UpdateAnswerDisplay();
            RebuildTape();
        }

        private void OnDelete()
        {
            if (_currentAnswer.Length == 0) return;

            _currentAnswer = _currentAnswer.Substring(0, _currentAnswer.Length - 1);
            UpdateAnswerDisplay();
            RebuildTape();
        }

        private void OnSubmit()
        {
            if (NormalizeForCompare(_currentAnswer) == _targetNormalized)
            {
                _onSuccess?.Invoke();
                return;
            }

            if (_answerLabel != null)
                _answerLabel.text = "НЕВЕРНО";

            _root?.schedule.Execute(() =>
            {
                _currentAnswer = string.Empty;
                UpdateAnswerDisplay();
                RebuildTape();
            }).StartingIn(900);
        }

        private void UpdateAnswerDisplay()
        {
            if (_answerLabel != null)
                _answerLabel.text = _currentAnswer;
        }

        private static bool IsAllowedLetter(char c)
        {
            return RussianAlphabet.IndexOf(c) >= 0 || LatinAlphabet.IndexOf(c) >= 0;
        }

        private void RebuildTape()
        {
            if (_tapeContainer == null) return;
            _tapeContainer.Clear();

            int keyIndex = 0;

            for (int i = 0; i < _encryptedSource.Length; i++)
            {
                char c = char.ToUpper(_encryptedSource[i]);
                bool isLetter = IsAllowedLetter(c);
                char k = ' ';

                if (isLetter && keyIndex < _keyNormalized.Length)
                {
                    k = _keyNormalized[keyIndex];
                    keyIndex++;
                }

                char p = i < _currentAnswer.Length ? _currentAnswer[i] : ' ';

                VisualElement column = new VisualElement();
                column.style.flexDirection = FlexDirection.Column;
                column.style.alignItems = Align.Center;
                column.style.marginRight = isLetter ? 5 : 15;
                column.style.marginLeft = isLetter ? 5 : 15;

                column.Add(CreateTapeCell(isLetter ? c.ToString() : c.ToString(), true));
                column.Add(CreateTapeCell(isLetter ? k.ToString() : " ", false, true));
                column.Add(CreateTapeCell(isLetter ? p.ToString() : " ", false, false, i < _currentAnswer.Length));

                _tapeContainer.Add(column);
            }
        }

        private static VisualElement CreateTapeCell(string text, bool isCipher, bool isKey = false, bool hasInput = false)
        {
            var cell = new Label(text);
            cell.style.width = 40;
            cell.style.height = 50;
            cell.style.marginBottom = 5;
            cell.style.unityTextAlign = TextAnchor.MiddleCenter;
            cell.style.unityFontStyleAndWeight = FontStyle.Bold;
            cell.style.fontSize = 24;
            
            cell.style.borderTopWidth = 2;
            cell.style.borderBottomWidth = 2;
            cell.style.borderLeftWidth = 2;
            cell.style.borderRightWidth = 2;

            if (isCipher)
            {
                cell.style.color = new Color(0.38f, 0.87f, 0.92f);
                var c = new Color(0.38f, 0.87f, 0.92f, 0.5f);
                cell.style.borderTopColor = c;
                cell.style.borderBottomColor = c;
                cell.style.borderLeftColor = c;
                cell.style.borderRightColor = c;
                cell.style.backgroundColor = new Color(0.38f, 0.87f, 0.92f, 0.1f);
            }
            else if (isKey)
            {
                cell.style.color = new Color(1f, 0.78f, 0.39f);
                var c = new Color(1f, 0.78f, 0.39f, 0.5f);
                cell.style.borderTopColor = c;
                cell.style.borderBottomColor = c;
                cell.style.borderLeftColor = c;
                cell.style.borderRightColor = c;
                cell.style.backgroundColor = new Color(1f, 0.78f, 0.39f, 0.1f);
            }
            else
            {
                cell.style.color = hasInput ? Color.white : new Color(0.5f, 0.5f, 0.5f);
                var c = hasInput ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.3f);
                cell.style.borderTopColor = c;
                cell.style.borderBottomColor = c;
                cell.style.borderLeftColor = c;
                cell.style.borderRightColor = c;
                cell.style.backgroundColor = hasInput ? new Color(1, 1, 1, 0.1f) : Color.clear;
            }

            return cell;
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            char[] buffer = key.ToUpper().ToCharArray();
            var chars = new System.Text.StringBuilder(buffer.Length);
            for (int i = 0; i < buffer.Length; i++)
                if (IsAllowedLetter(buffer[i])) chars.Append(buffer[i]);
            return chars.ToString();
        }

        private static string NormalizeForCompare(string text)
        {
            return (text ?? string.Empty).Trim().ToUpper();
        }
    }
}
