using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GameSettings
{
    private static GameSettings _instance;
    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Load();
            }
            return _instance;
        }
    }

    // GAME - Movement
    public float moveSpeed = 12f;
    public float sprintMultiplier = 1.5f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public float airAcceleration = 100f;
    public float airMaxSpeed = 1.5f;
    public float groundFriction = 6f;
    public float maxBunnySpeed = 30f;
    public float fallGravityMultiplier = 2.5f;
    public float fallAirControlMultiplier = 0.1f;

    // GAME - Dash
    public float dashDistance = 8f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 1f;
    public float doubleTapWindow = 0.3f;

    // GAME - Combat
    public float railgunCooldown = 1.5f;
    public float railgunRange = 500f;
    public float railJumpRadius = 5f;
    public float railJumpForce = 10f;
    public float railJumpUpForce = 6f;
    public float maxEffectiveHeight = 1.5f;
    public float jumpComboBonus = 1.3f;
    public float beamDuration = 0.15f;
    public float beamWidth = 0.08f;

    // GAME - Arena
    public float arenaWidth = 100f;
    public float arenaLength = 100f;
    public int numberOfPillars = 12;
    public int numberOfPlatforms = 8;
    public int numberOfRamps = 6;
    public int numberOfTowers = 4;
    public int numberOfBridges = 3;

    // VIDEO
    public int resolutionIndex = -1; // -1 = native
    public bool fullscreen = true;
    public int vsync = 1;
    public int qualityLevel = 2;
    public int targetFrameRate = -1; // -1 = unlimited

    // INPUT
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 89f;
    public bool invertMouseY = false;

    // Key bindings
    public KeyCode moveForward = KeyCode.W;
    public KeyCode moveBack = KeyCode.S;
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode jump = KeyCode.Space;
    public KeyCode sprint = KeyCode.LeftShift;
    public KeyCode fire = KeyCode.Mouse0;

    public void Save()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("GameSettings", json);
        PlayerPrefs.Save();
    }

    public static GameSettings Load()
    {
        if (PlayerPrefs.HasKey("GameSettings"))
        {
            string json = PlayerPrefs.GetString("GameSettings");
            return JsonUtility.FromJson<GameSettings>(json);
        }
        return new GameSettings();
    }

    public void ApplyVideoSettings()
    {
        // Resolution
        if (resolutionIndex >= 0 && resolutionIndex < Screen.resolutions.Length)
        {
            Resolution res = Screen.resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, fullscreen);
        }
        else
        {
            Screen.fullScreen = fullscreen;
        }

        // VSync
        QualitySettings.vSyncCount = vsync;

        // Quality
        if (qualityLevel >= 0 && qualityLevel < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(qualityLevel);
        }

        // Frame rate
        Application.targetFrameRate = targetFrameRate;
    }

    public void ApplyToPlayer(PlayerController player)
    {
        if (player == null) return;

        player.moveSpeed = moveSpeed;
        player.sprintMultiplier = sprintMultiplier;
        player.jumpForce = jumpForce;
        player.gravity = gravity;
        player.airAcceleration = airAcceleration;
        player.airMaxSpeed = airMaxSpeed;
        player.groundFriction = groundFriction;
        player.maxBunnySpeed = maxBunnySpeed;
        player.fallGravityMultiplier = fallGravityMultiplier;
        player.fallAirControlMultiplier = fallAirControlMultiplier;
        player.dashDistance = dashDistance;
        player.dashDuration = dashDuration;
        player.dashCooldown = dashCooldown;
        player.doubleTapWindow = doubleTapWindow;
        player.mouseSensitivity = mouseSensitivity;
        player.maxLookAngle = maxLookAngle;
    }

    public void ApplyToRailgun(Railgun railgun)
    {
        if (railgun == null) return;

        railgun.cooldown = railgunCooldown;
        railgun.range = railgunRange;
        railgun.railJumpRadius = railJumpRadius;
        railgun.railJumpForce = railJumpForce;
        railgun.railJumpBaseUpForce = railJumpUpForce;
        railgun.maxEffectiveHeight = maxEffectiveHeight;
        railgun.jumpComboBonus = jumpComboBonus;
        railgun.beamDuration = beamDuration;
        railgun.beamWidth = beamWidth;
    }

    public void ApplyToArena(ArenaGenerator arena)
    {
        if (arena == null) return;

        arena.arenaWidth = arenaWidth;
        arena.arenaLength = arenaLength;
        arena.numberOfPillars = numberOfPillars;
        arena.numberOfPlatforms = numberOfPlatforms;
        arena.numberOfRamps = numberOfRamps;
        arena.numberOfTowers = numberOfTowers;
        arena.numberOfBridges = numberOfBridges;
    }

    public void ResetToDefaults()
    {
        var defaults = new GameSettings();

        // Copy all values
        moveSpeed = defaults.moveSpeed;
        sprintMultiplier = defaults.sprintMultiplier;
        jumpForce = defaults.jumpForce;
        gravity = defaults.gravity;
        airAcceleration = defaults.airAcceleration;
        airMaxSpeed = defaults.airMaxSpeed;
        groundFriction = defaults.groundFriction;
        maxBunnySpeed = defaults.maxBunnySpeed;
        fallGravityMultiplier = defaults.fallGravityMultiplier;
        fallAirControlMultiplier = defaults.fallAirControlMultiplier;
        dashDistance = defaults.dashDistance;
        dashDuration = defaults.dashDuration;
        dashCooldown = defaults.dashCooldown;
        doubleTapWindow = defaults.doubleTapWindow;
        railgunCooldown = defaults.railgunCooldown;
        railgunRange = defaults.railgunRange;
        railJumpRadius = defaults.railJumpRadius;
        railJumpForce = defaults.railJumpForce;
        railJumpUpForce = defaults.railJumpUpForce;
        maxEffectiveHeight = defaults.maxEffectiveHeight;
        jumpComboBonus = defaults.jumpComboBonus;
        beamDuration = defaults.beamDuration;
        beamWidth = defaults.beamWidth;
        arenaWidth = defaults.arenaWidth;
        arenaLength = defaults.arenaLength;
        numberOfPillars = defaults.numberOfPillars;
        numberOfPlatforms = defaults.numberOfPlatforms;
        numberOfRamps = defaults.numberOfRamps;
        numberOfTowers = defaults.numberOfTowers;
        numberOfBridges = defaults.numberOfBridges;
        mouseSensitivity = defaults.mouseSensitivity;
        maxLookAngle = defaults.maxLookAngle;
        invertMouseY = defaults.invertMouseY;
    }
}
