using UnityEngine;

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

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        Invoke(nameof(RespawnPlayer), respawnDelay);
    }

    void RespawnPlayer()
    {
        if (player == null) return;

        Vector3 spawnPos = SpawnManager.Instance?.GetPlayerSpawnPosition() ?? Vector3.up;
        player.Respawn(spawnPos);
        uiManager?.HideDeathMessage();
    }
}
