using UnityEngine;
using UnityEngine.UIElements;
using Managers;

namespace UI.ScreenInitializers
{
    public class EndingScreenInitializer : IScreenInitializer
    {
        private const string MenuButtonId = "MenuButton";
        private const string NextButtonId = "NextButton";

        public void Initialize(VisualElement screen)
        {
            GameManager.Instance.SaveLevel(GameManager.Instance.LevelManager.Level + 1);
            
            var menuBtn = screen.Q<Button>(MenuButtonId);
            var nextBtn = screen.Q<Button>(NextButtonId);
            
            menuBtn?.RegisterCallback<ClickEvent>(evt => GoToMenu());
            nextBtn?.RegisterCallback<ClickEvent>(evt => GoToNextLevel());
        }

        private void GoToMenu()
        {
            GameManager.Instance.ChangeState(GameState.MainMenu);
        }

        private void GoToNextLevel()
        {
            int nextLevel = GameManager.Instance.LevelManager.Level + 1;
            GameManager.Instance.LevelManager.SetLevel(nextLevel);
            GameManager.Instance.DialogueManager.CurrentDialogueId = 0;
            GameManager.Instance.ChangeState(GameState.InDialogue);
        }
    }
}