using UnityEngine;

/// <summary>
/// Bootstrap for the main menu scene.
/// Creates the menu with animated background.
/// </summary>
public class MenuBootstrap : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "Game";

    void Awake()
    {
        // Apply video settings
        GameSettings.Instance.ApplyVideoSettings();
    }

    void Start()
    {
        CreateMainMenu();
    }

    void CreateMainMenu()
    {
        GameObject menuObj = new GameObject("MainMenu");
        MainMenu menu = menuObj.AddComponent<MainMenu>();
        menu.isMenuScene = true;
        menu.gameSceneName = gameSceneName;
    }
}
