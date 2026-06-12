using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay
{
    public class CaesarDemoController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string encryptedText = "ЕЗЙТЧ";
        [SerializeField] private string targetText = "АГЕНТ";
        
        private VisualElement _root;
        private CaesarHackMinigame _minigame;
        private bool _isRunning = false;

        public bool IsRunning() => _isRunning;

        public void Initialize(VisualElement rootElement, string startTextOverride = null)
        {
            _root = rootElement;
            if (!string.IsNullOrEmpty(startTextOverride)) encryptedText = startTextOverride;
            
            _minigame = new CaesarHackMinigame(_root, encryptedText, targetText, () => {
                Debug.Log("Demo Solved!");
            });

            StartCoroutine(DemoSequence());
        }

        private IEnumerator DemoSequence()
        {
            _isRunning = true;
            yield return new WaitForSeconds(0.5f);
            yield return _minigame.AutoSolve(targetText);
            _isRunning = false;
        }

        public void Stop()
        {
            StopAllCoroutines();
            _isRunning = false;
        }
    }
}
