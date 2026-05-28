using Unity.VisualScripting;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public bool isOpen = false;
    public Sprite openDoorSprite;
    public Sprite closedDoorSprite;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D wallCollider;
    private BoxCollider2D triggerZone;
    private bool playerInRange = false;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        BoxCollider2D[] allColliders = GetComponentsInChildren<BoxCollider2D>();
        foreach (var col in allColliders)
        {
            if (col.isTrigger)
                triggerZone = col;      // Это зона взаимодействия
            else
                wallCollider = col;      // Это физическая стена
        }

        if (!isOpen)
        {
            spriteRenderer.sprite = closedDoorSprite;
            if (wallCollider != null) wallCollider.enabled = true;
        }
        else
        {
            spriteRenderer.sprite = openDoorSprite;
            if (wallCollider != null) wallCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        if (isOpen)
        {
            spriteRenderer.sprite = openDoorSprite;
            wallCollider.enabled = false;

        }
        else
        {
            spriteRenderer.sprite = closedDoorSprite;
            wallCollider.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

}
