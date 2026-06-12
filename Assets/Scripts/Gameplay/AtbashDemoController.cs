using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UI.Util;

namespace Gameplay
{
    public class AtbashDemoController : MonoBehaviour
    {
        private const string Alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        private const float DemoInitialDelay = 1f;
        private const float EncryptedTextDelay = 1.5f;
        private const float HighlightDuration = 0.15f;
        private const float HighlightPause = 0.1f;
        private const float FinalHighlightDuration = 0.3f;
        private const float CharacterDelay = 0.8f;
        private const float DemoCompletionDelay = 2f;
        
        [Header("Settings")]
        [SerializeField] private string wordToDecrypt = "ШИФРОТЕКСТ";
        
        private VisualElement _root;
        private AtbashUIElements _uiElements;
        private UniversalHighlighter _highlighter;
        private bool _isRunning = false;

        public bool IsRunning() => _isRunning;

        public void Initialize(VisualElement rootElement)
        {
            _root = rootElement;
            _isRunning = true;
            _uiElements = new AtbashUIElements(_root);
            _highlighter = new UniversalHighlighter(_uiElements.TopAlphabet, _uiElements.BottomAlphabet, UniversalHighlighter.CyrillicAlphabet, UniversalHighlighter.AtbashCyrillicTop, UniversalHighlighter.AtbashCyrillicBottom);
            HideGameElements();
            StartCoroutine(DemoSequence());
        }

        private void HideGameElements()
        {
            HideElement(_uiElements.InputLabel);
            HideElement(_uiElements.AnswerText);
            HideElement(_uiElements.LettersPanel);
            HideElement(_uiElements.ButtonsContainer);
            HideElement(_uiElements.DemoLetterPairContainer);
            
            if (_uiElements.EncryptedText != null) _uiElements.EncryptedText.text = "";
            HideElement(_uiElements.CipherLabel);
        }

        private void HideElement(VisualElement element)
        {
            if (element != null) element.style.display = DisplayStyle.None;
        }

        private IEnumerator DemoSequence()
        {
            yield return new WaitForSeconds(DemoInitialDelay);

            string encrypted = ApplyAtbash(wordToDecrypt);
            
            ShowElement(_uiElements.CipherLabel);
            if (_uiElements.EncryptedText != null) _uiElements.EncryptedText.text = encrypted;
            
            yield return new WaitForSeconds(EncryptedTextDelay);

            string currentDecryption = "";

            for (int i = 0; i < wordToDecrypt.Length; i++)
            {
                char originalChar = wordToDecrypt[i];
                currentDecryption += originalChar;
                
                if (_uiElements.DecryptedText != null)
                    _uiElements.DecryptedText.text = currentDecryption;
                
                yield return AnimateLetterTransform(originalChar);
                yield return new WaitForSeconds(CharacterDelay);
            }

            yield return new WaitForSeconds(DemoCompletionDelay);
            _isRunning = false;
        }

        private void ShowElement(VisualElement element)
        {
            if (element != null) element.style.display = DisplayStyle.Flex;
        }

        private IEnumerator AnimateLetterTransform(char originalChar)
        {
            int index = Alphabet.IndexOf(originalChar);
            if (index < 0) yield break;

            for (int step = 0; step < 3; step++)
            {
                _highlighter.HighlightAtIndex(32 - index, 32 - index);
                yield return new WaitForSeconds(HighlightDuration);
                _highlighter.Reset();
                yield return new WaitForSeconds(HighlightPause);
            }

            _highlighter.HighlightAtIndex(32 - index, 32 - index);
            yield return new WaitForSeconds(FinalHighlightDuration);
            _highlighter.Reset();
        }

        private string ApplyAtbash(string input)
        {
            char[] buffer = input.ToCharArray();
            int n = Alphabet.Length;
            for (int i = 0; i < buffer.Length; i++)
            {
                char c = buffer[i];
                int index = Alphabet.IndexOf(char.ToUpper(c));

                if (index >= 0)
                {
                    int reverseIndex = n - 1 - index;
                    char newChar = Alphabet[reverseIndex];
                    buffer[i] = char.IsLower(c) ? char.ToLowerInvariant(newChar) : newChar;
                }
                else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    char baseChar = char.IsUpper(c) ? 'A' : 'a';
                    int reverseIndex = 25 - (c - baseChar);
                    buffer[i] = (char)(baseChar + reverseIndex);
                }
            }
            return new string(buffer);
        }

        public void Stop()
        {
            StopAllCoroutines();
            _isRunning = false;
        }
    }
}
