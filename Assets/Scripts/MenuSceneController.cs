using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MenuSceneController : MonoBehaviour
{
    [Header("Camera Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float pathRadius = 30f;
    public float pathHeight = 15f;
    public float heightVariation = 5f;

    [Header("Look Settings")]
    public Vector3 lookTarget = Vector3.zero;
    public float lookSmoothness = 2f;

    [Header("Blur Settings")]
    public float blurIntensity = 0.5f;

    private Camera menuCamera;
    private float pathAngle = 0f;
    private Volume postProcessVolume;
    private DepthOfField depthOfField;

    void Start()
    {
        SetupCamera();
        SetupArena();
        SetupPostProcessing();
    }

    void SetupCamera()
    {
        // Create camera
        GameObject camObj = new GameObject("MenuCamera");
        menuCamera = camObj.AddComponent<Camera>();
        menuCamera.clearFlags = CameraClearFlags.SolidColor;
        menuCamera.backgroundColor = new Color(0.02f, 0.02f, 0.05f);
        menuCamera.fieldOfView = 60f;
        menuCamera.nearClipPlane = 0.1f;
        menuCamera.farClipPlane = 500f;

        // Add URP camera data
        var cameraData = camObj.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = true;

        // Initial position
        UpdateCameraPosition();
    }

    void SetupArena()
    {
        // Create a simple arena for the background
        // Using ArenaGenerator settings from GameSettings
        GameObject arenaObj = new GameObject("MenuArena");
        ArenaGenerator arena = arenaObj.AddComponent<ArenaGenerator>();

        // Apply saved settings
        GameSettings.Instance.ApplyToArena(arena);

        // Generate arena
        arena.GenerateArena();
    }

    void SetupPostProcessing()
    {
        // Create post-processing volume
        GameObject volumeObj = new GameObject("PostProcessVolume");
        volumeObj.transform.SetParent(transform);

        postProcessVolume = volumeObj.AddComponent<Volume>();
        postProcessVolume.isGlobal = true;
        postProcessVolume.priority = 100;

        // Create profile
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        postProcessVolume.profile = profile;

        // Add depth of field for blur
        depthOfField = profile.Add<DepthOfField>(true);
        depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
        depthOfField.gaussianStart.Override(0f);
        depthOfField.gaussianEnd.Override(50f);
        depthOfField.gaussianMaxRadius.Override(1.5f);
        depthOfField.highQualitySampling.Override(true);

        // Add color adjustments for mood
        var colorAdjust = profile.Add<ColorAdjustments>(true);
        colorAdjust.saturation.Override(-20f);
        colorAdjust.contrast.Override(10f);

        // Add vignette
        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.4f);
        vignette.smoothness.Override(0.5f);
        vignette.color.Override(new Color(0, 0.1f, 0.2f));

        // Add bloom for glow effect
        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(0.8f);
        bloom.intensity.Override(1f);
        bloom.tint.Override(new Color(0.5f, 0.8f, 1f));
    }

    void Update()
    {
        // Move camera along a circular path
        pathAngle += moveSpeed * Time.unscaledDeltaTime;
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (menuCamera == null) return;

        // Circular path with height variation
        float x = Mathf.Cos(pathAngle * Mathf.Deg2Rad) * pathRadius;
        float z = Mathf.Sin(pathAngle * Mathf.Deg2Rad) * pathRadius;
        float y = pathHeight + Mathf.Sin(pathAngle * 0.5f * Mathf.Deg2Rad) * heightVariation;

        Vector3 targetPosition = new Vector3(x, y, z);
        menuCamera.transform.position = Vector3.Lerp(
            menuCamera.transform.position,
            targetPosition,
            Time.unscaledDeltaTime * 2f
        );

        // Look at center
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - menuCamera.transform.position);
        menuCamera.transform.rotation = Quaternion.Slerp(
            menuCamera.transform.rotation,
            targetRotation,
            Time.unscaledDeltaTime * lookSmoothness
        );
    }

    void OnDestroy()
    {
        // Clean up
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            DestroyImmediate(postProcessVolume.profile);
        }
    }
}
