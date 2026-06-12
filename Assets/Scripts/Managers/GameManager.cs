using UnityEngine;
using UnityEngine.Events;
namespace Managers
{
    public class GameManager : BaseManager
    {
        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; } = GameState.MainMenu;
        public UnityEvent<GameState> onStateChanged;
        public LevelManager LevelManager { get; private set; }
        public DialogueManager DialogueManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        private string ConfigPath => System.IO.Path.Combine(Application.persistentDataPath, "config.json");
        [System.Serializable]
        public class GameConfig
        {
            public int LastLevel = 1;
            public float VolumeBGM = 1.0f;
            public float VolumeSFX = 1.0f;
        }
        public GameConfig Config { get; private set; } = new GameConfig();
        #if UNITY_EDITOR
        private const bool TEST_MODE = true;
        private const int TEST_LEVELS = 15;
        #else
        private const bool TEST_MODE = false;
        #endif
        private GameState _prePauseState = GameState.Hacking;
        private void Start()
        {
            Init();
        }
        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (State == GameState.Hacking || State == GameState.InDialogue)
                {
                    _prePauseState = State;
                    ChangeState(GameState.Paused);
                }
                else if (State == GameState.Paused)
                {
                    ChangeState(_prePauseState);
                }
            }
        }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            
            if (LevelManager == null) LevelManager = FindFirstObjectByType<LevelManager>();
            if (DialogueManager == null) DialogueManager = FindFirstObjectByType<DialogueManager>();
            if (UIManager == null) UIManager = FindFirstObjectByType<UIManager>();
            if (AudioManager == null)
            {
                AudioManager = FindFirstObjectByType<AudioManager>();
                if (AudioManager == null)
                {
                    var audioObj = new GameObject("AudioManager");
                    audioObj.transform.SetParent(this.transform);
                    AudioManager = audioObj.AddComponent<AudioManager>();
                }
            }
            if (gameObject.GetComponent<CheatConsole>() == null)
            {
                gameObject.AddComponent<CheatConsole>();
            }
        }
        public override void Init()
        {
            LoadConfig();
            #if UNITY_EDITOR
            if (TEST_MODE)
            {
                Config.LastLevel = TEST_LEVELS;
                SaveConfig();
                Debug.Log($"TEST MODE: Unlocked all {TEST_LEVELS} levels");
            }
            #endif
            LoadLastLevel();
            LevelManager?.Init();
            DialogueManager?.Init();
            UIManager?.Init();
            AudioManager?.Init();
        }
        public void LoadConfig()
        {
            try
            {
                if (System.IO.File.Exists(ConfigPath))
                {
                    string json = System.IO.File.ReadAllText(ConfigPath);
                    JsonUtility.FromJsonOverwrite(json, Config);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load config: {e.Message}");
            }
        }
        public void SaveConfig()
        {
            try
            {
                string json = JsonUtility.ToJson(Config, true);
                System.IO.File.WriteAllText(ConfigPath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save config: {e.Message}");
            }
        }
        public void ChangeState(GameState newState)
        {
            if (State == newState) return;
            State = newState;
            onStateChanged?.Invoke(State);
        }
        public void ResumeFromPause()
        {
            if (State == GameState.Paused)
            {
                ChangeState(_prePauseState);
            }
        }
        public int GetLastLevel()
        {
            return Config.LastLevel;
        }
        public void SaveLevel(int level)
        {
            if (level > GetLastLevel())
            {
                Config.LastLevel = level;
                SaveConfig();
                Debug.Log($"Saved level: {level}");
            }
        }
        public void EraseProgress()
        {
            Config.LastLevel = 1;
            SaveConfig();
            LoadLastLevel();
            Debug.Log("Progress erased. Returning to level 1.");
        }
        private void LoadLastLevel()
        {
            int lastLevel = GetLastLevel();
            LevelManager?.SetLevel(lastLevel);
        }
    }
    public enum GameState
    {
        MainMenu,
        LevelSelect,
        Hacking,
        Paused,
        InDialogue,
        LevelEnding
    }
}
