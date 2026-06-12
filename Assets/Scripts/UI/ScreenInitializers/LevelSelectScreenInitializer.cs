using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using Managers;

namespace UI.ScreenInitializers
{
    public class LevelSelectScreenInitializer : IScreenInitializer
    {
        private const int MaxLevelButtons = 15;
        private const int ButtonWidth = 60;
        private const int ButtonHeight = 60;
        private const int FontSize = 16;

        public void Initialize(VisualElement screen)
        {
            SetupVideoBackground(screen);

            var container = screen.Q<VisualElement>("LevelsContainer");
            if (container == null) return;

            int maxLevel = GameManager.Instance.GetLastLevel();

            VisualElement currentRow = null;
            for (int i = 1; i <= MaxLevelButtons; i++)
            {
                if ((i - 1) % 5 == 0)
                {
                    currentRow = new VisualElement();
                    currentRow.style.flexDirection = FlexDirection.Row;
                    currentRow.style.justifyContent = Justify.Center;
                    currentRow.style.marginBottom = 15;
                    container.Add(currentRow);
                }

                int levelNum = i;
                var button = CreateLevelButton(levelNum, i <= maxLevel);
                button.style.marginLeft = 10;
                button.style.marginRight = 10;
                currentRow?.Add(button);
            }

            screen.Q<Button>("BackButton")?.RegisterCallback<ClickEvent>(_ => 
                GameManager.Instance.ChangeState(GameState.MainMenu));
        }

        private void SetupVideoBackground(VisualElement screen)
        {
            var bgElement = screen.Q<VisualElement>("VideoBackground");
            if (bgElement == null) return;

            var tex = SharedMenuVideoBackground.GetTexture();
            bgElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(tex));
        }

        private Button CreateLevelButton(int levelNum, bool isAvailable)
        {
            var button = new Button(() => SelectLevel(levelNum))
            {
                text = levelNum.ToString(),
                style =
                {
                    width = ButtonWidth,
                    height = ButtonHeight,
                    fontSize = FontSize,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                }
            };

            if (isAvailable)
            {
                button.style.backgroundColor = new Color(97 / 255f, 223 / 255f, 235 / 255f);
                button.style.color = Color.black;
            }
            else
            {
                button.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                button.style.color = Color.gray;
                button.SetEnabled(false);
            }

            return button;
        }

        private void SelectLevel(int level)
        {
            GameManager.Instance.LevelManager.SetLevel(level);
            GameManager.Instance.DialogueManager.CurrentDialogueId = 0;
            GameManager.Instance.ChangeState(GameState.InDialogue);
        }
    }
}
