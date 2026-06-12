using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UI.ScreenInitializers;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        private const string MusicLevel1To5 = "Music/music_level1-5.wav";
        private const string MusicLevel6To10 = "Music/music_level6-10.wav";
        private const string MusicLevel11To15 = "Music/music_level11-15.wav";

        private const string PauseTemplatePath = "Assets/UI/Gameplay/Pause.uxml";
        private const string HackingViewId = "HackingView";

        private VisualTreeAsset menuTemplate;
        private VisualTreeAsset levelSelectTemplate;
        private VisualTreeAsset dialogueTemplate;
        private VisualTreeAsset pauseTemplate;
        private VisualTreeAsset endingTemplate;

        private VisualElement _root;
        private VisualElement _pauseOverlay;

        private Dictionary<GameState, IScreenInitializer> _screenInitializers;

        public void Init()
        {
            menuTemplate = Resources.Load<VisualTreeAsset>("Main/MainMenu");
            levelSelectTemplate = Resources.Load<VisualTreeAsset>("Main/LevelSelect");
            dialogueTemplate = Resources.Load<VisualTreeAsset>("Main/Dialogue");
            pauseTemplate = Resources.Load<VisualTreeAsset>("Main/Pause");
            endingTemplate = Resources.Load<VisualTreeAsset>("Main/Ending");

            _root = GetComponent<UIDocument>().rootVisualElement;
            InitializeScreenInitializers();
            GameManager.Instance.onStateChanged.AddListener(HandleStateChange);
            HandleStateChange(GameManager.Instance.State);
        }

        private void InitializeScreenInitializers()
        {
            _screenInitializers = new Dictionary<GameState, IScreenInitializer>
            {
                { GameState.MainMenu, new MainMenuScreenInitializer() },
                { GameState.LevelSelect, new LevelSelectScreenInitializer() },
                { GameState.InDialogue, new DialogueScreenInitializer() },
            };
        }

        private void HandleStateChange(GameState newState)
        {
            Debug.Log($"State changed to: {newState}");
            
            if (newState != GameState.MainMenu && newState != GameState.LevelSelect)
            {
                UI.ScreenInitializers.SharedMenuVideoBackground.StopVideo();
            }

            if (_pauseOverlay != null && newState != GameState.Paused)
            {
                if (_root.Contains(_pauseOverlay))
                {
                    _root.Remove(_pauseOverlay);
                }
                _pauseOverlay = null;
                
                if (newState == GameState.Hacking || newState == GameState.InDialogue)
                {
                    return;
                }
            }

            if (newState != GameState.Paused)
            {
                _root.Clear();
                _pauseOverlay = null;
            }

            switch (newState)
            {
                case GameState.MainMenu: 
                    LoadAndInitializeScreen(menuTemplate, _screenInitializers[GameState.MainMenu]);
                    break;
                case GameState.LevelSelect:
                    LoadAndInitializeScreen(levelSelectTemplate, _screenInitializers[GameState.LevelSelect]);
                    break;
                case GameState.InDialogue: 
                    PlayLevelBGM();
                    LoadAndInitializeScreen(dialogueTemplate, _screenInitializers[GameState.InDialogue]);
                    break;
                case GameState.Hacking:
                    PlayLevelBGM();
                    HandleHackingState();
                    break;
                case GameState.LevelEnding:
                    PlayLevelBGM();
                    LoadAndInitializeScreen(endingTemplate, new EndingScreenInitializer());
                    break;
                case GameState.Paused: 
                    ShowPauseOverlay();
                    break;
            }
        }

        private void PlayLevelBGM()
        {
            if (GameManager.Instance.AudioManager == null || GameManager.Instance.LevelManager == null) return;
            
            int level = GameManager.Instance.LevelManager.Level;
            string musicFile = "";
            
            if (level >= 1 && level <= 5) musicFile = MusicLevel1To5;
            else if (level >= 6 && level <= 10) musicFile = MusicLevel6To10;
            else if (level >= 11 && level <= 15) musicFile = MusicLevel11To15;
            
            if (!string.IsNullOrEmpty(musicFile))
            {
                GameManager.Instance.AudioManager.PlayBGM(musicFile);
            }
        }

        public VisualTreeAsset GetCipherTemplate(string cipherName)
        {
            string mappedName = MapCipherName(cipherName);
            string resourcePath = "Ciphers/" + mappedName;
            var template = Resources.Load<VisualTreeAsset>(resourcePath);
            if (template == null)
            {
                Debug.LogError($"[UIManager] Failed to load cipher template from Resources: {resourcePath}");
            }
            return template;
        }

        private string MapCipherName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Caesar";
            return name.ToUpper() switch
            {
                "CAESAR" => "Caesar",
                "ATBASH" => "Atbash",
                "ROT13" => "Rot13",
                "RAILFENCE" => "RailFence",
                "POLYBIUS" => "Polybius",
                "MORSECODE" => "MorseCode",
                "VIGENERE" => "Vigenere",
                "VERNAM" => "Vernam",
                "SHAMIR" => "Shamir",
                "XOR" => "Xor",
                "SDES" => "SDES",
                "MD5" => "MD5",
                "MAC" => "MAC",
                "RSA" => "RSA",
                _ => name
            };
        }

        private void HandleHackingState()
        {
            int level = GameManager.Instance.LevelManager.Level;
            var fromLevelManager = GameManager.Instance.LevelManager.GetCurrentLevelUI();
            if (fromLevelManager != null)
            {
                LoadAndInitializeScreen(fromLevelManager, new HackingScreenInitializer());
                return;
            }

            string cipherName = level switch
            {
                3 => "Atbash",
                4 => "Rot13",
                5 => "RailFence",
                6 => "Polybius",
                7 => "MorseCode",
                8 => "Vigenere",
                9 => "Vernam",
                10 => "Shamir",
                11 => "Xor",
                12 => "SDES",
                13 => "MD5",
                14 => "MAC",
                15 => "RSA",
                _ => "Caesar"
            };
            
            var template = GetCipherTemplate(cipherName);
            LoadAndInitializeScreen(template, new HackingScreenInitializer());
        }

        private void LoadAndInitializeScreen(VisualTreeAsset asset, IScreenInitializer initializer)
        {
            if (asset == null) return;
            
            var instance = asset.Instantiate();
            instance.style.flexGrow = 1;
            _root.Add(instance);
            initializer.Initialize(instance);
        }

        private void ShowPauseOverlay()
        {
            if (_pauseOverlay != null) return;

            if (pauseTemplate == null)
            {
                Debug.LogError("[UIManager] Pause template is missing!");
                return;
            }

            _pauseOverlay = pauseTemplate.Instantiate();
            _pauseOverlay.style.position = Position.Absolute;
            _pauseOverlay.style.left = 0;
            _pauseOverlay.style.top = 0;
            _pauseOverlay.style.right = 0;
            _pauseOverlay.style.bottom = 0;
            _pauseOverlay.style.width = Length.Percent(100);
            _pauseOverlay.style.height = Length.Percent(100);
            
            var initializer = new PauseScreenInitializer();
            initializer.Initialize(_pauseOverlay);
            _root.Add(_pauseOverlay);
            _pauseOverlay.BringToFront();
        }

        public void ChangeHackingTemplate(VisualTreeAsset newTemplate)
        {
            if (newTemplate == null) return;
            var hackingView = _root.Q<VisualElement>(HackingViewId);
            if (hackingView == null) return;

            hackingView.Clear();
            var instance = newTemplate.Instantiate();
            instance.style.flexGrow = 1;
            hackingView.Add(instance);
        }
    }
}
