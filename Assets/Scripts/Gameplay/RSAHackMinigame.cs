using System;
using UnityEngine.UIElements;
using UnityEngine;
namespace Gameplay
{
    public class RSAHackMinigame
    {
        private Action _onSuccess;
        private Button _infoBtn;
        private VisualElement _cheatSheetOverlay;
        private Button _closeInfoBtn;
        private Label _publicLabel;
        private Label _encryptedTextLabel;
        private TextField _inputP;
        private TextField _inputQ;
        private TextField _inputPhi;
        private TextField _inputD;
        private Label _statusLabel;
        private Button _submitButton;
        private int _valN;
        private int _valE;
        private int _targetD;
        private string _targetDecryptedText = "";
        private string _encryptedHexDisplay = "4F 21 8A";

        public RSAHackMinigame(VisualElement root, string encryptedData, string decryptedData, string keyData, Action onSuccess)
        {
            _onSuccess = onSuccess;
            if (!string.IsNullOrEmpty(decryptedData)) _targetDecryptedText = decryptedData;
            if (!string.IsNullOrEmpty(encryptedData))
            {
                try {
                    string[] parts = encryptedData.Split(',');
                    _valN = int.Parse(parts[0].Split('=')[1]);
                    _valE = int.Parse(parts[1].Split('=')[1]);
                } catch { }
            }
            if (!string.IsNullOrEmpty(keyData))
            {
                int.TryParse(keyData, out _targetD);
            }
            _infoBtn = root.Q<Button>("InfoButton");
            _cheatSheetOverlay = root.Q<VisualElement>("CheatSheetOverlay");
            _closeInfoBtn = root.Q<Button>("CloseInfoButton");
            _publicLabel = root.Q<Label>("PublicLabel");
            _encryptedTextLabel = root.Q<Label>("EncryptedTextLabel");
            _inputP = root.Q<TextField>("InputP");
            _inputQ = root.Q<TextField>("InputQ");
            _inputPhi = root.Q<TextField>("InputPhi");
            _inputD = root.Q<TextField>("InputD");
            _statusLabel = root.Q<Label>("StatusLabel");
            _submitButton = root.Q<Button>("SubmitButton");
            _infoBtn.clicked += () => _cheatSheetOverlay.style.display = DisplayStyle.Flex;
            _closeInfoBtn.clicked += () => _cheatSheetOverlay.style.display = DisplayStyle.None;
            _publicLabel.text = $"N = {_valN}     e = {_valE}";
            _encryptedTextLabel.text = _encryptedHexDisplay;
            _submitButton.clicked += CheckWinCondition;
        }
        private void CheckWinCondition()
        {
            int p = 0, q = 0, phi = 0, d = 0;
            int.TryParse(_inputP.value, out p);
            int.TryParse(_inputQ.value, out q);
            int.TryParse(_inputPhi.value, out phi);
            int.TryParse(_inputD.value, out d);
            if (p * q != _valN)
            {
                _statusLabel.text = "ОШИБКА: P * Q НЕ РАВНО N";
                _statusLabel.style.color = new StyleColor(new Color(235/255f, 97/255f, 97/255f));
                return;
            }
            // it could be p=11, q=13 or p=13, q=11
            if (phi != (p - 1) * (q - 1))
            {
                _statusLabel.text = "ОШИБКА: НEВЕРНАЯ ФУНКЦИЯ ЭЙЛЕРА Φ(N)";
                _statusLabel.style.color = new StyleColor(new Color(235/255f, 97/255f, 97/255f));
                return;
            }
            if (d == _targetD || ((d * _valE) % phi == 1 && d > 0))
            {
                _submitButton.SetEnabled(false);
                _statusLabel.text = "ПРИВАТНЫЙ КЛЮЧ ВОССТАНОВЛЕН. ДЕШИФРОВКА АРХИВА...";
                _statusLabel.style.color = new StyleColor(new Color(97/255f, 235/255f, 97/255f));
                
                int blinkCount = 0;
                _encryptedTextLabel.schedule.Execute(() =>
                {
                    if (blinkCount % 2 == 0)
                    {
                        _encryptedTextLabel.style.color = new StyleColor(Color.white);
                        _encryptedTextLabel.text = "DECRYPTING...";
                    }
                    else
                    {
                        _encryptedTextLabel.style.color = new StyleColor(new Color(235/255f, 97/255f, 97/255f));
                        _encryptedTextLabel.text = _encryptedHexDisplay;
                    }
                    
                    blinkCount++;
                    if (blinkCount > 6)
                    {
                        _encryptedTextLabel.text = _targetDecryptedText;
                        _encryptedTextLabel.style.color = new StyleColor(new Color(97/255f, 235/255f, 97/255f));
                        _encryptedTextLabel.schedule.Execute(() => _onSuccess?.Invoke()).StartingIn(1500);
                    }
                }).StartingIn(200).Every(200).Until(() => blinkCount > 6);
            }
            else
            {
                _statusLabel.text = "ОШИБКА: НЕВЕРНЫЙ ПРИВАТНЫЙ КЛЮЧ D";
                _statusLabel.style.color = new StyleColor(new Color(235/255f, 97/255f, 97/255f));
            }
        }
    }
}
