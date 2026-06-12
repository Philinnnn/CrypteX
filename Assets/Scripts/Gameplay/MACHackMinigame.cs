using System;
using UnityEngine.UIElements;
using UnityEngine;

namespace Gameplay
{
    public class MACHackMinigame
    {
        private Action _onSuccess;
        private Button _infoBtn;
        private VisualElement _cheatSheetOverlay;
        private Button _closeInfoBtn;
        private Label _messageLabel;
        private TextField _keyInput;
        private Label _currentMACLabel;
        private Label _targetMACLabel;
        private Label _matchPercentLabel;
        private Button _submitButton;
        private string _targetMAC = "A3F8B2C4";
        private string _secretKey = "84926";

        public MACHackMinigame(VisualElement root, string encryptedData, string decryptedData, string keyData, Action onSuccess)

        {
            _onSuccess = onSuccess;
            if (!string.IsNullOrEmpty(keyData))
            {
                _secretKey = keyData; 
            }
            if (!string.IsNullOrEmpty(encryptedData))
            {
                _targetMAC = encryptedData;
            }
            _infoBtn = root.Q<Button>("InfoButton");
            _cheatSheetOverlay = root.Q<VisualElement>("CheatSheetOverlay");
            _closeInfoBtn = root.Q<Button>("CloseInfoButton");
            _messageLabel = root.Q<Label>("MessageLabel");
            _keyInput = root.Q<TextField>("KeyInput");
            _currentMACLabel = root.Q<Label>("CurrentMACLabel");
            _targetMACLabel = root.Q<Label>("TargetMACLabel");
            _matchPercentLabel = root.Q<Label>("MatchPercentLabel");
            _submitButton = root.Q<Button>("SubmitButton");
            _infoBtn.clicked += () => _cheatSheetOverlay.style.display = DisplayStyle.Flex;
            _closeInfoBtn.clicked += () => _cheatSheetOverlay.style.display = DisplayStyle.None;
            _messageLabel.text = decryptedData;
            _targetMACLabel.text = _targetMAC;
            _currentMACLabel.text = "00000000";
            _keyInput.RegisterValueChangedCallback(evt => OnKeyChanged(evt.newValue));
            _keyInput.maxLength = _secretKey.Length;
            _submitButton.clicked += CheckWinCondition;
            UpdateMAC("");
        }
        private void OnKeyChanged(string newKey)
        {
            UpdateMAC(newKey);
        }
        private void UpdateMAC(string currentKey)
        {
            if (string.IsNullOrEmpty(currentKey) || currentKey.Length < _secretKey.Length)
            {
                _currentMACLabel.text = "--------";
                _currentMACLabel.style.color = new StyleColor(new Color(235/255f, 97/255f, 97/255f));
                _matchPercentLabel.text = "ОЖИДАНИЕ КЛЮЧА...";
                _matchPercentLabel.style.color = new StyleColor(Color.yellow);
                return;
            }

            int bulls = 0;
            int cows = 0;
            
            bool[] secretUsed = new bool[_secretKey.Length];
            bool[] guessUsed = new bool[currentKey.Length];
            
            for (int i = 0; i < _secretKey.Length; i++)
            {
                if (currentKey[i] == _secretKey[i])
                {
                    bulls++;
                    secretUsed[i] = true;
                    guessUsed[i] = true;
                }
            }
            
            for (int i = 0; i < currentKey.Length; i++)
            {
                if (!guessUsed[i])
                {
                    for (int j = 0; j < _secretKey.Length; j++)
                    {
                        if (!secretUsed[j] && currentKey[i] == _secretKey[j])
                        {
                            cows++;
                            secretUsed[j] = true;
                            break;
                        }
                    }
                }
            }

            if (bulls == _secretKey.Length)
            {
                _currentMACLabel.text = _targetMAC;
                _currentMACLabel.style.color = new StyleColor(new Color(97/255f, 235/255f, 97/255f));
                _matchPercentLabel.text = "СИНХРОНИЗАЦИЯ ПРОЙДЕНА";
                _matchPercentLabel.style.color = new StyleColor(new Color(97/255f, 235/255f, 97/255f));
            }
            else
            {
                string simulatedHex = CalculateFakeHash(currentKey, _targetMAC);
                _currentMACLabel.text = simulatedHex;
                _currentMACLabel.style.color = new StyleColor(new Color(235/255f, 97/255f, 97/255f));
                _matchPercentLabel.text = $"ТОЧНО: {bulls} | СМЕЩЕНО: {cows}";
                _matchPercentLabel.style.color = new StyleColor(Color.yellow);
            }
        }
        
        private string CalculateFakeHash(string input, string target)
        {
            if (string.IsNullOrEmpty(input)) input = "0";
            char[] result = target.ToCharArray();
            int seed = 0;
            foreach (char c in input) seed += c;
            System.Random rnd = new System.Random(seed);
            for (int i = 0; i < result.Length; i++)
            {
                bool shouldMutate = rnd.NextDouble() > (GetMatchPercentage(input) / 100f);
                if (shouldMutate)
                {
                    result[i] = "0123456789ABCDEF"[rnd.Next(16)];
                }
            }
            return new string(result);
        }
        private float GetMatchPercentage(string currentKey)
        {
            int matchCount = 0;
            for (int i = 0; i < _secretKey.Length; i++)
            {
                if (i < currentKey.Length && currentKey[i] == _secretKey[i])
                {
                    matchCount++;
                }
            }
            return (float)matchCount / _secretKey.Length * 100f;
        }

        private void CheckWinCondition()
        {
            if (_currentMACLabel.text == _targetMAC)
            {
                _onSuccess?.Invoke();
            }
        }
    }
}
