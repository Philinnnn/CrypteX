using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay;
using Managers;

namespace UI.ScreenInitializers
{
    public class DialogueScreenInitializer : IScreenInitializer
    {
        private const string ButtonAimSound = "SoundFX/button_aim.wav";
        private const string ButtonClickSound = "SoundFX/button_click.wav";
        
        private const string NextButtonId = "NextButton";
        
        private const string AlexAuthor = "Алекс";
        private const string EvaAuthor = "Ева Штайн";
        private const string MarcusAuthor = "Маркус";
        private const string SystemAuthor = "Система";
        private const string ActionAuthor = "ACTION";
        
        private const string AlexPortraitId = "AlexPortrait";
        private const string EvaPortraitId = "EvaPortrait";
        private const string MarcusPortraitId = "MarcusPortrait";
        private const string SystemPortraitId = "SystemPortrait";
        
        private const string AlexTextId = "AlexText";
        private const string EvaTextId = "EvaText";
        private const string MarcusTextId = "MarcusText";
        private const string SystemTextId = "SystemText";
        
        private const string DemoPrefix = "DEMO_";
        private const string CaesarCipher = "CAESAR";
        private const string AtbashCipher = "ATBASH";
        private const string Rot13Cipher = "ROT13";
        
        private const string RightUpButtonId = "RightUpButton";
        private const string LeftDownButtonId = "LeftDownButton";

        private const string CaesarDemoText = "ЕЗЙТЧ";

        public void Initialize(VisualElement screen)
        {
            int level = GameManager.Instance.LevelManager.Level;
            LoadDialogueState(level);
            
            GameManager.Instance.AudioManager?.PreloadSFX(ButtonAimSound);
            GameManager.Instance.AudioManager?.PreloadSFX(ButtonClickSound);

            var portraits = CreatePortraitDictionary(screen);
            var labels = CreateLabelDictionary(screen);

            Action updateUI = null;
            updateUI = () => UpdateDialogueUI(screen, portraits, labels, updateUI);

            updateUI();

            var nextBtn = screen.Q<Button>(NextButtonId);
            if (nextBtn != null)
            {
                nextBtn.RegisterCallback<MouseEnterEvent>(evt => {
                    GameManager.Instance.AudioManager?.PlaySFX(ButtonAimSound, true);
                    nextBtn.style.scale = new StyleScale(new Vector2(1.05f, 1.05f));
                    nextBtn.style.transitionDuration = new StyleList<TimeValue>(new System.Collections.Generic.List<TimeValue> { new TimeValue(0.1f) });
                });
                
                nextBtn.RegisterCallback<MouseLeaveEvent>(evt => {
                    nextBtn.style.scale = new StyleScale(Vector2.one);
                });

                nextBtn.RegisterCallback<ClickEvent>(evt => 
                {
                    GameManager.Instance.AudioManager?.PlaySFX(ButtonClickSound);
                    if (GameManager.Instance.DialogueManager.IsEndOfDialogue())
                    {
                        HandleDialogueEnd();
                    }
                    else
                    {
                        updateUI();
                    }
                });
            }
        }

        private void LoadDialogueState(int level)
        {
            var dialogueManager = GameManager.Instance.DialogueManager;
            
            if (dialogueManager.CurrentDialogueId == 0)
            {
                dialogueManager.LoadDialogue(level, 1);
            }
            else if (dialogueManager.CurrentDialogueId == 1 && dialogueManager.IsEndOfDialogue())
            {
                dialogueManager.LoadDialogue(level, 2);
            }
            else if (dialogueManager.CurrentDialogueId != 1)
            {
                dialogueManager.LoadDialogue(level, 1);
            }
        }

        private Dictionary<string, VisualElement> CreatePortraitDictionary(VisualElement screen)
        {
            return new Dictionary<string, VisualElement>
            {
                { AlexAuthor, screen.Q<VisualElement>(AlexPortraitId) },
                { EvaAuthor, screen.Q<VisualElement>(EvaPortraitId) },
                { MarcusAuthor, screen.Q<VisualElement>(MarcusPortraitId) },
                { SystemAuthor, screen.Q<VisualElement>(SystemPortraitId) }
            };
        }

        private Dictionary<string, Label> CreateLabelDictionary(VisualElement screen)
        {
            return new Dictionary<string, Label>
            {
                { AlexAuthor, screen.Q<Label>(AlexTextId) },
                { MarcusAuthor, screen.Q<Label>(MarcusTextId) },
                { EvaAuthor, screen.Q<Label>(EvaTextId) },
                { SystemAuthor, screen.Q<Label>(SystemTextId) }
            };
        }

        private void UpdateDialogueUI(VisualElement screen, Dictionary<string, VisualElement> portraits, Dictionary<string, Label> labels, Action updateUI)
        {
            var line = GameManager.Instance.DialogueManager.GetNextLine();
            if (line == null) return;
            
            if (line.author == ActionAuthor)
            {
                if (line.text.StartsWith(DemoPrefix))
                {
                    ShowCipherDemo(screen, line.text, updateUI);
                }
                return;
            }

            HideAllPortraits(portraits);
            
            if (portraits.ContainsKey(line.author) && portraits[line.author] != null)
            {
                portraits[line.author].style.display = DisplayStyle.Flex;
                
                if (labels.ContainsKey(line.author) && labels[line.author] != null)
                {
                    labels[line.author].text = line.text;
                }
            }
        }

        private void ShowCipherDemo(VisualElement screen, string actionText, Action updateUI)
        {
            if (!actionText.StartsWith(DemoPrefix))
            {
                return;
            }

            string cipherName = actionText.Substring(DemoPrefix.Length);

            var uiManager = UnityEngine.Object.FindObjectOfType<UIManager>();
            VisualTreeAsset demoTemplate = GetTemplateForCipher(cipherName, uiManager);
            
            if (demoTemplate == null) 
            {
                Debug.LogError($"Demo template not found for cipher: {cipherName}");
                return;
            }

            HideAllUIElements(screen);
            
            var demoInstance = demoTemplate.Instantiate();
            demoInstance.style.position = Position.Absolute;
            demoInstance.style.width = Length.Percent(100);
            demoInstance.style.height = Length.Percent(100);
            screen.Add(demoInstance);
            
            MonoBehaviour demoController = CreateDemoController(cipherName, demoInstance);
            if (demoController != null)
            {
                GameManager.Instance.StartCoroutine(WaitDemoComplete(demoController, demoInstance, screen, updateUI));
            }
            else
            {
                Debug.LogError($"Failed to create demo controller for cipher: {cipherName}");
            }
        }

        private VisualTreeAsset GetTemplateForCipher(string cipherName, UIManager uiManager)
        {
            return uiManager.GetCipherTemplate(cipherName);
        }

        private MonoBehaviour CreateDemoController(string cipherName, VisualElement demoInstance)
        {
            return cipherName switch
            {
                CaesarCipher => CreateCaesarDemoController(demoInstance),
                AtbashCipher => CreateAtbashDemoController(demoInstance),
                _ => null
            };
        }

        private MonoBehaviour CreateCaesarDemoController(VisualElement demoInstance)
        {
            var demoController = new GameObject().AddComponent<CaesarDemoController>();
            HideButton(demoInstance, RightUpButtonId);
            HideButton(demoInstance, LeftDownButtonId);
            demoController.Initialize(demoInstance, CaesarDemoText);
            return demoController;
        }

        private MonoBehaviour CreateAtbashDemoController(VisualElement demoInstance)
        {
            var demoController = new GameObject().AddComponent<AtbashDemoController>();
            demoController.Initialize(demoInstance);
            return demoController;
        }

        private void HideAllPortraits(Dictionary<string, VisualElement> portraits)
        {
            foreach (var portrait in portraits.Values)
            {
                if (portrait != null) portrait.style.display = DisplayStyle.None;
            }
        }

        private void HideAllUIElements(VisualElement screen)
        {
            var portraitElements = new[] { AlexPortraitId, EvaPortraitId, MarcusPortraitId };
            foreach (var portraitName in portraitElements)
            {
                var element = screen.Q<VisualElement>(portraitName);
                if (element != null) element.style.display = DisplayStyle.None;
            }
            
            var textParent = screen.Q<Label>(AlexTextId)?.parent;
            if (textParent != null) textParent.style.display = DisplayStyle.None;
        }

        private void HideButton(VisualElement root, string buttonName)
        {
            var btn = root.Q<VisualElement>(buttonName);
            if (btn != null) btn.style.display = DisplayStyle.None;
        }

        private void HandleDialogueEnd()
        {
            int dialogueId = GameManager.Instance.DialogueManager.CurrentDialogueId;
            GameState nextState = dialogueId == 1 ? GameState.Hacking : GameState.LevelEnding;
            GameManager.Instance.ChangeState(nextState);
        }

        private System.Collections.IEnumerator WaitDemoComplete(MonoBehaviour demoController, VisualElement demoInstance, VisualElement screen, Action onComplete)
        {
            yield return new WaitForSeconds(0.5f);
            
            if (demoController is CaesarDemoController caesarDemo)
            {
                yield return new WaitUntil(() => !caesarDemo.IsRunning());
            }
            else if (demoController is AtbashDemoController atbashDemo)
            {
                yield return new WaitUntil(() => !atbashDemo.IsRunning());
            }
            
            yield return new WaitForSeconds(1.5f);
            
            screen.Remove(demoInstance);
            UnityEngine.Object.Destroy(demoController);
            onComplete?.Invoke();
        }
    }
}

