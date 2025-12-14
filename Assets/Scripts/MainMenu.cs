using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "Game";
    public bool isMenuScene = false;

    private GameObject mainMenuPanel;
    private GameObject optionsPanel;
    private OptionsMenu optionsMenu;
    private Canvas canvas;

    void Awake()
    {
        // IMMEDIATELY pause and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Start()
    {
        GameSettings.Instance.ApplyVideoSettings();

        SetupCanvas();
        CreateMainMenu();
        CreateOptionsPanel();

        ShowMainMenu();
    }

    void SetupCanvas()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Very high to be on top

        if (GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // EventSystem - critical for UI interaction
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    void CreateMainMenu()
    {
        mainMenuPanel = new GameObject("MainMenuPanel", typeof(RectTransform));
        mainMenuPanel.transform.SetParent(transform, false);

        RectTransform panelRect = mainMenuPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Solid background to block view of game
        Image panelBg = mainMenuPanel.AddComponent<Image>();
        panelBg.color = new Color(0.02f, 0.02f, 0.05f, 1f); // Fully opaque
        panelBg.raycastTarget = true; // Block clicks

        // Title
        CreateText(mainMenuPanel.transform, "INSTAGIB ARENA", 72, Color.cyan,
            new Vector2(0.5f, 0.78f), new Vector2(800, 100));

        // Subtitle
        CreateText(mainMenuPanel.transform, "Rail or Die", 28, new Color(0.6f, 0.6f, 0.7f),
            new Vector2(0.5f, 0.68f), new Vector2(400, 50));

        // Buttons
        CreateButton(mainMenuPanel.transform, "PLAY", new Vector2(0.5f, 0.52f), StartGame);
        CreateButton(mainMenuPanel.transform, "OPTIONS", new Vector2(0.5f, 0.42f), ShowOptions);
        CreateButton(mainMenuPanel.transform, "QUIT", new Vector2(0.5f, 0.32f), QuitGame);

        // Controls info
        CreateText(mainMenuPanel.transform,
            "WASD - Move | SPACE - Jump | SHIFT - Sprint/Dash | LMB - Railgun",
            16, new Color(0.5f, 0.5f, 0.6f), new Vector2(0.5f, 0.08f), new Vector2(800, 40));
    }

    void CreateOptionsPanel()
    {
        optionsPanel = new GameObject("OptionsPanel", typeof(RectTransform));
        optionsPanel.transform.SetParent(transform, false);

        RectTransform panelRect = optionsPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelBg = optionsPanel.AddComponent<Image>();
        panelBg.color = new Color(0.02f, 0.02f, 0.05f, 1f);
        panelBg.raycastTarget = true;

        optionsMenu = optionsPanel.AddComponent<OptionsMenu>();
        optionsMenu.Initialize(optionsPanel.transform, this);

        optionsPanel.SetActive(false);
    }

    void CreateText(Transform parent, string text, int fontSize, Color color, Vector2 anchor, Vector2 size)
    {
        GameObject obj = new GameObject("Text", typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    void CreateButton(Transform parent, string text, Vector2 anchor, System.Action onClick)
    {
        GameObject btnObj = new GameObject(text + "Button", typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(300, 60);
        rect.anchoredPosition = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.12f, 0.12f, 0.18f);
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.12f, 0.12f, 0.18f);
        colors.highlightedColor = new Color(0.15f, 0.4f, 0.5f);
        colors.pressedColor = new Color(0, 0.8f, 1f);
        colors.selectedColor = new Color(0.15f, 0.4f, 0.5f);
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick());

        // Text
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowOptions()
    {
        mainMenuPanel.SetActive(false);

        // Destroy and recreate options panel to refresh values
        if (optionsPanel != null)
        {
            Destroy(optionsPanel);
        }
        CreateOptionsPanel();
        optionsPanel.SetActive(true);
    }

    void StartGame()
    {
        GameSettings.Instance.Save();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (isMenuScene)
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void QuitGame()
    {
        GameSettings.Instance.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
