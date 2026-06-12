using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay
{
    public class VigenereHackMinigame
    {
        private const string TableAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string RussianAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        private const string LatinAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private readonly VisualElement _root;
        private readonly Action _onSuccess;
        private readonly string _targetNormalized;

        private Label _keyLabel;
        private Label _encryptedLabel;
        private Label _encryptedOverlayLabel;
        private Label _answerLabel;
        private VisualElement _alphabetHeaderRow;
        private VisualElement _expandedKeyRowsContainer;
        private VisualElement _cheatSheetOverlay;
        private Button _submitButton;
        private Button _deleteButton;
        private Button _infoButton;
        private Button _closeInfoButton;

        private string _currentAnswer = string.Empty;
        private string _encryptedSource = string.Empty;
        private string _expandedKeyRowBase = string.Empty;

        public VigenereHackMinigame(VisualElement root, string encryptedText, string targetDecryptedText, string key, Action onSuccess)
        {
            _root = root;
            _onSuccess = onSuccess;
            _targetNormalized = NormalizeForCompare(targetDecryptedText);

            BindElements();
            InitializeLabels(encryptedText, key);
            RegisterCallbacks();
            UpdateAnswerDisplay();
        }

        private void BindElements()
        {
            _keyLabel = _root.Q<Label>("KeyLabel");
            _encryptedLabel = _root.Q<Label>("EncryptedText");
            _encryptedOverlayLabel = _root.Q<Label>("EncryptedTextOverlay");
            _answerLabel = _root.Q<Label>("AnswerText");
            _alphabetHeaderRow = _root.Q<VisualElement>("AlphabetHeaderRow");
            _expandedKeyRowsContainer = _root.Q<VisualElement>("ExpandedKeyRowsContainer");
            _cheatSheetOverlay = _root.Q<VisualElement>("CheatSheetOverlay");
            _submitButton = _root.Q<Button>("SubmitButton");
            _deleteButton = _root.Q<Button>("DeleteButton");
            _infoButton = _root.Q<Button>("InfoButton");
            _closeInfoButton = _root.Q<Button>("CloseInfoButton");
        }

        private void InitializeLabels(string encryptedText, string key)
        {
            _encryptedSource = encryptedText ?? string.Empty;
            string normalizedKey = NormalizeKey(key);

            if (_keyLabel != null)
                _keyLabel.text = string.IsNullOrWhiteSpace(normalizedKey) ? "-" : normalizedKey;

            if (_encryptedLabel != null)
                _encryptedLabel.text = _encryptedSource;

            if (_encryptedOverlayLabel != null)
                _encryptedOverlayLabel.text = string.IsNullOrEmpty(_encryptedSource) ? "-" : _encryptedSource;

            _expandedKeyRowBase = BuildExpandedKeyRow(_encryptedSource, normalizedKey);
            UpdateTableHighlight();
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
            UpdateTableHighlight();
        }

        private void OnDelete()
        {
            if (_currentAnswer.Length == 0) return;

            _currentAnswer = _currentAnswer.Substring(0, _currentAnswer.Length - 1);
            UpdateAnswerDisplay();
            UpdateTableHighlight();
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
                UpdateTableHighlight();
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

        private void UpdateTableHighlight()
        {
            RebuildAlphabetHeader(-1);
            RebuildExpandedKeyRows(-1, -1);
        }

        private void RebuildAlphabetHeader(int highlightedColumn)
        {
            if (_alphabetHeaderRow == null)
                return;

            _alphabetHeaderRow.Clear();
            _alphabetHeaderRow.Add(CreateTableCell(" ", false, isHeader: true, isKeyCell: true));

            for (int i = 0; i < TableAlphabet.Length; i++)
            {
                _alphabetHeaderRow.Add(CreateTableCell(TableAlphabet[i].ToString(), false, isHeader: true));
            }
        }

        private void RebuildExpandedKeyRows(int activePosition, int highlightedColumn)
        {
            if (_expandedKeyRowsContainer == null)
                return;

            _expandedKeyRowsContainer.Clear();

            if (string.IsNullOrEmpty(_expandedKeyRowBase))
            {
                _expandedKeyRowsContainer.Add(new Label("-"));
                return;
            }

            for (int i = 0; i < _expandedKeyRowBase.Length; i++)
            {
                char keyChar = char.ToUpper(_expandedKeyRowBase[i]);

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 2;

                bool isLetterRow = LatinAlphabet.IndexOf(keyChar) >= 0;
                row.Add(CreateTableCell(isLetterRow ? keyChar.ToString() : "", false, isKeyCell: true));

                int shift = TableAlphabet.IndexOf(keyChar);
                for (int col = 0; col < TableAlphabet.Length; col++)
                {
                    if (!isLetterRow)
                    {
                        row.Add(CreateTableCell("", false));
                        continue;
                    }

                    int shiftedIndex = (shift + col) % TableAlphabet.Length;
                    row.Add(CreateTableCell(TableAlphabet[shiftedIndex].ToString(), false));
                }

                _expandedKeyRowsContainer.Add(row);
            }

            if (_expandedKeyRowsContainer.childCount == 0)
                _expandedKeyRowsContainer.Add(new Label("-"));
        }

        private static VisualElement CreateTableCell(
            string text,
            bool highlighted,
            bool isHeader = false,
            bool isKeyCell = false)
        {
            var cell = new Label(text);
            cell.style.width = isKeyCell ? 34 : 32;
            cell.style.height = 30;
            cell.style.marginRight = 2;
            cell.style.marginBottom = 2;
            cell.style.unityTextAlign = TextAnchor.MiddleCenter;
            cell.style.unityFontStyleAndWeight = FontStyle.Bold;
            cell.style.borderTopWidth = 1;
            cell.style.borderBottomWidth = 1;
            cell.style.borderLeftWidth = 1;
            cell.style.borderRightWidth = 1;
            cell.style.borderTopColor = new Color(0.38f, 0.87f, 0.92f, 0.5f);
            cell.style.borderBottomColor = new Color(0.38f, 0.87f, 0.92f, 0.5f);
            cell.style.borderLeftColor = new Color(0.38f, 0.87f, 0.92f, 0.5f);
            cell.style.borderRightColor = new Color(0.38f, 0.87f, 0.92f, 0.5f);

            if (isKeyCell)
            {
                cell.style.color = new Color(1f, 0.78f, 0.39f);
                cell.style.backgroundColor = new Color(1f, 0.78f, 0.39f, 0.08f);
            }
            else if (highlighted)
            {
                cell.style.color = Color.black;
                cell.style.backgroundColor = new Color(1f, 1f, 0f, 0.95f);
            }
            else if (isHeader)
            {
                cell.style.color = new Color(0.38f, 0.87f, 0.92f);
                cell.style.backgroundColor = new Color(0.38f, 0.87f, 0.92f, 0.08f);
            }
            else
            {
                cell.style.color = new Color(0.85f, 0.95f, 1f);
                cell.style.backgroundColor = new Color(1f, 1f, 1f, 0.03f);
            }

            return cell;
        }



        private static string BuildExpandedKeyRow(string encryptedText, string key)
        {
            string normalizedKey = NormalizeKey(key);
            if (string.IsNullOrEmpty(normalizedKey) || string.IsNullOrEmpty(encryptedText)) return string.Empty;

            char[] result = new char[encryptedText.Length];
            int keyIndex = 0;

            for (int i = 0; i < encryptedText.Length; i++)
            {
                char symbol = char.ToUpper(encryptedText[i]);
                if (IsAllowedLetter(symbol))
                {
                    result[i] = normalizedKey[keyIndex % normalizedKey.Length];
                    keyIndex++;
                }
                else
                {
                    result[i] = encryptedText[i] == ' ' ? ' ' : '·';
                }
            }

            return new string(result);
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;

            char[] buffer = key.ToUpper().ToCharArray();
            var chars = new System.Text.StringBuilder(buffer.Length);
            for (int i = 0; i < buffer.Length; i++)
            {
                if (IsAllowedLetter(buffer[i]))
                    chars.Append(buffer[i]);
            }

            return chars.ToString();
        }

        private static string NormalizeForCompare(string text)
        {
            return (text ?? string.Empty).Trim().ToUpper();
        }
    }
}
