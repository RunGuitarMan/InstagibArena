using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI deathMessageText;
    public TextMeshProUGUI speedText;
    public Image crosshair;
    public Image cooldownIndicator;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;

    [Header("Settings")]
    public Color crosshairColor = Color.white;
    public float crosshairSize = 4f;

    private Railgun playerRailgun;
    private PlayerController playerController;
    private Canvas mainCanvas;

    void Start()
    {
        SetupUI();
        playerRailgun = FindFirstObjectByType<Railgun>();
        playerController = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        UpdateCooldownIndicator();
        UpdateSpeedometer();
    }

    void SetupUI()
    {
        // Create canvas if needed
        mainCanvas = GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            mainCanvas = gameObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Create score text
        if (scoreText == null)
        {
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(transform);
            scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.fontSize = 24;
            scoreText.color = Color.white;
            scoreText.alignment = TextAlignmentOptions.TopLeft;

            RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.pivot = new Vector2(0, 1);
            scoreRect.anchoredPosition = new Vector2(20, -20);
            scoreRect.sizeDelta = new Vector2(300, 100);
        }
        UpdateScore(0, 0);

        // Create death message
        if (deathMessageText == null)
        {
            GameObject deathObj = new GameObject("DeathMessage");
            deathObj.transform.SetParent(transform);
            deathMessageText = deathObj.AddComponent<TextMeshProUGUI>();
            deathMessageText.fontSize = 48;
            deathMessageText.color = Color.red;
            deathMessageText.alignment = TextAlignmentOptions.Center;
            deathMessageText.text = "FRAGGED";

            RectTransform deathRect = deathMessageText.GetComponent<RectTransform>();
            deathRect.anchorMin = new Vector2(0.5f, 0.5f);
            deathRect.anchorMax = new Vector2(0.5f, 0.5f);
            deathRect.pivot = new Vector2(0.5f, 0.5f);
            deathRect.anchoredPosition = new Vector2(0, 50);
            deathRect.sizeDelta = new Vector2(400, 100);
        }
        deathMessageText.gameObject.SetActive(false);

        // Create speedometer
        if (speedText == null)
        {
            GameObject speedObj = new GameObject("SpeedText");
            speedObj.transform.SetParent(transform);
            speedText = speedObj.AddComponent<TextMeshProUGUI>();
            speedText.fontSize = 20;
            speedText.color = Color.white;
            speedText.alignment = TextAlignmentOptions.Center;

            RectTransform speedRect = speedText.GetComponent<RectTransform>();
            speedRect.anchorMin = new Vector2(0.5f, 0);
            speedRect.anchorMax = new Vector2(0.5f, 0);
            speedRect.pivot = new Vector2(0.5f, 0);
            speedRect.anchoredPosition = new Vector2(0, 20);
            speedRect.sizeDelta = new Vector2(150, 40);
        }

        // Create crosshair
        if (crosshair == null)
        {
            GameObject crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(transform);
            crosshair = crosshairObj.AddComponent<Image>();
            crosshair.color = crosshairColor;

            RectTransform crossRect = crosshair.GetComponent<RectTransform>();
            crossRect.anchorMin = new Vector2(0.5f, 0.5f);
            crossRect.anchorMax = new Vector2(0.5f, 0.5f);
            crossRect.pivot = new Vector2(0.5f, 0.5f);
            crossRect.anchoredPosition = Vector2.zero;
            crossRect.sizeDelta = new Vector2(crosshairSize, crosshairSize);

            // Create simple dot crosshair
            Texture2D dotTexture = new Texture2D(1, 1);
            dotTexture.SetPixel(0, 0, Color.white);
            dotTexture.Apply();
            crosshair.sprite = Sprite.Create(dotTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        // Create cooldown indicator
        if (cooldownIndicator == null)
        {
            GameObject cooldownObj = new GameObject("CooldownIndicator");
            cooldownObj.transform.SetParent(transform);
            cooldownIndicator = cooldownObj.AddComponent<Image>();
            cooldownIndicator.color = new Color(0, 1, 1, 0.5f);
            cooldownIndicator.type = Image.Type.Filled;
            cooldownIndicator.fillMethod = Image.FillMethod.Radial360;

            RectTransform coolRect = cooldownIndicator.GetComponent<RectTransform>();
            coolRect.anchorMin = new Vector2(0.5f, 0.5f);
            coolRect.anchorMax = new Vector2(0.5f, 0.5f);
            coolRect.pivot = new Vector2(0.5f, 0.5f);
            coolRect.anchoredPosition = Vector2.zero;
            coolRect.sizeDelta = new Vector2(20, 20);

            Texture2D circleTexture = CreateCircleTexture(64);
            cooldownIndicator.sprite = Sprite.Create(circleTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        // Create pause menu
        SetupPauseMenu();
    }

    Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius && dist > radius - 4)
                    texture.SetPixel(x, y, Color.white);
                else
                    texture.SetPixel(x, y, Color.clear);
            }
        }

        texture.Apply();
        return texture;
    }

    void SetupPauseMenu()
    {
        if (pauseMenuPanel == null)
        {
            pauseMenuPanel = new GameObject("PauseMenu");
            pauseMenuPanel.transform.SetParent(transform);

            Image panelImage = pauseMenuPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            RectTransform panelRect = pauseMenuPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(pauseMenuPanel.transform);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "PAUSED";
            titleText.fontSize = 48;
            titleText.color = Color.cyan;
            titleText.alignment = TextAlignmentOptions.Center;

            RectTransform titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.7f);
            titleRect.sizeDelta = new Vector2(400, 80);
            titleRect.anchoredPosition = Vector2.zero;

            // Buttons
            resumeButton = CreateButton("Resume", new Vector2(0, 50), pauseMenuPanel.transform);
            restartButton = CreateButton("Restart", new Vector2(0, -20), pauseMenuPanel.transform);
            quitButton = CreateButton("Quit", new Vector2(0, -90), pauseMenuPanel.transform);

            resumeButton.onClick.AddListener(() => GameManager.Instance?.ResumeGame());
            restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
            quitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());
        }

        pauseMenuPanel.SetActive(false);
    }

    Button CreateButton(string text, Vector2 position, Transform parent)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = Color.cyan;
        colors.pressedColor = Color.white;
        button.colors = colors;

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(200, 50);
        buttonRect.anchoredPosition = position;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    void UpdateCooldownIndicator()
    {
        if (playerRailgun == null || cooldownIndicator == null) return;

        cooldownIndicator.fillAmount = playerRailgun.CooldownPercent;

        // Change color based on ready state
        if (playerRailgun.CanFire)
            cooldownIndicator.color = new Color(0, 1, 1, 0.7f);
        else
            cooldownIndicator.color = new Color(1, 0.5f, 0, 0.5f);
    }

    void UpdateSpeedometer()
    {
        if (playerController == null || speedText == null) return;

        Vector3 vel = playerController.Velocity;
        float horizontalSpeed = new Vector3(vel.x, 0, vel.z).magnitude;
        speedText.text = Mathf.RoundToInt(horizontalSpeed).ToString();
    }

    public void UpdateScore(int kills, int deaths)
    {
        if (scoreText != null)
        {
            scoreText.text = $"FRAGS: {kills}\nDEATHS: {deaths}";
        }
    }

    public void ShowDeathMessage()
    {
        if (deathMessageText != null)
            deathMessageText.gameObject.SetActive(true);
    }

    public void HideDeathMessage()
    {
        if (deathMessageText != null)
            deathMessageText.gameObject.SetActive(false);
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }
}
