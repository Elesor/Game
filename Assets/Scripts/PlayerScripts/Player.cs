using System.Runtime.CompilerServices;
using UnityEngine;

public class Player:MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private float movingSpeed = 5f;

    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;

    private Rigidbody2D rb;
    
    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

    }

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

    public bool IsRunning()
    {
        return isRunning;
    }

    public Vector3 GetPlayerScreenPosition()
    {
        Vector3 playerSreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        return playerSreenPosition;
    }
}
