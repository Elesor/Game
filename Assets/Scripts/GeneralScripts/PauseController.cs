using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static PauseController Instance { get; private set; }
    public static bool IsGamePaused { get; private set; } = false;

    // Новый флаг для UI режима (диалоги, инвентарь и т.д.)
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
        // Устанавливаем стандартное состояние для игры
        SetGameCursorMode();
    }

    public void PauseGame()
    {
        if (IsGamePaused || isPausing) return;

        isPausing = true;

        // Сохраняем состояние курсора
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        SetPause(true);
        DisablePlayerMovement(true);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        // Включаем курсор для меню паузы
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

    // НОВЫЙ МЕТОД: Для диалогов и UI (без паузы времени)
    public void EnableUIMode()
    {
        if (IsUIModeActive) return;

        // Сохраняем текущее состояние игры
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        // Отключаем движение
        DisablePlayerMovement(true);

        // Включаем курсор для UI
        SetUICursorMode();

        IsUIModeActive = true;
        // ВРЕМЯ НЕ ОСТАНАВЛИВАЕМ! Time.timeScale остается 1

        Debug.Log("UI режим активирован (диалог, инвентарь)");
    }

    // НОВЫЙ МЕТОД: Выход из режима UI
    public void DisableUIMode()
    {
        if (!IsUIModeActive) return;

        // Включаем движение обратно
        DisablePlayerMovement(false);

        // Возвращаем игровой режим курсора
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

    // Устанавливает режим курсора для игры (скрытый и заблокированный)
    private void SetGameCursorMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Устанавливает режим курсора для UI (видимый и свободный)
    private void SetUICursorMode()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Опционально: метод для принудительного сброса
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