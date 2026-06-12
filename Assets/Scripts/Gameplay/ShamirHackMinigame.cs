using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace Gameplay
{
    public class ShamirHackMinigame
    {
        private readonly VisualElement _root;
        private readonly Action _onSuccess;
        private readonly string _targetNormalized;
        private Label _answerLabel;
        private Label _stepTitle;
        private Label _valueLabel;
        private Label _keyLabel;
        private Label _modLabel;
        private string _currentAnswer = string.Empty;
        private int _currentStep = 1;
        public ShamirHackMinigame(VisualElement root, string encryptedText, string targetDecryptedText, string key, Action onSuccess)
        {
            _root = root;
            _onSuccess = onSuccess;
            _targetNormalized = (targetDecryptedText ?? string.Empty).Trim();
            BindElements();
            RegisterCallbacks();
            SetStep(1);
        }
        private void BindElements()
        {
            _answerLabel = _root.Q<Label>("AnswerText");
            _stepTitle = _root.Q<Label>("StepTitle");
            _valueLabel = _root.Q<Label>("ValueLabel");
            _keyLabel = _root.Q<Label>("KeyLabel");
            _modLabel = _root.Q<Label>("ModLabel");
            
            var submitButton = _root.Q<Button>("SubmitButton");
            var deleteButton = _root.Q<Button>("DeleteButton");
            var infoButton = _root.Q<Button>("InfoButton");
            var closeInfoButton = _root.Q<Button>("CloseInfoButton");
            submitButton?.RegisterCallback<ClickEvent>(_ => OnSubmit());
            deleteButton?.RegisterCallback<ClickEvent>(_ => OnDelete());
            infoButton?.RegisterCallback<ClickEvent>(_ => ToggleCheatSheet(true));
            closeInfoButton?.RegisterCallback<ClickEvent>(_ => ToggleCheatSheet(false));
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
                _root.Focus();
        }
        private void SetStep(int step)
        {
            _currentStep = step;
            _currentAnswer = "";
            UpdateAnswerDisplay();
            
            if (_stepTitle == null) return;
            
            _stepTitle.text = $"ЭТАП {step}";
            _modLabel.text = "11";

            switch (step)
            {
                case 1:
                    _valueLabel.text = "5";
                    _keyLabel.text = "3";
                    break;
                case 2:
                    _valueLabel.text = "4";
                    _keyLabel.text = "7";
                    break;
                case 3:
                    _valueLabel.text = "5";
                    _keyLabel.text = "7";
                    break;
                case 4:
                    _valueLabel.text = "3";
                    _keyLabel.text = "3";
                    break;
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
            char c = evt.character;
            if (char.IsDigit(c))
            {
                if (_currentAnswer.Length < 10)
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
            string expected = "";
            switch (_currentStep)
            {
                case 1: expected = "4"; break;
                case 2: expected = "5"; break;
                case 3: expected = "3"; break;
                case 4: expected = "5"; break;
            }
            if (_currentAnswer == expected)
            {
                if (_currentStep == 4)
                {
                    _onSuccess?.Invoke();
                }
                else
                {
                    SetStep(_currentStep + 1);
                }
            }
            else
            {
                if (_answerLabel != null)
                    _answerLabel.text = "ОШИБКА";
                _root?.schedule.Execute(() =>
                {
                    _currentAnswer = "";
                    UpdateAnswerDisplay();
                }).StartingIn(900);
            }
        }
        private void UpdateAnswerDisplay()
        {
            if (_answerLabel != null)
                _answerLabel.text = _currentAnswer;
        }
    }
}
