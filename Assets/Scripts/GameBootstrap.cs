using UnityEngine;

/// <summary>
/// Main bootstrap script - creates all game objects at runtime.
/// Just add this to an empty GameObject in your scene and press Play!
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Game Settings")]
    public int enemyCount = 5;
    public float enemyAccuracy = 0.08f;
    public bool isGameScene = true;

    void Awake()
    {
        // Apply video settings
        GameSettings.Instance.ApplyVideoSettings();

        if (isGameScene)
        {
            InitializeGame();
        }
    }

    void InitializeGame()
    {
        // Create managers first
        CreateGameManager();
        CreateSpawnManager();

        // Generate arena with saved settings
        CreateArena();

        // Create player with saved settings
        CreatePlayer();

        // Create UI
        CreateUI();

        Debug.Log("=== INSTAGIB ARENA LOADED ===");
        Debug.Log("Controls: WASD - Move, Mouse - Look, LMB - Fire, Space - Jump, Double-SHIFT - Dash, Esc - Pause");
    }

    void CreateGameManager()
    {
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();
    }

    void CreateSpawnManager()
    {
        GameObject smObj = new GameObject("SpawnManager");
        SpawnManager sm = smObj.AddComponent<SpawnManager>();
        sm.maxEnemies = enemyCount;
    }

    void CreateArena()
    {
        GameSettings settings = GameSettings.Instance;

        GameObject arenaObj = new GameObject("Arena");
        ArenaGenerator arena = arenaObj.AddComponent<ArenaGenerator>();

        // Apply saved arena settings
        settings.ApplyToArena(arena);
    }

    void CreatePlayer()
    {
        GameSettings settings = GameSettings.Instance;

        // Player root
        GameObject playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.layer = LayerMask.NameToLayer("Default");
        playerObj.transform.position = new Vector3(0, 1f, 0);

        // Character Controller
        CharacterController cc = playerObj.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = Vector3.up;

        // Player Controller
        PlayerController pc = playerObj.AddComponent<PlayerController>();

        // Apply saved player settings
        settings.ApplyToPlayer(pc);

        // Camera holder (for vertical look)
        GameObject cameraHolder = new GameObject("CameraHolder");
        cameraHolder.transform.SetParent(playerObj.transform);
        cameraHolder.transform.localPosition = new Vector3(0, 1.6f, 0);

        // Main Camera
        Camera existingCamera = Camera.main;
        if (existingCamera != null)
        {
            existingCamera.transform.SetParent(cameraHolder.transform);
            existingCamera.transform.localPosition = Vector3.zero;
            existingCamera.transform.localRotation = Quaternion.identity;
        }
        else
        {
            GameObject cameraObj = new GameObject("MainCamera");
            cameraObj.tag = "MainCamera";
            cameraObj.transform.SetParent(cameraHolder.transform);
            cameraObj.transform.localPosition = Vector3.zero;
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.fieldOfView = 90f;
            cam.nearClipPlane = 0.1f;
            cameraObj.AddComponent<AudioListener>();

            // Add URP camera data for post-processing
            var cameraData = cameraObj.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
        }

        pc.cameraHolder = cameraHolder.transform;

        // Railgun
        GameObject railgunObj = new GameObject("Railgun");
        railgunObj.transform.SetParent(playerObj.transform);
        Railgun railgun = railgunObj.AddComponent<Railgun>();

        // Apply saved railgun settings
        settings.ApplyToRailgun(railgun);

        // Register player with managers
        SpawnManager sm = FindFirstObjectByType<SpawnManager>();
        if (sm != null)
        {
            sm.player = playerObj.transform;
        }

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.player = pc;
        }
    }

    void CreateUI()
    {
        GameObject uiObj = new GameObject("UI");
        Canvas canvas = uiObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        UnityEngine.UI.CanvasScaler scaler = uiObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        uiObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        UIManager uiManager = uiObj.AddComponent<UIManager>();

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.uiManager = uiManager;
        }
    }
}
