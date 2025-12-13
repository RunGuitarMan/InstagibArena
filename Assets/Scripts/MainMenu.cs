using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button optionsButton;
    public Button quitButton;

    [Header("Options")]
    public OptionsMenu optionsMenu;

    private Canvas canvas;

    void Start()
    {
        SetupCanvas();
        CreateMainMenu();
        CreateOptionsMenu();

        // Start with main menu visible
        ShowMainMenu();

        // Unlock cursor for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pause game
        Time.timeScale = 0f;
    }

    void SetupCanvas()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Create EventSystem if not exists
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }

    void CreateMainMenu()
    {
        // Main Menu Panel
        mainMenuPanel = CreatePanel("MainMenuPanel");

        // Title
        CreateText(mainMenuPanel.transform, "INSTAGIB ARENA", 72, Color.cyan,
            new Vector2(0.5f, 0.8f), new Vector2(600, 100));

        // Subtitle
        CreateText(mainMenuPanel.transform, "Rail or Die", 24, Color.gray,
            new Vector2(0.5f, 0.72f), new Vector2(400, 50));

        // Buttons
        playButton = CreateMenuButton(mainMenuPanel.transform, "PLAY", new Vector2(0.5f, 0.5f));
        optionsButton = CreateMenuButton(mainMenuPanel.transform, "OPTIONS", new Vector2(0.5f, 0.4f));
        quitButton = CreateMenuButton(mainMenuPanel.transform, "QUIT", new Vector2(0.5f, 0.3f));

        // Button actions
        playButton.onClick.AddListener(StartGame);
        optionsButton.onClick.AddListener(ShowOptions);
        quitButton.onClick.AddListener(QuitGame);

        // Controls info
        CreateText(mainMenuPanel.transform,
            "WASD - Move | SPACE - Jump | SHIFT - Sprint | Double SHIFT - Dash | LMB - Railgun",
            16, Color.gray, new Vector2(0.5f, 0.1f), new Vector2(800, 40));
    }

    void CreateOptionsMenu()
    {
        optionsPanel = CreatePanel("OptionsPanel");

        GameObject optionsObj = new GameObject("OptionsMenuController");
        optionsObj.transform.SetParent(optionsPanel.transform);
        optionsMenu = optionsObj.AddComponent<OptionsMenu>();
        optionsMenu.Initialize(optionsPanel.transform, this);

        optionsPanel.SetActive(false);
    }

    GameObject CreatePanel(string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(transform);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return panel;
    }

    TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, Color color, Vector2 anchor, Vector2 size)
    {
        GameObject textObj = new GameObject("Text_" + text);
        textObj.transform.SetParent(parent);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rect = tmp.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        return tmp;
    }

    Button CreateMenuButton(Transform parent, string text, Vector2 anchor)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.2f);
        colors.highlightedColor = new Color(0.1f, 0.5f, 0.7f);
        colors.pressedColor = Color.cyan;
        colors.selectedColor = new Color(0.1f, 0.5f, 0.7f);
        button.colors = colors;

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(300, 60);

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = tmp.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
        optionsMenu.RefreshValues();
    }

    void StartGame()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Destroy menu after starting
        Destroy(gameObject, 0.1f);
    }

    void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
