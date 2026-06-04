using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static PauseController Instance { get; private set; }
    public static bool IsGamePaused { get; private set; } = false;
    public static bool IsUIModeActive { get; private set; } = false;

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

    private void Start()
    {
        // По умолчанию - игровой режим (курсор скрыт и заблокирован)
        SetGameCursorMode();
    }

    /// <summary>
    /// Игровой режим курсора: скрыт и заблокирован в центре экрана
    /// </summary>
    private void SetGameCursorMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// UI режим курсора: видимый и свободный
    /// </summary>
    private void SetUICursorMode()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PauseGame()
    {
        if (IsGamePaused || isPausing) return;

        isPausing = true;

        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        SetPause(true);
        DisablePlayerMovement(true);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        SetUICursorMode();
        IsUIModeActive = true;

        Debug.Log("Игра на паузе, UI режим активен");
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

        // Возвращаем игровой режим курсора
        SetGameCursorMode();
        IsUIModeActive = false;

        Debug.Log("Игра продолжена, игровой режим курсора");
        isPausing = false;
    }

    /// <summary>
    /// Включение UI режима (диалоги, инвентарь) без паузы времени
    /// </summary>
    public void EnableUIMode()
    {
        if (IsUIModeActive) return;

        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        DisablePlayerMovement(true);
        SetUICursorMode();

        IsUIModeActive = true;

        Debug.Log("UI режим активирован (диалог, инвентарь)");
    }

    /// <summary>
    /// Выключение UI режима
    /// </summary>
    public void DisableUIMode()
    {
        if (!IsUIModeActive) return;

        DisablePlayerMovement(false);
        SetGameCursorMode();

        IsUIModeActive = false;

        Debug.Log("UI режим деактивирован");
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

    public void ResetToGameMode()
    {
        if (!IsGamePaused && !IsUIModeActive)
        {
            SetGameCursorMode();
        }
        else if (IsUIModeActive)
        {
            SetUICursorMode();
        }
    }
}