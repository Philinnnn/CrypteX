using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Managers;

namespace Gameplay
{
    public class CaesarHackMinigame
    {
        private VisualElement _root;
        private Label _centerLabel;
        private Label _topLabel;
        private Label _bottomLabel;
        
        private string _originalEncryptedText;
        private string _targetDecryptedText;
        private Action _onSuccess;
        
        private int _currentShift = 0;
        private bool _isAnimating;

        private Button _upBtn;
        private Button _downBtn;

        public CaesarHackMinigame(VisualElement root, string encryptedText, string targetDecryptedText, Action onSuccess)
        {
            _root = root;
            _originalEncryptedText = encryptedText;
            _targetDecryptedText = targetDecryptedText;
            _onSuccess = onSuccess;

            BindElements();
            UpdateTextImmediate();
        }

        private void BindElements()
        {
            _centerLabel = _root.Q<Label>("EncryptedText");
            _topLabel = _root.Q<Label>("RightUpText");
            _bottomLabel = _root.Q<Label>("LeftDownText");

            _upBtn = _root.Q<Button>("RightUpButton");
            _downBtn = _root.Q<Button>("LeftDownButton");
            
            _upBtn?.RegisterCallback<ClickEvent>(evt => OnShiftInput(1));
            _downBtn?.RegisterCallback<ClickEvent>(evt => OnShiftInput(-1));
        }

        public void OnShiftInput(int direction)
        {
            if (_isAnimating) return;
            GameManager.Instance.StartCoroutine(AnimateShift(direction));
        }

        private void UpdateTextImmediate()
        {
            if (_centerLabel == null) return;
            _centerLabel.text = ApplyShift(_originalEncryptedText, _currentShift);
            _topLabel.text = ApplyShift(_originalEncryptedText, _currentShift + 1);
            _bottomLabel.text = ApplyShift(_originalEncryptedText, _currentShift - 1);
        }

        private IEnumerator AnimateShift(int direction)
        {
            _isAnimating = true;

            float distance = _centerLabel.resolvedStyle.height; 
            if (distance <= 1f) distance = 100f;

            float duration = 0.5f;
            float elapsed = 0f;

            int visualDir = direction;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                
                float currentY = Mathf.Lerp(0, visualDir * distance, smoothT);
                
                SetTranslate(_centerLabel, currentY);
                SetTranslate(_topLabel, currentY);
                SetTranslate(_bottomLabel, currentY);
                
                yield return null;
            }

            _currentShift += direction;
            
            SetTranslate(_centerLabel, 0);
            SetTranslate(_topLabel, 0);
            SetTranslate(_bottomLabel, 0);
            
            UpdateTextImmediate();

            _isAnimating = false;
            CheckWin();
        }

        private void SetTranslate(VisualElement e, float y)
        {
            if (e == null) return;
            e.style.translate = new Translate(0, y, 0);
        }

        private string ApplyShift(string text, int shiftAmount)
        {
            const string Alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
            char[] buffer = text.ToCharArray();
            int n = Alphabet.Length;

            for (int i = 0; i < buffer.Length; i++)
            {
                char c = buffer[i];
                int index = Alphabet.IndexOf(char.ToUpper(c));

                if (index != -1)
                {
                    int newIndex = (index + shiftAmount) % n;
                    if (newIndex < 0) newIndex += n;

                    char newChar = Alphabet[newIndex];
                    buffer[i] = char.IsLower(c) ? char.ToLower(newChar) : newChar;
                }
                else if (char.IsLetter(c) && ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                {
                    char d = char.IsUpper(c) ? 'A' : 'a';
                    buffer[i] = (char)((((c + shiftAmount) - d) % 26 + 26) % 26 + d);
                }
            }
            return new string(buffer);
        }

        private void CheckWin()
        {
            if (_centerLabel.text == _targetDecryptedText)
            {
                if (_upBtn != null) _upBtn.style.display = DisplayStyle.None;
                if (_downBtn != null) _downBtn.style.display = DisplayStyle.None;
                
                GameManager.Instance.StartCoroutine(DelaySuccessCall());
            }
        }
        
        private IEnumerator DelaySuccessCall()
        {
            yield return new WaitForSeconds(1.5f);
            _onSuccess?.Invoke();
        }
        
        public IEnumerator AutoSolve(string targetText)
        {
            int targetShift = -5;

            int currentShift = _currentShift;
            int diff = targetShift - currentShift;
            int length = 33;

            int forwardSteps = (diff + length) % length;
            int backwardSteps = (length - forwardSteps) % length;

            int direction = 1;
            int steps = forwardSteps;

            if (backwardSteps < forwardSteps && backwardSteps > 0)
            {
                direction = -1;
                steps = backwardSteps;
            }

            Debug.Log($"AutoSolve: Current={currentShift}, Target={targetShift}, Steps={steps}, Dir={direction}");

            for (int i = 0; i < steps; i++)
            {
                if (_centerLabel.text == targetText) break;
                yield return AnimateShift(direction);
                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}
