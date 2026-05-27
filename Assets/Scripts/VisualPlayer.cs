using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVisual : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private const string IS_RUNNING = "IsRunning";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
        ClickReverse();
    }

    private void ClickReverse()
    {
        float horizontalInput = Keyboard.current.aKey.isPressed ? 1f : Keyboard.current.dKey.isPressed ? -1f : 0f;
        if (horizontalInput > 0) { spriteRenderer.flipX = true; }
        else if (horizontalInput < 0) { spriteRenderer.flipX = false; }
    }
}
