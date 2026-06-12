using UnityEngine.UIElements;
using Managers;
using System.Collections.Generic;

namespace UI.ScreenInitializers
{
    public class PauseScreenInitializer : IScreenInitializer
    {
        public void Initialize(VisualElement screen)
        {
            var resumeBtn = screen.Q<Button>("ResumeButton");
            if (resumeBtn != null)
            {
                resumeBtn.RegisterCallback<ClickEvent>(evt => 
                {
                    PlayClickSound();
                    GameManager.Instance.ResumeFromPause();
                });
                SetupButtonHover(resumeBtn);
            }

            var menuBtn = screen.Q<Button>("MenuButton");
            if (menuBtn != null)
            {
                menuBtn.RegisterCallback<ClickEvent>(evt => 
                {
                    PlayClickSound();
                    GameManager.Instance.ChangeState(GameState.MainMenu);
                });
                SetupButtonHover(menuBtn);
            }

            var volumeBSlider = screen.Q<Slider>("VolumeBGMSlider");
            if (volumeBSlider != null)
            {
                volumeBSlider.value = GameManager.Instance.AudioManager.VolumeBGM;
                volumeBSlider.RegisterValueChangedCallback(evt => {
                    GameManager.Instance.AudioManager.VolumeBGM = evt.newValue;
                });
            }

            var volumeSSlider = screen.Q<Slider>("VolumeSFXSlider");
            if (volumeSSlider != null)
            {
                volumeSSlider.value = GameManager.Instance.AudioManager.VolumeSFX;
                volumeSSlider.RegisterValueChangedCallback(evt => {
                    GameManager.Instance.AudioManager.VolumeSFX = evt.newValue;
                });
            }
        }

        private void SetupButtonHover(Button btn)
        {
            btn.style.transitionProperty = new StyleList<StylePropertyName>(new List<StylePropertyName> { new StylePropertyName("scale") });
            btn.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.1f, TimeUnit.Second) });

            btn.RegisterCallback<PointerEnterEvent>(evt => {
                btn.style.scale = new StyleScale(new Scale(new UnityEngine.Vector3(1.05f, 1.05f, 1f)));
                GameManager.Instance.AudioManager?.PlaySFX("button_aim.wav", true);
            });
            btn.RegisterCallback<PointerLeaveEvent>(evt => {
                btn.style.scale = new StyleScale(new Scale(new UnityEngine.Vector3(1f, 1f, 1f)));
            });
        }

        private void PlayClickSound()
        {
            GameManager.Instance.AudioManager?.PlaySFX("button_click.wav");
        }
    }
}
