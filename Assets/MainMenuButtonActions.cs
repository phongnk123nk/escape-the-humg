using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gan chuc nang cho cac nut trong scene main menu.
/// Nut "play" vao scene room1, nut "quit" thoat game, nut "donate" tam thoi de trong.
/// </summary>
public class MainMenuButtonActions : MonoBehaviour
{
    public string mainMenuSceneName = "main menu";
    public string firstGameSceneName = "room1";
    public string playButtonName = "play";
    public string quitButtonName = "quit";

    private static MainMenuButtonActions instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnGameStart()
    {
        if (instance != null) return;

        GameObject obj = new GameObject("MainMenuButtonActions");
        DontDestroyOnLoad(obj);
        instance = obj.AddComponent<MainMenuButtonActions>();
        instance.BindIfMainMenu();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindIfMainMenu();
    }

    private void BindIfMainMenu()
    {
        if (SceneManager.GetActiveScene().name != mainMenuSceneName)
            return;

        Button playButton = FindButtonByName(playButtonName);
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayGame);
            playButton.onClick.AddListener(PlayGame);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy nút play trong main menu.");
        }

        Button quitButton = FindButtonByName(quitButtonName);
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
            quitButton.onClick.AddListener(QuitGame);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy nút quit trong main menu.");
        }
    }

    public void PlayGame()
    {
        if (string.IsNullOrWhiteSpace(firstGameSceneName))
            return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(firstGameSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static Button FindButtonByName(string buttonName)
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].name == buttonName)
                return buttons[i];
        }

        return null;
    }
}
