using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using Managers;

namespace UI.ScreenInitializers
{
    public class MainMenuScreenInitializer : IScreenInitializer
    {
        private const string ButtonAimSound = "SoundFX/button_aim.wav";
        private const string ButtonClickSound = "SoundFX/button_click.wav";
        
        private const string StartButtonId = "StartButton";
        private const string ContinueButtonId = "ContinueButton";
        private const string ChooseLevelButtonId = "ChooseLevelButton";
        private const string ExitButtonId = "ExitButton";
        
        private const string SettingsButtonId = "SettingsButton";
        private const string SettingsPanelId = "SettingsPanel";
        private const string BGMSliderId = "BGMSlider";
        private const string SFXSliderId = "SFXSlider";
        
        private const string VideoBackgroundId = "VideoBackground";

        public void Initialize(VisualElement screen)
        {
            SetupVideoBackground(screen);

            void PlayHoverSound() => GameManager.Instance.AudioManager?.PlaySFX(ButtonAimSound, true);
            void PlayClickSound() => GameManager.Instance.AudioManager?.PlaySFX(ButtonClickSound);

            GameManager.Instance.AudioManager?.PreloadSFX(ButtonAimSound);
            GameManager.Instance.AudioManager?.PreloadSFX(ButtonClickSound);

            var startBtn = screen.Q<Button>(StartButtonId);
            var contBtn = screen.Q<Button>(ContinueButtonId);
            var chooseBtn = screen.Q<Button>(ChooseLevelButtonId);
            var exitBtn = screen.Q<Button>(ExitButtonId);

            void SetupButton(Button btn, System.Action onClick)
            {
                if (btn == null) return;
                
                btn.RegisterCallback<MouseEnterEvent>(evt => {
                    PlayHoverSound();
                    btn.style.scale = new StyleScale(new Vector2(1.05f, 1.05f));
                    btn.style.transitionDuration = new StyleList<TimeValue>(new System.Collections.Generic.List<TimeValue> { new TimeValue(0.1f) });
                });
                
                btn.RegisterCallback<MouseLeaveEvent>(evt => {
                    btn.style.scale = new StyleScale(Vector2.one);
                });

                btn.RegisterCallback<ClickEvent>(evt => {
                    PlayClickSound();
                    onClick?.Invoke();
                });
            }

            SetupButton(startBtn, () => {
                GameManager.Instance.LevelManager.SetLevel(1);
                GameManager.Instance.DialogueManager.CurrentDialogueId = 0;
                GameManager.Instance.ChangeState(GameState.InDialogue);
            });
            
            SetupButton(contBtn, () => {
                GameManager.Instance.LevelManager.SetLevel(GameManager.Instance.GetLastLevel());
                GameManager.Instance.DialogueManager.CurrentDialogueId = 0;
                GameManager.Instance.ChangeState(GameState.InDialogue);
            });
            
            SetupButton(chooseBtn, () => {
                GameManager.Instance.ChangeState(GameState.LevelSelect);
            });
            
            SetupButton(exitBtn, () => {
                Application.Quit();
            });

            var settingsBtn = screen.Q<Button>(SettingsButtonId);
            var settingsPanel = screen.Q<VisualElement>(SettingsPanelId);
            var bgmSlider = screen.Q<Slider>(BGMSliderId);
            var sfxSlider = screen.Q<Slider>(SFXSliderId);

            if (settingsBtn != null && settingsPanel != null)
            {
                bool isSettingsVisible = false;
                SetupButton(settingsBtn, () => {
                    isSettingsVisible = !isSettingsVisible;
                    settingsPanel.style.display = isSettingsVisible ? DisplayStyle.Flex : DisplayStyle.None;
                });
            }

            if (bgmSlider != null && GameManager.Instance.AudioManager != null)
            {
                bgmSlider.value = GameManager.Instance.AudioManager.VolumeBGM;
                bgmSlider.RegisterValueChangedCallback(evt => {
                    GameManager.Instance.AudioManager.VolumeBGM = evt.newValue;
                });
            }

            if (sfxSlider != null && GameManager.Instance.AudioManager != null)
            {
                sfxSlider.value = GameManager.Instance.AudioManager.VolumeSFX;
                sfxSlider.RegisterValueChangedCallback(evt => {
                    GameManager.Instance.AudioManager.VolumeSFX = evt.newValue;
                });
            }
        }

        private void SetupVideoBackground(VisualElement screen)
        {
            var bgElement = screen.Q<VisualElement>(VideoBackgroundId);
            if (bgElement == null) return;

            var tex = SharedMenuVideoBackground.GetTexture();
            bgElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(tex));
        }
    }
}