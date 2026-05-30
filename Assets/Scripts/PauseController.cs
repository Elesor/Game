using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; } = false;

    [SerializeField] private GameObject pauseMenuUI;

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsGamePaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;

        if (pause)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        SetPause(true);
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    public void ResumeGame()
    {
        SetPause(false);
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }
}