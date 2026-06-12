using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Gameplay
{
    public class SDESHackMinigame
    {
        private readonly VisualElement _root;
        private readonly Action _onSuccess;
        private Label _answerLabel;
        private Label _dataLabel;
        private Label _operationLabel;
        private Label _stepTitleLabel;
        private string _currentAnswer = string.Empty;
        private int _currentStep = 1;
        private string _targetNormalized = string.Empty;
        public SDESHackMinigame(VisualElement root, string _, string __, string ___, Action onSuccess)
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
            _operationLabel = _root.Q<Label>("OperationLabel");
            _stepTitleLabel = _root.Q<Label>("StepTitle");
            var submitButton = _root.Q<Button>("SubmitButton");
            var deleteButton = _root.Q<Button>("DeleteButton");
            var infoButton = _root.Q<Button>("InfoButton");
            var closeInfoButton = _root.Q<Button>("CloseInfoButton");
            submitButton?.RegisterCallback<ClickEvent>(evt => OnSubmit());
            deleteButton?.RegisterCallback<ClickEvent>(evt => OnDelete());
            infoButton?.RegisterCallback<ClickEvent>(evt => ToggleCheatSheet(true));
            closeInfoButton?.RegisterCallback<ClickEvent>(evt => ToggleCheatSheet(false));
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
            }
        }
        private void SetStep(int step)
        {
            _currentStep = step;
            _currentAnswer = string.Empty;
            switch (step)
            {
                case 1:
                    if (_stepTitleLabel != null) _stepTitleLabel.text = "ЭТАП 1 / 4: КЛЮЧ P10";
                    if (_dataLabel != null) _dataLabel.text = "1010000010";
                    if (_operationLabel != null) _operationLabel.text = "P10";
                    _targetNormalized = "1000001100";
                    break;
                case 2:
                    if (_stepTitleLabel != null) _stepTitleLabel.text = "ЭТАП 2 / 4: ДАННЫЕ IP";
                    if (_dataLabel != null) _dataLabel.text = "01110010";
                    if (_operationLabel != null) _operationLabel.text = "IP";
                    _targetNormalized = "10101001";
                    break;
                case 3:
                    if (_stepTitleLabel != null) _stepTitleLabel.text = "ЭТАП 3 / 4: РАСШИРЕНИЕ EP";
                    if (_dataLabel != null) _dataLabel.text = "1001";
                    if (_operationLabel != null) _operationLabel.text = "EP";
                    _targetNormalized = "11000011";
                    break;
                case 4:
                    if (_stepTitleLabel != null) _stepTitleLabel.text = "ЭТАП 4 / 4: СЛОЖЕНИЕ С КЛЮЧОМ (XOR)";
                    if (_dataLabel != null) _dataLabel.text = "11000011\n(Рез. EP)";
                    if (_operationLabel != null) _operationLabel.text = "XOR\n10000011\n(Ключ P10)";
                    _targetNormalized = "01000000";
                    if (_dataLabel != null) _dataLabel.style.fontSize = 40;
                    if (_operationLabel != null) _operationLabel.style.fontSize = 40;
                    break;
            }
            if (step != 4)
            {
                if (_dataLabel != null) _dataLabel.style.fontSize = 60;
                if (_operationLabel != null) _operationLabel.style.fontSize = 60;
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
            if (c == '0' || c == '1')
            {
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
                if (_currentStep < 4)
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
                string display = _currentAnswer.PadRight(_targetNormalized.Length, '_');
                _answerLabel.text = display;
            }
        }
    }
}
