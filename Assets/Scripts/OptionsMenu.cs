using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OptionsMenu : MonoBehaviour
{
    private Transform container;
    private MainMenu mainMenu;
    private GameObject scrollContent;
    private List<System.Action> refreshActions = new List<System.Action>();

    // References to game components
    private PlayerController playerController;
    private Railgun railgun;
    private ArenaGenerator arenaGenerator;

    // Category buttons
    private int currentCategory = 0;
    private GameObject[] categoryContents;
    private Button[] categoryButtons;

    private string[] categories = { "Movement", "Combat", "Dash", "Mouse", "Arena" };

    public void Initialize(Transform parent, MainMenu menu)
    {
        container = parent;
        mainMenu = menu;

        // Delay to ensure all objects are created
        Invoke("DelayedInit", 0.1f);
    }

    void DelayedInit()
    {
        FindGameComponents();
        CreateOptionsUI();
    }

    void FindGameComponents()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        railgun = FindFirstObjectByType<Railgun>();
        arenaGenerator = FindFirstObjectByType<ArenaGenerator>();

        Debug.Log($"Found: Player={playerController != null}, Railgun={railgun != null}, Arena={arenaGenerator != null}");
    }

    void CreateOptionsUI()
    {
        // Title
        CreateTitle("OPTIONS", container);

        // Back button
        CreateBackButton();

        // Category tabs
        CreateCategoryTabs();

        // Create content for each category
        categoryContents = new GameObject[categories.Length];
        for (int i = 0; i < categories.Length; i++)
        {
            categoryContents[i] = CreateCategoryContent(i);
            categoryContents[i].SetActive(i == 0);
        }

        // Populate categories
        PopulateMovementOptions(categoryContents[0]);
        PopulateCombatOptions(categoryContents[1]);
        PopulateDashOptions(categoryContents[2]);
        PopulateMouseOptions(categoryContents[3]);
        PopulateArenaOptions(categoryContents[4]);
    }

    void CreateTitle(string text, Transform parent)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent);

        TextMeshProUGUI tmp = titleObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 48;
        tmp.color = Color.cyan;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rect = tmp.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.92f);
        rect.anchorMax = new Vector2(0.5f, 0.92f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(400, 60);
    }

    void CreateBackButton()
    {
        GameObject buttonObj = new GameObject("BackButton");
        buttonObj.transform.SetParent(container);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.6f, 0.2f, 0.2f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.8f, 0.3f, 0.3f);
        colors.pressedColor = Color.red;
        button.colors = colors;

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.05f);
        rect.anchorMax = new Vector2(0.5f, 0.05f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(200, 50);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "BACK";
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = tmp.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        button.onClick.AddListener(() => mainMenu.ShowMainMenu());
    }

    void CreateCategoryTabs()
    {
        GameObject tabContainer = new GameObject("TabContainer");
        tabContainer.transform.SetParent(container);

        RectTransform tabRect = tabContainer.AddComponent<RectTransform>();
        tabRect.anchorMin = new Vector2(0.1f, 0.82f);
        tabRect.anchorMax = new Vector2(0.9f, 0.88f);
        tabRect.offsetMin = Vector2.zero;
        tabRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup layout = tabContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        categoryButtons = new Button[categories.Length];

        for (int i = 0; i < categories.Length; i++)
        {
            int index = i;
            categoryButtons[i] = CreateTabButton(tabContainer.transform, categories[i], () => SelectCategory(index));
        }

        UpdateTabColors();
    }

    Button CreateTabButton(Transform parent, string text, System.Action onClick)
    {
        GameObject buttonObj = new GameObject(text + "Tab");
        buttonObj.transform.SetParent(parent);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.25f);

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => onClick());

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = tmp.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    void SelectCategory(int index)
    {
        currentCategory = index;

        for (int i = 0; i < categoryContents.Length; i++)
        {
            categoryContents[i].SetActive(i == index);
        }

        UpdateTabColors();
    }

    void UpdateTabColors()
    {
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            Image img = categoryButtons[i].GetComponent<Image>();
            img.color = i == currentCategory ?
                new Color(0.1f, 0.5f, 0.7f) :
                new Color(0.2f, 0.2f, 0.25f);
        }
    }

    GameObject CreateCategoryContent(int index)
    {
        GameObject content = new GameObject(categories[index] + "Content");
        content.transform.SetParent(container);

        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.12f);
        contentRect.anchorMax = new Vector2(0.9f, 0.8f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        // Scroll view
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(content.transform);

        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;

        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        Image scrollBg = scrollView.AddComponent<Image>();
        scrollBg.color = new Color(0.1f, 0.1f, 0.15f, 0.5f);

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform);

        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(10, 10);
        viewportRect.offsetMax = new Vector2(-10, -10);

        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content container
        GameObject scrollContent = new GameObject("Content");
        scrollContent.transform.SetParent(viewport.transform);

        RectTransform scrollContentRect = scrollContent.AddComponent<RectTransform>();
        scrollContentRect.anchorMin = new Vector2(0, 1);
        scrollContentRect.anchorMax = new Vector2(1, 1);
        scrollContentRect.pivot = new Vector2(0.5f, 1);
        scrollContentRect.anchoredPosition = Vector2.zero;
        scrollContentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup vlg = scrollContent.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15;
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = scrollContent.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = scrollContentRect;

        // Store reference for later
        content.GetComponent<RectTransform>().name = categories[index] + "Content";

        return scrollContent;
    }

    void PopulateMovementOptions(GameObject content)
    {
        if (playerController == null) return;

        CreateSlider(content.transform, "Move Speed", 5f, 30f, playerController.moveSpeed,
            v => playerController.moveSpeed = v);

        CreateSlider(content.transform, "Sprint Multiplier", 1f, 3f, playerController.sprintMultiplier,
            v => playerController.sprintMultiplier = v);

        CreateSlider(content.transform, "Jump Force", 4f, 20f, playerController.jumpForce,
            v => playerController.jumpForce = v);

        CreateSlider(content.transform, "Gravity", 10f, 40f, playerController.gravity,
            v => playerController.gravity = v);

        CreateSlider(content.transform, "Air Acceleration", 20f, 200f, playerController.airAcceleration,
            v => playerController.airAcceleration = v);

        CreateSlider(content.transform, "Air Max Speed", 0.5f, 3f, playerController.airMaxSpeed,
            v => playerController.airMaxSpeed = v);

        CreateSlider(content.transform, "Ground Friction", 1f, 15f, playerController.groundFriction,
            v => playerController.groundFriction = v);

        CreateSlider(content.transform, "Max Bunny Speed", 15f, 60f, playerController.maxBunnySpeed,
            v => playerController.maxBunnySpeed = v);

        CreateSlider(content.transform, "Fall Gravity Multiplier", 1f, 5f, playerController.fallGravityMultiplier,
            v => playerController.fallGravityMultiplier = v);

        CreateSlider(content.transform, "Fall Air Control", 0f, 1f, playerController.fallAirControlMultiplier,
            v => playerController.fallAirControlMultiplier = v);
    }

    void PopulateCombatOptions(GameObject content)
    {
        if (railgun == null) return;

        CreateSlider(content.transform, "Railgun Cooldown", 0.5f, 5f, railgun.cooldown,
            v => railgun.cooldown = v);

        CreateSlider(content.transform, "Railgun Range", 100f, 1000f, railgun.range,
            v => railgun.range = v);

        CreateSlider(content.transform, "Rail Jump Radius", 1f, 15f, railgun.railJumpRadius,
            v => railgun.railJumpRadius = v);

        CreateSlider(content.transform, "Rail Jump Force", 1f, 30f, railgun.railJumpForce,
            v => railgun.railJumpForce = v);

        CreateSlider(content.transform, "Rail Jump Up Force", 1f, 20f, railgun.railJumpBaseUpForce,
            v => railgun.railJumpBaseUpForce = v);

        CreateSlider(content.transform, "Rail Jump Max Height", 0.5f, 5f, railgun.maxEffectiveHeight,
            v => railgun.maxEffectiveHeight = v);

        CreateSlider(content.transform, "Jump Combo Bonus", 1f, 2f, railgun.jumpComboBonus,
            v => railgun.jumpComboBonus = v);

        CreateSlider(content.transform, "Beam Duration", 0.05f, 0.5f, railgun.beamDuration,
            v => railgun.beamDuration = v);

        CreateSlider(content.transform, "Beam Width", 0.02f, 0.3f, railgun.beamWidth,
            v => railgun.beamWidth = v);
    }

    void PopulateDashOptions(GameObject content)
    {
        if (playerController == null) return;

        CreateSlider(content.transform, "Dash Distance", 3f, 20f, playerController.dashDistance,
            v => playerController.dashDistance = v);

        CreateSlider(content.transform, "Dash Duration", 0.05f, 0.3f, playerController.dashDuration,
            v => playerController.dashDuration = v);

        CreateSlider(content.transform, "Dash Cooldown", 0.2f, 3f, playerController.dashCooldown,
            v => playerController.dashCooldown = v);

        CreateSlider(content.transform, "Double Tap Window", 0.1f, 0.5f, playerController.doubleTapWindow,
            v => playerController.doubleTapWindow = v);
    }

    void PopulateMouseOptions(GameObject content)
    {
        if (playerController == null) return;

        CreateSlider(content.transform, "Mouse Sensitivity", 0.5f, 10f, playerController.mouseSensitivity,
            v => playerController.mouseSensitivity = v);

        CreateSlider(content.transform, "Max Look Angle", 45f, 90f, playerController.maxLookAngle,
            v => playerController.maxLookAngle = v);
    }

    void PopulateArenaOptions(GameObject content)
    {
        if (arenaGenerator == null) return;

        CreateLabel(content.transform, "Arena settings require restart to apply", Color.yellow);

        CreateSlider(content.transform, "Arena Width", 40f, 200f, arenaGenerator.arenaWidth,
            v => arenaGenerator.arenaWidth = v);

        CreateSlider(content.transform, "Arena Length", 40f, 200f, arenaGenerator.arenaLength,
            v => arenaGenerator.arenaLength = v);

        CreateSlider(content.transform, "Number of Pillars", 0f, 30f, arenaGenerator.numberOfPillars,
            v => arenaGenerator.numberOfPillars = (int)v, true);

        CreateSlider(content.transform, "Number of Platforms", 0f, 20f, arenaGenerator.numberOfPlatforms,
            v => arenaGenerator.numberOfPlatforms = (int)v, true);

        CreateSlider(content.transform, "Number of Ramps", 0f, 15f, arenaGenerator.numberOfRamps,
            v => arenaGenerator.numberOfRamps = (int)v, true);

        CreateSlider(content.transform, "Number of Towers", 0f, 8f, arenaGenerator.numberOfTowers,
            v => arenaGenerator.numberOfTowers = (int)v, true);

        CreateSlider(content.transform, "Number of Bridges", 0f, 10f, arenaGenerator.numberOfBridges,
            v => arenaGenerator.numberOfBridges = (int)v, true);
    }

    void CreateLabel(Transform parent, string text, Color color)
    {
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(parent);

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;

        LayoutElement le = labelObj.AddComponent<LayoutElement>();
        le.minHeight = 30;
        le.preferredHeight = 30;
    }

    void CreateSlider(Transform parent, string label, float min, float max, float defaultValue,
        System.Action<float> onValueChanged, bool wholeNumbers = false)
    {
        GameObject sliderContainer = new GameObject(label + "Slider");
        sliderContainer.transform.SetParent(parent);

        RectTransform containerRect = sliderContainer.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(0, 50);

        LayoutElement le = sliderContainer.AddComponent<LayoutElement>();
        le.minHeight = 50;
        le.preferredHeight = 50;

        HorizontalLayoutGroup hlg = sliderContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(sliderContainer.transform);
        TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 16;
        labelTmp.color = Color.white;
        labelTmp.alignment = TextAlignmentOptions.MidlineRight;

        LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
        labelLe.minWidth = 200;
        labelLe.preferredWidth = 200;

        // Slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(sliderContainer.transform);

        Image sliderBg = sliderObj.AddComponent<Image>();
        sliderBg.color = new Color(0.2f, 0.2f, 0.25f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = defaultValue;
        slider.wholeNumbers = wholeNumbers;

        LayoutElement sliderLe = sliderObj.AddComponent<LayoutElement>();
        sliderLe.minWidth = 300;
        sliderLe.preferredWidth = 300;

        // Fill area
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(sliderObj.transform);

        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-5, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.cyan;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;

        // Handle
        GameObject handleArea = new GameObject("HandleArea");
        handleArea.transform.SetParent(sliderObj.transform);

        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(5, 0);
        handleAreaRect.offsetMax = new Vector2(-5, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform);

        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;

        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);

        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;

        // Value display
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(sliderContainer.transform);
        TextMeshProUGUI valueTmp = valueObj.AddComponent<TextMeshProUGUI>();
        valueTmp.text = wholeNumbers ? defaultValue.ToString("0") : defaultValue.ToString("0.00");
        valueTmp.fontSize = 16;
        valueTmp.color = Color.cyan;
        valueTmp.alignment = TextAlignmentOptions.MidlineLeft;

        LayoutElement valueLe = valueObj.AddComponent<LayoutElement>();
        valueLe.minWidth = 80;
        valueLe.preferredWidth = 80;

        // Slider event
        slider.onValueChanged.AddListener(v =>
        {
            valueTmp.text = wholeNumbers ? v.ToString("0") : v.ToString("0.00");
            onValueChanged(v);
        });

        // Store refresh action
        refreshActions.Add(() =>
        {
            // This would need to re-read the value from the component
        });
    }

    public void RefreshValues()
    {
        // Re-find components in case they changed
        FindGameComponents();
    }
}
