using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Managers;

namespace Gameplay
{
    public class MorseCodeHackMinigame
    {
        private const string KeyboardAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        private VisualElement _root;
        private VisualElement _lampElement;
        private Label _signalLabel;
        private Label _answerLabel;
        private VisualElement _cheatSheetOverlay;
        private VisualElement _cheatSheetTable;
        
        private string _targetDecryptedText;
        private string _currentAnswer = "";
        private Action _onSuccess;

        // Morse Code Data
        private readonly Dictionary<char, string> _morseCode = new Dictionary<char, string>
        {
            {'А', ".-"}, {'Б', "-..."}, {'В', ".--"}, {'Г', "--."}, {'Д', "-.."}, 
            {'Е', "."}, {'Ё', "."}, {'Ж', "...-"}, {'З', "--.."}, {'И', ".."}, 
            {'Й', ".---"}, {'К', "-.-"}, {'Л', ".-.."}, {'М', "--"}, {'Н', "-."}, 
            {'O', "---"}, {'П', ".--."}, {'Р', ".-."}, {'С', "..."}, {'Т', "-"}, 
            {'У', "..-"}, {'Ф', "..-."}, {'Х', "...."}, {'Ц', "-.-."}, {'Ч', "---."}, 
            {'Ш', "----"}, {'Щ', "--.-"}, {'Ъ', "--.--"}, {'Ы', "-.--"}, {'Ь', "-..-"}, 
            {'Э', "..-.."}, {'Ю', "..--"}, {'Я', ".-.-"}
        };

        private List<string> _sequence = new List<string>();
        private bool _isPlayingSequence = false;
        private Coroutine _sequenceCoroutine;
        
        
        private Sprite[] _lampSprites;

        public MorseCodeHackMinigame(VisualElement root, string targetDecryptedText, Action onSuccess)
        {
            _root = root;
            _targetDecryptedText = targetDecryptedText.ToUpper();
            _onSuccess = onSuccess;
            
            if (GameManager.Instance != null && GameManager.Instance.LevelManager != null)
            {
                _lampSprites = GameManager.Instance.LevelManager.GetMorseLampSprites();
            }

            BindElements();
            SetupCheatSheet();
            HideVirtualKeyboard();
            PrepareSequence();
            
            StartSequence();
            
            _root.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (_sequenceCoroutine != null)
            {
                GameManager.Instance.StopCoroutine(_sequenceCoroutine);
                _sequenceCoroutine = null;
            }
            StopAudio();
        }

        private void BindElements()
        {
            _lampElement = _root.Q<VisualElement>("LampImage");
            _signalLabel = _root.Q<Label>("SignalLabel");
            _answerLabel = _root.Q<Label>("AnswerText");
            _cheatSheetOverlay = _root.Q<VisualElement>("CheatSheetOverlay");
            _cheatSheetTable = _root.Q<VisualElement>("CheatSheetTable");

            Debug.Log($"BindElements: InfoButton found? {_root.Q<Button>("InfoButton") != null}");
            
            _root.Q<Button>("InfoButton")?.RegisterCallback<ClickEvent>(_ => ToggleCheatSheet(true));
            _root.Q<Button>("CloseInfoButton")?.RegisterCallback<ClickEvent>(_ => ToggleCheatSheet(false));

            _root.Q<Button>("DeleteButton")?.RegisterCallback<ClickEvent>(_ => OnDelete());
            _root.Q<Button>("SubmitButton")?.RegisterCallback<ClickEvent>(_ => OnSubmit());
            _root.Q<Button>("ReplayButton")?.RegisterCallback<ClickEvent>(_ => StartSequence());
            _root?.RegisterCallback<KeyDownEvent>(OnKeyDown);

            if (_root != null)
            {
                _root.focusable = true;
                _root.Focus();
            }
        }

        private void SetupCheatSheet()
        {
            if (_cheatSheetTable == null) return;
            
            _cheatSheetTable.Clear();
            
            foreach (var kvp in _morseCode)
            {
                var cell = new VisualElement();
                cell.style.width = 110;
                cell.style.height = 80;
                cell.style.marginRight = 10;
                cell.style.marginLeft = 10;
                cell.style.marginBottom = 10;
                cell.style.paddingTop = 10;
                cell.style.paddingBottom = 10;
                cell.style.backgroundColor = new Color(1f, 1f, 1f, 0.05f);
                cell.style.borderTopLeftRadius = 8;
                cell.style.borderTopRightRadius = 8;
                cell.style.borderBottomLeftRadius = 8;
                cell.style.borderBottomRightRadius = 8;
                cell.style.alignItems = Align.Center;
                cell.style.justifyContent = Justify.Center;

                var letterLabel = new Label(kvp.Key.ToString());
                letterLabel.style.fontSize = 28;
                letterLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                letterLabel.style.color = new Color(97/255f, 223/255f, 235/255f);

                var codeLabel = new Label(kvp.Value);
                codeLabel.style.fontSize = 20;
                codeLabel.style.color = Color.white;
                codeLabel.style.marginTop = 5;

                cell.Add(letterLabel);
                cell.Add(codeLabel);
                
                _cheatSheetTable.Add(cell);
            }
        }

        private void ToggleCheatSheet(bool show)
        {
            if (_cheatSheetOverlay != null)
                _cheatSheetOverlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }


        private void HideVirtualKeyboard()
        {
            var container = _root.Q<VisualElement>("KeyboardContainer");
            if (container == null) return;

            container.style.display = DisplayStyle.None;
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
                OnLetterInput(' ');
                evt.StopPropagation();
                return;
            }

            if (evt.character == '\0' || char.IsControl(evt.character)) return;

            char input = char.ToUpper(evt.character);
            if (KeyboardAlphabet.IndexOf(input) < 0) return;

            OnLetterInput(input);
            evt.StopPropagation();
        }

        private void PrepareSequence()
        {
            _sequence.Clear();
            foreach (char c in _targetDecryptedText)
            {
                if (_morseCode.ContainsKey(c))
                {
                    string code = _morseCode[c];
                    foreach (char signal in code)
                    {
                        _sequence.Add(signal.ToString());
                    }
                    _sequence.Add("LETTER_SPACE");
                }
                else if (c == ' ')
                {
                    _sequence.Add("WORD_SPACE");
                }
            }
        }

        private void StartSequence()
        {
            if (_isPlayingSequence)
            {
                if (_sequenceCoroutine != null) GameManager.Instance.StopCoroutine(_sequenceCoroutine);
            }
            _sequenceCoroutine = GameManager.Instance.StartCoroutine(PlaySequenceRoutine());
        }

        private IEnumerator PlaySequenceRoutine()
        {
            _isPlayingSequence = true;
            
            yield return new WaitForSeconds(1.0f);

            foreach (string signal in _sequence)
            {
                if (signal == ".")
                {
                    StartAudio();
                    yield return AnimateLamp(true);
                    yield return new WaitForSeconds(0.2f);
                    yield return AnimateLamp(false);
                    StopAudio();
                    
                    if (_signalLabel != null) _signalLabel.text = "";
                    yield return new WaitForSeconds(0.2f);
                }
                else if (signal == "-")
                {
                    StartAudio();
                    yield return AnimateLamp(true);
                    yield return new WaitForSeconds(0.6f);
                    yield return AnimateLamp(false);
                    StopAudio();
                    
                    if (_signalLabel != null) _signalLabel.text = "";
                    yield return new WaitForSeconds(0.2f);
                }
                else if (signal == "LETTER_SPACE")
                {
                   yield return new WaitForSeconds(0.6f); 
                }
                else if (signal == "WORD_SPACE")
                {
                    yield return new WaitForSeconds(1.2f);
                }
            }

            _isPlayingSequence = false;
             if (_signalLabel != null) _signalLabel.text = "КОНЕЦ";
             yield return new WaitForSeconds(1f);
             if (_signalLabel != null) _signalLabel.text = "";
        }
        
        private IEnumerator AnimateLamp(bool turnOn)
        {
            if (_lampSprites == null || _lampSprites.Length == 0)
            {
                SetLampState(turnOn); 
                yield break;
            }

            int start = turnOn ? 0 : _lampSprites.Length - 1;
            int end = turnOn ? _lampSprites.Length - 1 : 0;
            int step = turnOn ? 1 : -1;
            
            float frameDuration = 0.03f; 

            for (int i = start; turnOn ? i <= end : i >= end; i += step)
            {
                if (_lampElement != null)
                {
                   _lampElement.style.backgroundImage = new StyleBackground(_lampSprites[i]);
                }
                yield return new WaitForSeconds(frameDuration);
            }
        }

        private void SetLampState(bool on)
        {
            if (_lampElement == null) return;
            
            if (_lampSprites != null && _lampSprites.Length > 0)
            {
                if (on)
                {
                    _lampElement.style.backgroundImage = new StyleBackground(_lampSprites[_lampSprites.Length - 1]);
                }
                else
                {
                    _lampElement.style.backgroundImage = new StyleBackground(_lampSprites[0]);
                }
                
                _lampElement.style.backgroundColor = Color.clear;
            }
            else
            {
                _lampElement.style.backgroundColor = on ? new Color(1f, 1f, 0.8f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f); 
            }
        }

        private void OnLetterInput(char letter)
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
            Debug.Log($"Check Answer: {_currentAnswer} vs {_targetDecryptedText}");
            
            string answer = _currentAnswer.Trim().ToUpper();
            string target = _targetDecryptedText.Trim().ToUpper();
            
            if (answer == target)
            {
                if (_sequenceCoroutine != null) GameManager.Instance.StopCoroutine(_sequenceCoroutine);
                GameManager.Instance.AudioManager?.StopTone();
                _onSuccess?.Invoke();
            }
            else
            {
                _currentAnswer = "";
                if (_answerLabel != null) _answerLabel.text = "ОШИБКА";
                GameManager.Instance.StartCoroutine(ResetErrorText());
            }
        }
        
        private IEnumerator ResetErrorText()
        {
            yield return new WaitForSeconds(1f);
            UpdateAnswerDisplay();
        }

        private void UpdateAnswerDisplay()
        {
            if (_answerLabel != null)
                _answerLabel.text = _currentAnswer;
        }

        private void StartAudio()
        {
             GameManager.Instance.AudioManager?.PlayTone();
        }

        private void StopAudio()
        {
             GameManager.Instance.AudioManager?.StopTone();
        }
    }
}
