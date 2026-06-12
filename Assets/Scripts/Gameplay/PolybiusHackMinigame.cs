using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay
{
    public class PolybiusHackMinigame
    {
        private VisualElement _root;
        private Label _encryptedLabel;
        private VisualElement _gridPanel;
        private Label _answerLabel;
        private Button _submitButton;
        private Button _deleteButton;
        
        private string _targetDecryptedText;
        private string _currentAnswer = "";
        private Action _onSuccess;
        private readonly HashSet<char> _allowedInput = new();
        
        private char[,] _grid = new char[6, 6];

        public PolybiusHackMinigame(VisualElement root, string encryptedText, string targetDecryptedText, string key, Action onSuccess)
        {
            _root = root;
            _targetDecryptedText = targetDecryptedText;
            _onSuccess = onSuccess;

            GenerateGrid(key);
            BuildAllowedInputSet();
            BindElements();
            SetupGrid();
            _encryptedLabel.text = encryptedText;
            UpdateAnswerDisplay();
        }

        private void GenerateGrid(string key)
        {
            string defaultAlphabet = "ЛАБИРНТВГДЕЁЖЗИЙКМОПСУФХЦЧШЩЪЫЬЭЮЯ.,?";
            string fullString = "";
            
            if (!string.IsNullOrEmpty(key))
            {
                fullString += key.ToUpper();
            }
            fullString += defaultAlphabet;
            
            string uniqueParams = "";
            foreach (char c in fullString)
            {
                if (!uniqueParams.Contains(c.ToString()))
                {
                    uniqueParams += c;
                }
            }
            
            int index = 0;
            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 6; c++)
                {
                    if (index < uniqueParams.Length)
                    {
                        _grid[r, c] = uniqueParams[index];
                        index++;
                    }
                    else
                    {
                        _grid[r, c] = ' ';
                    }
                }
            }
        }

        private void BindElements()
        {
            _encryptedLabel = _root.Q<Label>("EncryptedText");
            _gridPanel = _root.Q<VisualElement>("GridPanel");
            _answerLabel = _root.Q<Label>("AnswerText");
            _submitButton = _root.Q<Button>("SubmitButton");
            _deleteButton = _root.Q<Button>("DeleteButton");

            _submitButton?.RegisterCallback<ClickEvent>(_ => OnSubmit());
            _deleteButton?.RegisterCallback<ClickEvent>(_ => OnDelete());
            _root?.RegisterCallback<KeyDownEvent>(OnKeyDown);

            if (_root != null)
            {
                _root.focusable = true;
                _root.Focus();
            }
        }

        private void SetupGrid()
        {
            if (_gridPanel == null) return;
            _gridPanel.Clear();

            Color purple = new Color(150 / 255f, 100 / 255f, 200 / 255f);

            for (int r = 0; r < 6; r++)
            {
                var rowElement = new VisualElement();
                rowElement.style.flexDirection = FlexDirection.Row;
                rowElement.style.justifyContent = Justify.Center;
                rowElement.style.marginBottom = 2;

                var rowNum = new Label((r + 1).ToString());
                rowNum.style.width = 30;
                rowNum.style.fontSize = 20;
                rowNum.style.unityFontStyleAndWeight = FontStyle.Bold;
                rowNum.style.alignSelf = Align.Center;
                rowNum.style.unityTextAlign = TextAnchor.MiddleRight;
                rowNum.style.marginRight = 10;
                rowNum.style.color = purple;
                rowElement.Add(rowNum);

                for (int c = 0; c < 6; c++)
                {
                    char letter = _grid[r, c];
                    var btn = CreateGridButton(letter);
                    rowElement.Add(btn);
                }
                
                _gridPanel.Add(rowElement);
            }
            
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.justifyContent = Justify.Center;
            headerRow.style.marginBottom = 10;
            
            var corner = new VisualElement();
            corner.style.width = 40; 
            headerRow.Add(corner);

            for (int c = 0; c < 6; c++)
            {
                var colNum = new Label((c + 1).ToString());
                colNum.style.width = 50;
                colNum.style.fontSize = 20;
                colNum.style.unityFontStyleAndWeight = FontStyle.Bold;
                colNum.style.unityTextAlign = TextAnchor.MiddleCenter;
                colNum.style.color = purple;
                headerRow.Add(colNum);
            }
            
            _gridPanel.Insert(0, headerRow);
        }

        private Button CreateGridButton(char letter)
        {
            var btn = new Button();
            btn.text = letter.ToString();
            btn.focusable = false;
            
            btn.style.width = 46;
            btn.style.height = 46;
            btn.style.fontSize = 24;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            btn.style.color = new Color(97 / 255f, 223 / 255f, 235 / 255f); // Cyan
            btn.style.borderTopWidth = 1;
            btn.style.borderBottomWidth = 1;
            btn.style.borderLeftWidth = 1;
            btn.style.borderRightWidth = 1;
            btn.style.borderTopColor = new Color(97 / 255f, 223 / 255f, 235 / 255f, 0.5f);
            btn.style.borderBottomColor = new Color(97 / 255f, 223 / 255f, 235 / 255f, 0.5f);
            btn.style.borderLeftColor = new Color(97 / 255f, 223 / 255f, 235 / 255f, 0.5f);
            btn.style.borderRightColor = new Color(97 / 255f, 223 / 255f, 235 / 255f, 0.5f);
            btn.style.marginLeft = 2;
            btn.style.marginRight = 2;
            btn.style.marginTop = 2;
            btn.style.marginBottom = 2;
            btn.style.borderTopLeftRadius = 5;
            btn.style.borderTopRightRadius = 5;
            btn.style.borderBottomLeftRadius = 5;
            btn.style.borderBottomRightRadius = 5;

            btn.RegisterCallback<MouseEnterEvent>(_ => 
            {
                btn.style.backgroundColor = new Color(97 / 255f, 223 / 255f, 235 / 255f, 0.8f);
                btn.style.color = Color.black;
            });
            btn.RegisterCallback<MouseLeaveEvent>(_ => 
            {
                btn.style.backgroundColor = new Color(0, 0, 0, 0.5f);
                btn.style.color = new Color(97 / 255f, 223 / 255f, 235 / 255f);
            });

            return btn;
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
            if (!_allowedInput.Contains(input)) return;

            OnLetterSelected(input);
            evt.StopPropagation();
        }

        private void OnLetterSelected(char letter)
        {
            _currentAnswer += letter;
            UpdateAnswerDisplay();
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
            if (_currentAnswer.Trim() == _targetDecryptedText)
            {
                Debug.Log("Polybius solved!");
                _onSuccess?.Invoke();
            }
            else
            {
                Debug.Log($"Incorrect. Expected: {_targetDecryptedText}, Got: {_currentAnswer}");
                _answerLabel.text = "НЕВЕРНО";
                _root.schedule.Execute(() => UpdateAnswerDisplay()).StartingIn(1000);
                _currentAnswer = "";
            }
        }

        private void UpdateAnswerDisplay()
        {
            if (_answerLabel != null)
                _answerLabel.text = _currentAnswer;
        }

        private void BuildAllowedInputSet()
        {
            _allowedInput.Clear();
            _allowedInput.Add(' ');

            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 6; c++)
                {
                    char cell = _grid[r, c];
                    if (cell != ' ')
                        _allowedInput.Add(cell);
                }
            }
        }
    }
}

