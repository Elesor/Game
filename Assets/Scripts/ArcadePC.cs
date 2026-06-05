using UnityEngine;

public class ArcadePC : MonoBehaviour
{
    public GameObject rhythmGameCanvas;
    public RhythmGameManager rhythmGameManager;
    public KeyCode interactKey = KeyCode.E;

    private bool isPlayerInRange = false;
    private bool isGameActive = false;

    void Start()
    {
        if (rhythmGameCanvas != null)
            rhythmGameCanvas.SetActive(false);

        if (rhythmGameManager != null)
        {
            rhythmGameManager.OnGameEnd += OnRhythmGameEnd;
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey) && !isGameActive)
        {
            StartRhythmGame();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    void OnDestroy()
    {
        if (rhythmGameManager != null)
        {
            rhythmGameManager.OnGameEnd -= OnRhythmGameEnd;
        }
    }

    public void StartRhythmGame()
    {
        isGameActive = true;

        // Отключаем движение игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Player pm = player.GetComponent<Player>();
            if (pm != null) pm.enabled = false;
        }

        if (rhythmGameCanvas != null)
            rhythmGameCanvas.SetActive(true);

        if (rhythmGameManager != null)
            rhythmGameManager.StartGame();
    }

    void OnRhythmGameEnd(bool win)
    {
        isGameActive = false;

        // Включаем движение игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Player pm = player.GetComponent<Player>();
            if (pm != null) pm.enabled = true;
        }

        if (rhythmGameCanvas != null)
            rhythmGameCanvas.SetActive(false);

        if (win)
        {
            Debug.Log("Ритм-игра пройдена!");
        }
    }
}