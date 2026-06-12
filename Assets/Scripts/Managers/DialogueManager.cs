using UnityEngine;
using System.IO;
using Dialogues;

namespace Managers
{
    public class DialogueManager : BaseManager
    {
        private DialogueContainer _currentDialogue;
        private int _currentIndex = 0;
        
        public int CurrentDialogueId { get; set; } = 0;

        public override void Init()
        {
        }

        public void LoadDialogue(int levelId, int dialogueId)
        {
            CurrentDialogueId = dialogueId;
            var fileName = dialogueId + ".json";
            var path = Path.Combine(Application.streamingAssetsPath, "UI", "Dialogues", $"Level_{levelId}", fileName);
            
            Debug.Log($"LoadDialogue: level={levelId}, id={dialogueId}, path={path}");

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                _currentDialogue = JsonUtility.FromJson<DialogueContainer>(json);
                _currentIndex = 0;
                int count = (_currentDialogue != null && _currentDialogue.lines != null) ? _currentDialogue.lines.Count : -1;
                Debug.Log($"Dialogue loaded: lines={count}");
            }
            else
            {
                Debug.LogError("Dialogue file not found: " + path);
            }
        }

        public DialogueLine GetNextLine()
        {
            if (_currentDialogue == null || _currentIndex >= _currentDialogue.lines.Count)
                return null;

            return _currentDialogue.lines[_currentIndex++];
        }

        public bool IsEndOfDialogue()
        {
            if (_currentDialogue == null || _currentDialogue.lines == null) return true;
            return _currentIndex >= _currentDialogue.lines.Count;
        }
    }
}