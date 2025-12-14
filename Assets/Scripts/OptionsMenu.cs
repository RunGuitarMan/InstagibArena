using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OptionsMenu : MonoBehaviour
{
    private Transform container;
    private MainMenu mainMenu;
    private GameSettings settings;

    private GameObject[] tabPanels;
    private Transform[] tabContents; // Store content references directly
    private Button[] tabButtons;

    private bool waitingForKey = false;
    private System.Action<KeyCode> onKeyPressed;

    public void Initialize(Transform parent, MainMenu menu)
    {
        container = parent;
        mainMenu = menu;
        settings = GameSettings.Instance;

        BuildUI();
    }

    void Update()
    {
        if (waitingForKey)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key) && key != KeyCode.Escape)
                {
                    onKeyPressed?.Invoke(key);
                    waitingForKey = false;
                    onKeyPressed = null;
                    break;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                waitingForKey = false;
                onKeyPressed = null;
            }
        }
    }

    void BuildUI()
    {
        // Title
        CreateTitle("OPTIONS");

        // Tab buttons
        string[] tabs = { "GAME", "VIDEO", "INPUT" };
        tabButtons = new Button[tabs.Length];
        tabPanels = new GameObject[tabs.Length];
        tabContents = new Transform[tabs.Length];

        float tabWidth = 180f;
        float totalWidth = tabs.Length * tabWidth + (tabs.Length - 1) * 10f;
        float startX = -totalWidth / 2f + tabWidth / 2f;

        for (int i = 0; i < tabs.Length; i++)
        {
            int idx = i;
            tabButtons[i] = CreateTabButton(tabs[i], startX + i * (tabWidth + 10f), () => SelectTab(idx));
        }

        // Create tab panels and store content references
        for (int i = 0; i < tabs.Length; i++)
        {
            var result = CreateTabPanel();
            tabPanels[i] = result.Item1;
            tabContents[i] = result.Item2;
        }

        // Populate tabs
        PopulateGameTab(tabContents[0]);
        PopulateVideoTab(tabContents[1]);
        PopulateInputTab(tabContents[2]);

        // Back button
        CreateBackButton();

        // Show first tab
        SelectTab(0);
    }

    void CreateTitle(string text)
    {
        GameObject obj = new GameObject("Title", typeof(RectTransform));
        obj.transform.SetParent(container, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.92f);
        rect.anchorMax = new Vector2(0.5f, 0.98f);
        rect.sizeDelta = new Vector2(400, 60);
        rect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 48;
        tmp.color = Color.cyan;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    Button CreateTabButton(string text, float xPos, System.Action onClick)
    {
        GameObject obj = new GameObject(text + "Tab", typeof(RectTransform));
        obj.transform.SetParent(container, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.85f);
        rect.anchorMax = new Vector2(0.5f, 0.85f);
        rect.sizeDelta = new Vector2(180, 45);
        rect.anchoredPosition = new Vector2(xPos, 0);

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f);
        img.raycastTarget = true;

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick());

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(obj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 22;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;

        return btn;
    }

    (GameObject, Transform) CreateTabPanel()
    {
        // Main scroll view container
        GameObject scrollObj = new GameObject("TabPanel", typeof(RectTransform));
        scrollObj.transform.SetParent(container, false);

        RectTransform scrollRect = scrollObj.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.1f, 0.12f);
        scrollRect.anchorMax = new Vector2(0.9f, 0.8f);
        scrollRect.sizeDelta = Vector2.zero;
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;

        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = new Color(0.05f, 0.05f, 0.08f, 0.98f);
        scrollBg.raycastTarget = true;

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;

        // Viewport
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(scrollObj.transform, false);

        RectTransform vpRect = viewport.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.sizeDelta = Vector2.zero;
        vpRect.offsetMin = new Vector2(5, 5);
        vpRect.offsetMax = new Vector2(-5, -5);

        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0, 0, 0, 0);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content - this is what we return
        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);

        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        contentRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(30, 30, 15, 15);
        vlg.spacing = 6;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = vpRect;
        scroll.content = contentRect;

        scrollObj.SetActive(false);
        return (scrollObj, content.transform);
    }

    void SelectTab(int index)
    {
        for (int i = 0; i < tabPanels.Length; i++)
        {
            tabPanels[i].SetActive(i == index);
            tabButtons[i].GetComponent<Image>().color =
                i == index ? new Color(0.1f, 0.5f, 0.7f) : new Color(0.15f, 0.15f, 0.2f);
        }
    }

    void CreateBackButton()
    {
        GameObject obj = new GameObject("BackButton", typeof(RectTransform));
        obj.transform.SetParent(container, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.04f);
        rect.anchorMax = new Vector2(0.5f, 0.04f);
        rect.sizeDelta = new Vector2(200, 50);
        rect.anchoredPosition = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.5f, 0.15f, 0.15f);
        img.raycastTarget = true;

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.7f, 0.2f, 0.2f);
        colors.pressedColor = new Color(0.9f, 0.3f, 0.3f);
        btn.colors = colors;
        btn.onClick.AddListener(() =>
        {
            settings.Save();
            mainMenu.ShowMainMenu();
        });

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(obj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "BACK";
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
    }

    #region GAME TAB

    void PopulateGameTab(Transform content)
    {
        AddHeader(content, "MOVEMENT");
        AddSlider(content, "Move Speed", 5f, 25f, settings.moveSpeed, v => settings.moveSpeed = v);
        AddSlider(content, "Sprint Multiplier", 1f, 3f, settings.sprintMultiplier, v => settings.sprintMultiplier = v);
        AddSlider(content, "Jump Force", 4f, 15f, settings.jumpForce, v => settings.jumpForce = v);
        AddSlider(content, "Gravity", 10f, 40f, settings.gravity, v => settings.gravity = v);
        AddSlider(content, "Ground Friction", 1f, 15f, settings.groundFriction, v => settings.groundFriction = v);
        AddSlider(content, "Max Speed", 15f, 60f, settings.maxBunnySpeed, v => settings.maxBunnySpeed = v);

        AddHeader(content, "DASH");
        AddSlider(content, "Dash Distance", 3f, 20f, settings.dashDistance, v => settings.dashDistance = v);
        AddSlider(content, "Dash Cooldown", 0.2f, 3f, settings.dashCooldown, v => settings.dashCooldown = v);

        AddHeader(content, "COMBAT");
        AddSlider(content, "Railgun Cooldown", 0.3f, 3f, settings.railgunCooldown, v => settings.railgunCooldown = v);
        AddSlider(content, "Rail Jump Force", 1f, 30f, settings.railJumpForce, v => settings.railJumpForce = v);

        AddHeader(content, "ARENA (restart required)");
        AddSlider(content, "Arena Size", 40f, 200f, settings.arenaWidth, v => { settings.arenaWidth = v; settings.arenaLength = v; });
        AddSliderInt(content, "Pillars", 0, 30, settings.numberOfPillars, v => settings.numberOfPillars = v);
        AddSliderInt(content, "Platforms", 0, 20, settings.numberOfPlatforms, v => settings.numberOfPlatforms = v);
        AddSliderInt(content, "Towers", 0, 8, settings.numberOfTowers, v => settings.numberOfTowers = v);

        AddSpacer(content, 10);
        AddActionButton(content, "RESET TO DEFAULTS", () =>
        {
            settings.ResetToDefaults();
            mainMenu.ShowOptions();
        });
    }

    #endregion

    #region VIDEO TAB

    void PopulateVideoTab(Transform content)
    {
        AddHeader(content, "DISPLAY");

        Resolution[] resolutions = Screen.resolutions;
        List<string> resOptions = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            resOptions.Add($"{resolutions[i].width}x{resolutions[i].height}");
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentIndex = i;
        }

        if (settings.resolutionIndex < 0) settings.resolutionIndex = currentIndex;

        AddDropdown(content, "Resolution", resOptions.ToArray(), settings.resolutionIndex, idx =>
        {
            settings.resolutionIndex = idx;
            Resolution r = resolutions[idx];
            Screen.SetResolution(r.width, r.height, settings.fullscreen);
        });

        AddToggle(content, "Fullscreen", settings.fullscreen, v =>
        {
            settings.fullscreen = v;
            Screen.fullScreen = v;
        });

        AddDropdown(content, "VSync", new[] { "Off", "On", "Double" }, settings.vsync, v =>
        {
            settings.vsync = v;
            QualitySettings.vSyncCount = v;
        });

        AddDropdown(content, "Quality", QualitySettings.names, settings.qualityLevel, v =>
        {
            settings.qualityLevel = v;
            QualitySettings.SetQualityLevel(v);
        });

        AddHeader(content, "PERFORMANCE");

        int fpsIdx = 0;
        int[] fpsValues = { -1, 30, 60, 120, 144, 240 };
        for (int i = 0; i < fpsValues.Length; i++)
            if (fpsValues[i] == settings.targetFrameRate) fpsIdx = i;

        AddDropdown(content, "Target FPS", new[] { "Unlimited", "30", "60", "120", "144", "240" }, fpsIdx, idx =>
        {
            settings.targetFrameRate = fpsValues[idx];
            Application.targetFrameRate = fpsValues[idx];
        });

        AddSpacer(content, 10);
        AddActionButton(content, "APPLY", () =>
        {
            settings.ApplyVideoSettings();
            settings.Save();
        });
    }

    #endregion

    #region INPUT TAB

    void PopulateInputTab(Transform content)
    {
        AddHeader(content, "MOUSE");
        AddSlider(content, "Sensitivity", 0.1f, 10f, settings.mouseSensitivity, v => settings.mouseSensitivity = v);
        AddToggle(content, "Invert Y", settings.invertMouseY, v => settings.invertMouseY = v);

        AddHeader(content, "KEY BINDINGS");
        AddKeyBind(content, "Move Forward", settings.moveForward, k => settings.moveForward = k);
        AddKeyBind(content, "Move Back", settings.moveBack, k => settings.moveBack = k);
        AddKeyBind(content, "Move Left", settings.moveLeft, k => settings.moveLeft = k);
        AddKeyBind(content, "Move Right", settings.moveRight, k => settings.moveRight = k);
        AddKeyBind(content, "Jump", settings.jump, k => settings.jump = k);
        AddKeyBind(content, "Sprint/Dash", settings.sprint, k => settings.sprint = k);

        AddSpacer(content, 10);
        AddActionButton(content, "RESET BINDINGS", () =>
        {
            settings.moveForward = KeyCode.W;
            settings.moveBack = KeyCode.S;
            settings.moveLeft = KeyCode.A;
            settings.moveRight = KeyCode.D;
            settings.jump = KeyCode.Space;
            settings.sprint = KeyCode.LeftShift;
            settings.Save();
            mainMenu.ShowOptions();
        });
    }

    #endregion

    #region UI CREATION HELPERS

    void AddHeader(Transform parent, string text)
    {
        GameObject obj = new GameObject("Header", typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.minHeight = 32;
        le.preferredHeight = 32;

        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.3f, 0.4f, 0.8f);

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(obj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(15, 0);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.color = Color.cyan;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
    }

    void AddSlider(Transform parent, string label, float min, float max, float value, System.Action<float> onChange)
    {
        GameObject row = CreateRow(parent, 32);

        // Label (left side)
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(row.transform, false);

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.35f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 16;
        labelTmp.color = Color.white;
        labelTmp.alignment = TextAlignmentOptions.MidlineRight;
        labelTmp.raycastTarget = false;

        // Slider background
        GameObject sliderBgObj = new GameObject("SliderBg", typeof(RectTransform));
        sliderBgObj.transform.SetParent(row.transform, false);

        RectTransform sliderBgRect = sliderBgObj.GetComponent<RectTransform>();
        sliderBgRect.anchorMin = new Vector2(0.37f, 0.25f);
        sliderBgRect.anchorMax = new Vector2(0.82f, 0.75f);
        sliderBgRect.offsetMin = Vector2.zero;
        sliderBgRect.offsetMax = Vector2.zero;

        Image sliderBgImg = sliderBgObj.AddComponent<Image>();
        sliderBgImg.color = new Color(0.15f, 0.15f, 0.2f);

        Slider slider = sliderBgObj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;

        // Fill
        GameObject fillArea = new GameObject("FillArea", typeof(RectTransform));
        fillArea.transform.SetParent(sliderBgObj.transform, false);

        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.cyan;

        slider.fillRect = fillRect;

        // Handle
        GameObject handleSlideArea = new GameObject("HandleSlideArea", typeof(RectTransform));
        handleSlideArea.transform.SetParent(sliderBgObj.transform, false);

        RectTransform hsaRect = handleSlideArea.GetComponent<RectTransform>();
        hsaRect.anchorMin = Vector2.zero;
        hsaRect.anchorMax = Vector2.one;
        hsaRect.offsetMin = Vector2.zero;
        hsaRect.offsetMax = Vector2.zero;

        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleSlideArea.transform, false);

        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(18, 0);
        handleRect.anchorMin = new Vector2(0, 0);
        handleRect.anchorMax = new Vector2(0, 1);

        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;

        // Value display
        GameObject valueObj = new GameObject("Value", typeof(RectTransform));
        valueObj.transform.SetParent(row.transform, false);

        RectTransform valueRect = valueObj.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.84f, 0);
        valueRect.anchorMax = new Vector2(1f, 1);
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;

        TextMeshProUGUI valueTmp = valueObj.AddComponent<TextMeshProUGUI>();
        valueTmp.text = value.ToString("F1");
        valueTmp.fontSize = 16;
        valueTmp.color = Color.cyan;
        valueTmp.alignment = TextAlignmentOptions.MidlineLeft;
        valueTmp.raycastTarget = false;

        slider.onValueChanged.AddListener(v =>
        {
            valueTmp.text = v.ToString("F1");
            onChange(v);
        });
    }

    void AddSliderInt(Transform parent, string label, int min, int max, int value, System.Action<int> onChange)
    {
        AddSlider(parent, label, min, max, value, v => onChange(Mathf.RoundToInt(v)));
    }

    void AddToggle(Transform parent, string label, bool value, System.Action<bool> onChange)
    {
        GameObject row = CreateRow(parent, 32);

        // Label
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(row.transform, false);

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.35f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 16;
        labelTmp.color = Color.white;
        labelTmp.alignment = TextAlignmentOptions.MidlineRight;
        labelTmp.raycastTarget = false;

        // Toggle box
        GameObject toggleObj = new GameObject("Toggle", typeof(RectTransform));
        toggleObj.transform.SetParent(row.transform, false);

        RectTransform toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0.37f, 0.15f);
        toggleRect.anchorMax = new Vector2(0.37f, 0.85f);
        toggleRect.sizeDelta = new Vector2(35, 0);
        toggleRect.anchoredPosition = Vector2.zero;

        Image toggleBg = toggleObj.AddComponent<Image>();
        toggleBg.color = new Color(0.15f, 0.15f, 0.2f);

        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = value;
        toggle.targetGraphic = toggleBg;

        // Checkmark
        GameObject checkObj = new GameObject("Checkmark", typeof(RectTransform));
        checkObj.transform.SetParent(toggleObj.transform, false);

        RectTransform checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.15f, 0.15f);
        checkRect.anchorMax = new Vector2(0.85f, 0.85f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;

        Image checkImg = checkObj.AddComponent<Image>();
        checkImg.color = Color.cyan;

        toggle.graphic = checkImg;
        toggle.onValueChanged.AddListener(v => onChange(v));
    }

    void AddDropdown(Transform parent, string label, string[] options, int value, System.Action<int> onChange)
    {
        GameObject row = CreateRow(parent, 35);

        // Label
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(row.transform, false);

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.35f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 16;
        labelTmp.color = Color.white;
        labelTmp.alignment = TextAlignmentOptions.MidlineRight;
        labelTmp.raycastTarget = false;

        // Dropdown
        GameObject ddObj = new GameObject("Dropdown", typeof(RectTransform));
        ddObj.transform.SetParent(row.transform, false);

        RectTransform ddRect = ddObj.GetComponent<RectTransform>();
        ddRect.anchorMin = new Vector2(0.37f, 0.1f);
        ddRect.anchorMax = new Vector2(0.85f, 0.9f);
        ddRect.offsetMin = Vector2.zero;
        ddRect.offsetMax = Vector2.zero;

        Image ddBg = ddObj.AddComponent<Image>();
        ddBg.color = new Color(0.15f, 0.15f, 0.2f);

        TMP_Dropdown dd = ddObj.AddComponent<TMP_Dropdown>();

        // Caption
        GameObject captionObj = new GameObject("Label", typeof(RectTransform));
        captionObj.transform.SetParent(ddObj.transform, false);

        RectTransform captionRect = captionObj.GetComponent<RectTransform>();
        captionRect.anchorMin = Vector2.zero;
        captionRect.anchorMax = Vector2.one;
        captionRect.offsetMin = new Vector2(10, 0);
        captionRect.offsetMax = new Vector2(-25, 0);

        TextMeshProUGUI captionTmp = captionObj.AddComponent<TextMeshProUGUI>();
        captionTmp.fontSize = 16;
        captionTmp.color = Color.white;
        captionTmp.alignment = TextAlignmentOptions.MidlineLeft;
        dd.captionText = captionTmp;

        // Template
        GameObject template = new GameObject("Template", typeof(RectTransform));
        template.transform.SetParent(ddObj.transform, false);

        RectTransform templateRect = template.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.sizeDelta = new Vector2(0, 150);

        Image templateBg = template.AddComponent<Image>();
        templateBg.color = new Color(0.12f, 0.12f, 0.18f);

        ScrollRect templateScroll = template.AddComponent<ScrollRect>();

        // Viewport in template
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(template.transform, false);

        RectTransform vpRect = viewport.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.sizeDelta = Vector2.zero;

        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0, 0, 0, 0);
        Mask vpMask = viewport.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;

        templateScroll.viewport = vpRect;

        // Content in viewport
        GameObject contentObj = new GameObject("Content", typeof(RectTransform));
        contentObj.transform.SetParent(viewport.transform, false);

        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 28);

        templateScroll.content = contentRect;

        // Item template
        GameObject itemObj = new GameObject("Item", typeof(RectTransform));
        itemObj.transform.SetParent(contentObj.transform, false);

        RectTransform itemRect = itemObj.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 28);

        Image itemBg = itemObj.AddComponent<Image>();
        itemBg.color = new Color(0.15f, 0.15f, 0.2f);

        Toggle itemToggle = itemObj.AddComponent<Toggle>();
        itemToggle.targetGraphic = itemBg;

        GameObject itemLabelObj = new GameObject("Item Label", typeof(RectTransform));
        itemLabelObj.transform.SetParent(itemObj.transform, false);

        RectTransform itemLabelRect = itemLabelObj.GetComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(10, 0);
        itemLabelRect.offsetMax = new Vector2(-10, 0);

        TextMeshProUGUI itemTmp = itemLabelObj.AddComponent<TextMeshProUGUI>();
        itemTmp.fontSize = 16;
        itemTmp.color = Color.white;
        itemTmp.alignment = TextAlignmentOptions.MidlineLeft;

        dd.itemText = itemTmp;
        dd.template = templateRect;

        template.SetActive(false);

        dd.ClearOptions();
        dd.AddOptions(new List<string>(options));
        dd.value = Mathf.Clamp(value, 0, options.Length - 1);
        dd.RefreshShownValue();

        dd.onValueChanged.AddListener(v => onChange(v));
    }

    void AddKeyBind(Transform parent, string label, KeyCode currentKey, System.Action<KeyCode> onSet)
    {
        GameObject row = CreateRow(parent, 35);

        // Label
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(row.transform, false);

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.35f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 16;
        labelTmp.color = Color.white;
        labelTmp.alignment = TextAlignmentOptions.MidlineRight;
        labelTmp.raycastTarget = false;

        // Button
        GameObject btnObj = new GameObject("KeyBtn", typeof(RectTransform));
        btnObj.transform.SetParent(row.transform, false);

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.37f, 0.1f);
        btnRect.anchorMax = new Vector2(0.6f, 0.9f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = new Color(0.15f, 0.15f, 0.2f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnBg;

        GameObject btnTextObj = new GameObject("Text", typeof(RectTransform));
        btnTextObj.transform.SetParent(btnObj.transform, false);

        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnTmp.text = currentKey.ToString();
        btnTmp.fontSize = 16;
        btnTmp.color = Color.cyan;
        btnTmp.alignment = TextAlignmentOptions.Center;
        btnTmp.raycastTarget = false;

        btn.onClick.AddListener(() =>
        {
            btnTmp.text = "...";
            waitingForKey = true;
            onKeyPressed = key =>
            {
                btnTmp.text = key.ToString();
                onSet(key);
                settings.Save();
            };
        });
    }

    void AddActionButton(Transform parent, string text, System.Action onClick)
    {
        GameObject obj = new GameObject("ActionBtn", typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.minHeight = 40;
        le.preferredHeight = 40;

        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.3f, 0.4f);

        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = bg;
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.12f, 0.4f, 0.5f);
        colors.pressedColor = Color.cyan;
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick());

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(obj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
    }

    void AddSpacer(Transform parent, float height)
    {
        GameObject obj = new GameObject("Spacer", typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;
    }

    GameObject CreateRow(Transform parent, float height)
    {
        GameObject obj = new GameObject("Row", typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;

        return obj;
    }

    #endregion

    public void RefreshValues()
    {
        settings = GameSettings.Instance;
    }
}
