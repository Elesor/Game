using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static PauseController Instance { get; private set; }
    public static bool IsGamePaused { get; private set; } = false;

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private MonoBehaviour[] additionalScriptsToDisable;

    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisibility;
    private bool isPausing = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PauseGame()
    {
        if (IsGamePaused || isPausing) return;

        isPausing = true;
        SetPause(true);
        DisablePlayerMovement(true);
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Игра на паузе, движение отключено");
        isPausing = false;
    }

    public void ResumeGame()
    {
        if (!IsGamePaused || isPausing) return;

        isPausing = true;

        SetPause(false);
        DisablePlayerMovement(false);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisibility;

        Debug.Log("Игра продолжена, движение включено");
        isPausing = false;
    }

    private void DisablePlayerMovement(bool disable)
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = !disable;

        foreach (var script in additionalScriptsToDisable)
        {
            if (script != null)
                script.enabled = !disable;
        }
    }

    public static void SetPause(bool pause)
    {
        if (IsGamePaused == pause) return;

        IsGamePaused = pause;
        Time.timeScale = pause ? 0f : 1f;
    }
}