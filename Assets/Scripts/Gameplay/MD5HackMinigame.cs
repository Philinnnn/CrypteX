using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Gameplay
{
    public class MD5HackMinigame
    {
        private readonly VisualElement _root;
        private readonly Action _onSuccess;
        private Label _targetHashLabel;
        private Label _currentHashLabel;
        private Label _dictionaryLabel;
        private TextField _inputField;
        private string _targetText;
        private string _targetHash;
        private readonly string[] _passwordVariants = new string[]
        {
            "pX9#vL2!mR8*Q",
            "7tQn_W42$zK!b",
            "G#1sP&9kL@m0N",
            "Bf5^tX8*Zq2_V",
            "3kH!nL9#pS7&W",
            "rZ4@mG6*tX1$Y"
        };
        public MD5HackMinigame(VisualElement root, string encryptedText, string targetDecryptedText, string key, Action onSuccess)
        {
            _root = root;
            _onSuccess = onSuccess;
            System.Random rnd = new System.Random();
            _targetText = _passwordVariants[rnd.Next(_passwordVariants.Length)];
            
            _targetHash = CalculateHash(_targetText);
            BindElements();
            RegisterCallbacks();
            
            if (_inputField != null)
                _inputField.value = "";

            if (_dictionaryLabel != null)
            {
                var shuffled = _passwordVariants.OrderBy(x => rnd.Next()).ToArray();
                _dictionaryLabel.text = "ВОЗМОЖНЫЕ ВАРИАНТЫ:\n" + string.Join("  |  ", shuffled);
            }
            UpdateHashDisplay();
        }
        private void BindElements()
        {
            _targetHashLabel = _root.Q<Label>("TargetHashLabel");
            _currentHashLabel = _root.Q<Label>("CurrentHashLabel");
            _inputField = _root.Q<TextField>("InputField");
            _dictionaryLabel = _root.Q<Label>("DictionaryLabel");
            var infoButton = _root.Q<Button>("InfoButton");
            var closeInfoButton = _root.Q<Button>("CloseInfoButton");
            var submitButton = _root.Q<Button>("SubmitButton");
            infoButton?.RegisterCallback<ClickEvent>(evt => ToggleCheatSheet(true));
            closeInfoButton?.RegisterCallback<ClickEvent>(evt => ToggleCheatSheet(false));
            submitButton?.RegisterCallback<ClickEvent>(evt => OnSubmit());
            _inputField?.RegisterValueChangedCallback(evt => OnInputValueChanged(evt.newValue));
        }
        private void RegisterCallbacks()
        {
            if (_root != null)
            {
                _root.focusable = true;
                _root.Focus();
                if (_inputField != null)
                    _inputField.Focus();
            }
            ToggleCheatSheet(false);
        }
        private void ToggleCheatSheet(bool show)
        {
            var overlay = _root.Q<VisualElement>("CheatSheetOverlay");
            if (overlay != null)
                overlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show && _inputField != null)
            {
                _inputField.Focus();
            }
        }
        private void OnInputValueChanged(string newValue)
        {
            UpdateHashDisplay();
        }
        private void UpdateHashDisplay()
        {
            if (_currentHashLabel != null && _inputField != null)
            {
                string input = _inputField.value;
                string currentHash = CalculateHash(input);
                _currentHashLabel.text = currentHash;
                if (currentHash == _targetHash)
                {
                    _currentHashLabel.style.color = new StyleColor(new Color(0.4f, 0.9f, 0.4f)); // Green for match
                }
                else
                {
                    _currentHashLabel.style.color = new StyleColor(new Color(0.9f, 0.4f, 0.4f)); // Red for mismatch
                }
            }
            if (_targetHashLabel != null)
            {
                _targetHashLabel.text = _targetHash;
            }
        }
        private void OnSubmit()
        {
            if (_inputField != null && _inputField.value == _targetText)
            {
                _onSuccess?.Invoke();
            }
            else
            {
                if (_currentHashLabel != null)
                {
                    string oldText = _currentHashLabel.text;
                    _currentHashLabel.text = "ОШИБКА";
                    _root?.schedule.Execute(() =>
                    {
                        UpdateHashDisplay();
                    }).StartingIn(900);
                }
            }
        }
        private string CalculateHash(string input)
        {
            if (string.IsNullOrEmpty(input)) return new string('0', 16);
            // Simple deterministic avalanche hash simulation
            uint hash = 0x811C9DC5;
            foreach (char c in input)
            {
                hash ^= c;
                hash *= 0x01000193; // FNV Prime
            }
            // Combine with inverted to make it look like a 16-character pseudo-MD5 hex string
            return hash.ToString("X8") + (~hash).ToString("X8");
        }
    }
}
