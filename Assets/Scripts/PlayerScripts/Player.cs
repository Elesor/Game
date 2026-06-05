using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Компонент игрока, управляющий его движением и состоянием.
/// Реализует паттерн Singleton для доступа из других скриптов.
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>
    /// Синглтон экземпляр игрока. Доступен глобально из любого скрипта.
    /// </summary>
    public static Player Instance { get; private set; }

    [SerializeField] private float movingSpeed = 5f;
    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;
    private Rigidbody2D rb;

    /// <summary>
    /// Вызывается при создании объекта. Инициализирует синглтон и получает компонент Rigidbody2D.
    /// </summary>
    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Вызывается каждый фиксированный кадр (по умолчанию 50 раз в секунду).
    /// Обрабатывает движение игрока и приостанавливает физику при паузе.
    /// </summary>
    private void FixedUpdate()
    {
        HandleMovement();
        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
    }

    /// <summary>
    /// Обрабатывает ввод с клавиатуры и перемещает игрока.
    /// Нормализует вектор ввода для стабильной скорости по диагонали.
    /// Обновляет состояние isRunning на основе интенсивности ввода.
    /// </summary>
    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVector();
        inputVector = inputVector.normalized;
        rb.MovePosition(rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime));

        if (Mathf.Abs(inputVector.x) > minMovingSpeed || Mathf.Abs(inputVector.y) > minMovingSpeed)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
    }

    /// <summary>
    /// Возвращает состояние бега игрока.
    /// </summary>
    /// <returns>true - если игрок движется, false - если игрок стоит на месте</returns>
    public bool IsRunning()
    {
        return isRunning;
    }

    /// <summary>
    /// Получает позицию игрока в экранных координатах.
    /// Полезно для UI элементов, которые должны следовать за игроком.
    /// </summary>
    /// <returns>Позиция игрока в пикселях относительно левого нижнего угла экрана</returns>
    public Vector3 GetPlayerScreenPosition()
    {
        Vector3 playerSreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        return playerSreenPosition;
    }
}