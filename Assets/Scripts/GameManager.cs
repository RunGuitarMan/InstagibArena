using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int playerKills = 0;
    public int playerDeaths = 0;
    public float respawnDelay = 1.5f;

    [Header("References")]
    public PlayerController player;
    public UIManager uiManager;

    public bool IsGamePaused { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        ResumeGame();
    }

    void Update()
    {
        // Pause toggle
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void OnEnemyKilled()
    {
        playerKills++;
        uiManager?.UpdateScore(playerKills, playerDeaths);
    }

    public void OnPlayerDeath()
    {
        playerDeaths++;
        uiManager?.UpdateScore(playerKills, playerDeaths);
        uiManager?.ShowDeathMessage();

        // Respawn player after delay
        Invoke(nameof(RespawnPlayer), respawnDelay);
    }

    void RespawnPlayer()
    {
        if (player == null) return;

        Vector3 spawnPos = SpawnManager.Instance?.GetPlayerSpawnPosition() ?? Vector3.up;
        player.Respawn(spawnPos);
        uiManager?.HideDeathMessage();
    }

    public void TogglePause()
    {
        if (IsGamePaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        IsGamePaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        uiManager?.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        uiManager?.HidePauseMenu();
    }

    public void RestartGame()
    {
        playerKills = 0;
        playerDeaths = 0;
        uiManager?.UpdateScore(0, 0);
        ResumeGame();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
