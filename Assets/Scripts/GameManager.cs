using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int startingLives = 3;
    public int startingTime = 60;
    public int pointsPerFrog = 10;
    public int wrongClickPenalty = 5;

    [Header("Game Systems")]
    public FrogSpawner frogSpawner;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text livesText;

    private int score;
    private int lives;
    private float timeRemaining;
    private bool isPlaying;
    private bool hasEnded;

    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle hudStyle;
    private GUIStyle panelStyle;
    private GUIStyle buttonStyle;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("GameManager: Only one GameManager can exist in a scene.", this);
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        score = 0;
        lives = startingLives;
        timeRemaining = startingTime;

        UpdateUI();
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        // Check whether the player clicked.
        if (WasLeftClickPressed())
            HandlePlayerClick();

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Max(0f, timeRemaining);

        UpdateUI();

        if (timeRemaining <= 0f)
        {
            EndGame();
        }
    }

    public void StartGame()
    {
        score = 0;
        lives = startingLives;
        timeRemaining = startingTime;
        hasEnded = false;
        isPlaying = true;

        UpdateUI();

        if (frogSpawner != null)
        {
            frogSpawner.BeginSpawning();
        }
        else
        {
            Debug.LogError("GameManager: Assign the FrogSpawner in the Inspector.", this);
        }

        Debug.Log("Game started!");
    }

    public void FrogKilled()
    {
        if (!isPlaying)
            return;

        score += pointsPerFrog;
        UpdateUI();

        Debug.Log("Frog eliminated! Score: " + score);
    }

    public void WrongClick()
    {
        if (!isPlaying)
            return;

        score = Mathf.Max(0, score - wrongClickPenalty);
        UpdateUI();
    }

    public void FrogEscaped()
    {
        if (!isPlaying)
            return;

        lives--;
        UpdateUI();

        if (lives <= 0)
        {
            EndGame();
        }
    }

    public void EndGame()
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        hasEnded = true;

        if (frogSpawner != null)
        {
            frogSpawner.StopSpawning();
        }

        foreach (Frog frog in FindObjectsByType<Frog>(FindObjectsSortMode.None))
        {
            Destroy(frog.gameObject);
        }

        UpdateUI();
        Debug.Log("Game Over! Final score: " + score);
    }

    private void OnGUI()
    {
        EnsureGuiStyles();

        float scale = Mathf.Clamp(Screen.height / 720f, 0.65f, 1.4f);
        titleStyle.fontSize = Mathf.RoundToInt(34f * scale);
        bodyStyle.fontSize = Mathf.RoundToInt(17f * scale);
        hudStyle.fontSize = Mathf.RoundToInt(19f * scale);
        buttonStyle.fontSize = Mathf.RoundToInt(20f * scale);
        if (isPlaying)
        {
            DrawHud(scale);
        }
        else
        {
            DrawMenu(scale);
        }
    }

    private void DrawHud(float scale)
    {
        float width = Mathf.Min(Screen.width - 32f * scale, 450f * scale);
        float height = 68f * scale;
        Rect panel = new Rect((Screen.width - width) * 0.5f, 16f * scale, width, height);
        GUI.Box(panel, GUIContent.none, panelStyle);

        float itemWidth = width / 3f;
        GUI.Label(new Rect(panel.x, panel.y, itemWidth, height), "SCORE\n" + score, hudStyle);
        GUI.Label(new Rect(panel.x + itemWidth, panel.y, itemWidth, height),
            "TIME\n" + Mathf.CeilToInt(timeRemaining), hudStyle);
        GUI.Label(new Rect(panel.x + itemWidth * 2f, panel.y, itemWidth, height),
            "LIVES\n" + lives, hudStyle);
    }

    private void DrawMenu(float scale)
    {
        float width = Mathf.Min(Screen.width - 32f * scale, 500f * scale);
        float height = (hasEnded ? 300f : 340f) * scale;
        Rect panel = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height
        );
        GUI.Box(panel, GUIContent.none, panelStyle);

        string title = hasEnded ? "GAME OVER" : "FROG FRENZY";
        GUI.Label(
            new Rect(panel.x + 16f * scale, panel.y + 32f * scale, width - 32f * scale, 54f * scale),
            title,
            titleStyle
        );

        string instructions = hasEnded
            ? "Final score: " + score + "\nThe pond is quiet... for now."
            : "Tap or click the frogs before they escape!\nMisses cost points. Keep your eyes peeled.";
        GUI.Label(
            new Rect(panel.x + 24f * scale, panel.y + 100f * scale, width - 48f * scale, 72f * scale),
            instructions,
            bodyStyle
        );

        float buttonWidth = Mathf.Min(width - 48f * scale, 260f * scale);
        Rect button = new Rect(
            panel.x + (width - buttonWidth) * 0.5f,
            panel.y + height - 90f * scale,
            buttonWidth,
            54f * scale
        );
        if (GUI.Button(button, hasEnded ? "PLAY AGAIN" : "START GAME", buttonStyle))
        {
            StartGame();
        }
    }

    private void EnsureGuiStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 34,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.69f, 0.94f, 0.58f) }
        };
        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 17,
            wordWrap = true,
            normal = { textColor = Color.white }
        };
        hudStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 19,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        panelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            normal = { textColor = Color.white }
        };
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.08f, 0.16f, 0.12f) }
        };
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (timerText != null)
            timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining);

        if (livesText != null)
            livesText.text = "Lives: " + lives;
    }

    private bool WasLeftClickPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            return true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }
#endif

        return false;
    }

    private Vector2 GetPointerScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif

#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }

        return Input.mousePosition;
#else
        return Vector2.zero;
#endif
    }

    private void HandlePlayerClick()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogWarning("No MainCamera found.");
            return;
        }

        Vector3 screenPosition = GetPointerScreenPosition();
        Vector3 worldPosition = cam.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        // Find a 2D collider at the clicked world position.
        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit != null)
        {
            Frog frog = hit.GetComponentInParent<Frog>();

            if (frog != null)
            {
                frog.Kill();
                return;
            }
        }

        // No frog was clicked.
        WrongClick();
    }
} // Properly placed closing bracket for GameManager class
