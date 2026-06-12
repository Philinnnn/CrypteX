using UnityEngine;
using Managers;

public class CheatConsole : MonoBehaviour
{
    private bool showConsole = false;
    private string input = "";
    private bool showFPS = false;
    private float deltaTime = 0.0f;
    private string creditsMessage = "";
    private float creditsTimer = 0f;

    void OnEnable()
    {
        UnityEngine.InputSystem.Keyboard.current.onTextInput += OnTextInput;
    }

    void OnDisable()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null)
            UnityEngine.InputSystem.Keyboard.current.onTextInput -= OnTextInput;
    }

    private void OnTextInput(char ch)
    {
        if (!showConsole) return;

        if (ch == '`' || ch == '~') return;

        if (ch == '\b')
        {
            if (input.Length > 0)
                input = input.Substring(0, input.Length - 1);
        }
        else if (ch == '\n' || ch == '\r')
        {
            ProcessCommand(input);
            input = "";
            showConsole = false;
        }
        else if (!char.IsControl(ch))
        {
            input += ch;
        }
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            showConsole = !showConsole;
            if (!showConsole)
                input = "";
        }

        if (showFPS)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
        
        if (creditsTimer > 0)
        {
            creditsTimer -= Time.unscaledDeltaTime;
            if (creditsTimer <= 0) creditsMessage = "";
        }
    }

    void OnGUI()
    {
        if (showFPS)
        {
            int h = Screen.height;
            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(10, 10, 200, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = Mathf.Max(20, h * 2 / 100);
            style.normal.textColor = Color.green;
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }

        if (!string.IsNullOrEmpty(creditsMessage))
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, Screen.height - 100, Screen.width, 50), creditsMessage, style);
        }

        if (!showConsole) return;

        GUI.Box(new Rect(0, 0, Screen.width, 40), "");
        
        GUIStyle inputStyle = new GUIStyle(GUI.skin.label);
        inputStyle.fontSize = 20;

        GUI.Label(new Rect(10, 5, Screen.width - 20, 30), input + "_", inputStyle);
    }

    private void ProcessCommand(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd)) return;
        
        cmd = cmd.Trim().ToLower();
        if (cmd == "fps")
        {
            showFPS = !showFPS;
        }
        else if (cmd == "credits")
        {
            creditsMessage = "Игра создана Батырхановым Арманом в рамках дипломного проекта";
            creditsTimer = 10f;
        }
        else if (cmd == "erase")
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EraseProgress();
                if (GameManager.Instance.State == GameState.LevelSelect)
                {
                    GameManager.Instance.ChangeState(GameState.MainMenu);
                    GameManager.Instance.ChangeState(GameState.LevelSelect);
                }
            }
        }
        else if (cmd.StartsWith("unlock"))
        {
            string[] parts = cmd.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                if (parts[1] == "*" || parts[1] == "all")
                {
                    UnlockLevel(15);
                }
                else if (int.TryParse(parts[1], out int num))
                {
                    UnlockLevel(Mathf.Max(1, num));
                }
            }
            else if (parts.Length == 3 && parts[1] == "to")
            {
                if (int.TryParse(parts[2], out int num))
                {
                    UnlockLevel(Mathf.Max(1, num));
                }
            }
            else if (parts.Length == 3)
            {
                if (int.TryParse(parts[1], out int num1) && int.TryParse(parts[2], out int num2))
                {
                    UnlockLevel(Mathf.Max(num1, num2));
                }
            }
        }
    }
    
    private void UnlockLevel(int level)
    {
        level = Mathf.Clamp(level, 1, 15);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Config.LastLevel = Mathf.Max(GameManager.Instance.Config.LastLevel, level);
            GameManager.Instance.SaveConfig();
        }
        
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.LevelSelect)
        {
            GameManager.Instance.ChangeState(GameState.MainMenu);
            GameManager.Instance.ChangeState(GameState.LevelSelect);
        }
    }
}
