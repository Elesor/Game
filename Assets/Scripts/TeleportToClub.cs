using UnityEngine;
using UnityEngine.UI;

public class TeleportToClub : MonoBehaviour
{
    [Header("Настройки")]
    public Transform teleportDestination;
    public KeyCode interactionKey = KeyCode.E;
    public GameObject promptUI;  // UI панель с текстом "Нажмите E"
    public Text promptText;      // текст подсказки (опционально)

    private bool isPlayerInRange = false;
    private GameObject currentPlayer;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            TeleportPlayer();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            currentPlayer = other.gameObject;

            if (promptUI != null)
                promptUI.SetActive(true);

            Debug.Log("Игрок в зоне телепорта. Нажмите E для перехода в компьютерный клуб");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            currentPlayer = null;

            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    void TeleportPlayer()
    {
        if (currentPlayer != null && teleportDestination != null)
        {
            currentPlayer.transform.position = teleportDestination.position;
            Debug.Log("Игрок телепортирован в компьютерный клуб!");
        }
        else
        {
            Debug.LogError("Телепорт не удался! Проверь teleportDestination");
        }
    }
}