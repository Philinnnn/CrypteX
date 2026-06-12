using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay
{
    public class XorHackMinigame
    {
        private readonly VisualElement _root;
        private readonly Action _onSuccess;
        
        private Label _answerLabel;
        private Label _dataLabel;
        private Label _keyLabel;
        private Label _stepTitleLabel;
        
        private string _currentAnswer = string.Empty;
        private int _currentStep = 1;
        private string _targetNormalized = string.Empty;
        
        private TextField _decInput;
        private TextField _binInput;
        private Label _breakdownLabel;

        public XorHackMinigame(VisualElement root, string _, string __, string ___, Action onSuccess)
        {
            _root = root;
            _onSuccess = onSuccess;
            
            BindElements();
            RegisterCallbacks();
            SetStep(1);
        }

        private void BindElements()
        {
            _answerLabel = _root.Q<Label>("AnswerText");
            _dataLabel = _root.Q<Label>("DataLabel");
            _keyLabel = _root.Q<Label>("KeyLabel");
            _stepTitleLabel = _root.Q<Label>("StepTitle");
            
            var submitButton = _root.Q<Button>("SubmitButton");
            var deleteButton = _root.Q<Button>("DeleteButton");
            var infoButton = _root.Q<Button>("InfoButton");
            var closeInfoButton = _root.Q<Button>("CloseInfoButton");

            _decInput = _root.Q<TextField>("DecInput");
            _binInput = _root.Q<TextField>("BinInput");
            _breakdownLabel = _root.Q<Label>("ConversionBreakdown");
            var convertToBinButton = _root.Q<Button>("ConvertToBinButton");
            var convertToDecButton = _root.Q<Button>("ConvertToDecButton");

            submitButton?.RegisterCallback<ClickEvent>(evt => OnSubmit());
            deleteButton?.RegisterCallback<ClickEvent>(evt => OnDelete());
            infoButton?.RegisterCallback<ClickEvent>(evt => ToggleCheatSheet(true));
            closeInfoButton?.RegisterCallback<ClickEvent>(evt => ToggleCheatSheet(false));

            convertToBinButton?.RegisterCallback<ClickEvent>(evt => ConvertToBinary());
            convertToDecButton?.RegisterCallback<ClickEvent>(evt => ConvertToDecimal());
        }

        private void ConvertToBinary()
        {
            if (_decInput == null || string.IsNullOrWhiteSpace(_decInput.value)) return;
            string input = _decInput.value.Trim();
            
            int number;
            if (int.TryParse(input, out number))
            {
                if (number >= 0 && number <= 255)
                {
                    string bin = Convert.ToString(number, 2).PadLeft(8, '0');
                    if (_binInput != null) _binInput.value = bin;
                    GenerateBreakdown(number, bin, input);
                    return;
                }
            }
            if (input.Length == 1)
            {
                char c = input[0];
                number = (int)c;
                if (number >= 0 && number <= 255)
                {
                    string bin = Convert.ToString(number, 2).PadLeft(8, '0');
                    if (_binInput != null) _binInput.value = bin;
                    GenerateBreakdown(number, bin, $"Символ '{c}' (ASCII: {number})");
                    return;
                }
            }

            if (_breakdownLabel != null) _breakdownLabel.text = "Ошибка: Введите число от 0 до 255 или один символ.";
        }

        private void ConvertToDecimal()
        {
            if (_binInput == null || string.IsNullOrWhiteSpace(_binInput.value)) return;
            string bin = _binInput.value.Trim();
            
            if (bin.Length > 8)
            {
                 if (_breakdownLabel != null) _breakdownLabel.text = "Ошибка: Максимум 8 бит.";
                 return;
            }

            try
            {
                int number = Convert.ToInt32(bin, 2);
                string paddedBin = bin.PadLeft(8, '0');
                
                string resultStr = number.ToString();
                if (number >= 32 && number <= 126)
                {
                    resultStr = $"Символ: {(char)number} (Дес: {number})";
                }
                
                if (_decInput != null) _decInput.value = resultStr;
                GenerateBreakdown(number, paddedBin, resultStr);
            }
            catch
            {
                if (_breakdownLabel != null) _breakdownLabel.text = "Ошибка: Введите двоичное число (только 0 и 1).";
            }
        }

        private void GenerateBreakdown(int number, string bin, string header)
        {
            if (_breakdownLabel == null) return;
            
            int[] powers = { 128, 64, 32, 16, 8, 4, 2, 1 };
            string breakdown = $"{header} = ";
            bool first = true;
            
            for (int i = 0; i < 8; i++)
            {
                if (bin[i] == '1')
                {
                    if (!first) breakdown += " + ";
                    breakdown += powers[i].ToString();
                    first = false;
                }
            }
            if (first) breakdown += "0";

            breakdown += $"\n{bin} (128, 64, 32, 16, 8, 4, 2, 1)";
            _breakdownLabel.text = breakdown;
        }

        private void RegisterCallbacks()
        {
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
            var overlay = _root.Q<VisualElement>("CheatSheetOverlay");
            if (overlay != null)
                overlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show && _root != null)
            {
                _root.Focus();
                if (_decInput != null) _decInput.value = "";
                if (_binInput != null) _binInput.value = "";
                if (_breakdownLabel != null) _breakdownLabel.text = "Введите число, символ или 8-битный код...";
            }
        }

        private void SetStep(int step)
        {
            _currentStep = step;
            _currentAnswer = string.Empty;

            switch (step)
            {
                case 1:
                    if (_stepTitleLabel != null) _stepTitleLabel.text = "ЭТАП 1 / 3: ДВОИЧНЫЕ ДАННЫЕ";
                    if (_dataLabel != null) _dataLabel.text = "10101010";
                    if (_keyLabel != null) _keyLabel.text = "01011010";
                    _targetNormalized = "11110000";
                    break;
                case 2:
                    if (_stepTitleLabel != null) _stepTitleLabel.text = "ЭТАП 2 / 3: ДЕСЯТИЧНЫЕ ДАННЫЕ";
                    if (_dataLabel != null) _dataLabel.text = "42";
                    if (_keyLabel != null) _keyLabel.text = "15";
                    _targetNormalized = "37";
                    break;
                case 3:
                    if (_stepTitleLabel != null) _stepTitleLabel.text = "ЭТАП 3 / 3: ASCII ТЕКСТ";
                    if (_dataLabel != null) _dataLabel.text = "Z";
                    if (_keyLabel != null) _keyLabel.text = ")";
                    _targetNormalized = "S";
                    break;
            }
            UpdateAnswerDisplay();
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

            char c = evt.character;
            
            bool valid = false;
            if (_currentStep == 1 && (c == '0' || c == '1')) valid = true;
            else if (_currentStep == 2 && char.IsDigit(c)) valid = true;
            else if (_currentStep == 3 && char.IsLetterOrDigit(c)) valid = true;
            
            if (valid)
            {
                if (_currentStep == 3) c = char.ToUpper(c);
                
                if (_currentAnswer.Length < _targetNormalized.Length)
                {
                    _currentAnswer += c;
                    UpdateAnswerDisplay();
                }
                evt.StopPropagation();
            }
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
            if (_currentAnswer == _targetNormalized)
            {
                if (_currentStep < 3)
                {
                    SetStep(_currentStep + 1);
                }
                else
                {
                    _onSuccess?.Invoke();
                }
            }
            else
            {
                if (_answerLabel != null)
                    _answerLabel.text = "ОШИБКА";

                _root?.schedule.Execute(() =>
                {
                    _currentAnswer = string.Empty;
                    UpdateAnswerDisplay();
                }).StartingIn(900);
            }
        }

        private void UpdateAnswerDisplay()
        {
            if (_answerLabel != null)
            {
                _answerLabel.text = _currentAnswer.PadRight(_targetNormalized.Length, '_');
            }
        }
    }
}
