using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private InputSystem_Actions inputActions;  // Изменено с PlayerInputActions

    public event System.Action OnInteractPerformed;

    private void Awake()
    {
        Instance = this;
        inputActions = new InputSystem_Actions();  // Изменено
        inputActions.Enable();

        // Подписка на действие Interact
        inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.Interact.performed -= OnInteract;
            inputActions.Disable();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        OnInteractPerformed?.Invoke();
    }

    public Vector2 GetMovementVector()
    {
        return inputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector3 GetMousePosition()
    {
        return Mouse.current.position.ReadValue();
    }
}