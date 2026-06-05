using UnityEngine;

public class ArcadePC : MonoBehaviour, IInteractable
{
    public GameObject rhythmGameCanvas;
    public RhythmGameManager rhythmGameManager;

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

    void OnDestroy()
    {
        if (rhythmGameManager != null)
        {
            rhythmGameManager.OnGameEnd -= OnRhythmGameEnd;
        }
    }

    public bool CanInteract()
    {
        return !isGameActive;
    }

    public void Interact()
    {
        if (!isGameActive)
        {
            StartRhythmGame();
        }
    }

    public void StartRhythmGame()
    {
        isGameActive = true;

        if (rhythmGameCanvas != null)
            rhythmGameCanvas.SetActive(true);

        if (rhythmGameManager != null)
            rhythmGameManager.StartGame();
    }

    void OnRhythmGameEnd(bool win)
    {
        Debug.Log("=== OnRhythmGameEnd вызван, win = " + win + " ===");
        isGameActive = false;

        // НЕ ВЫКЛЮЧАЕМ CANVAS СРАЗУ!
        // rhythmGameCanvas.SetActive(false); // ← ЗАКОММЕНТИРОВАТЬ ИЛИ УДАЛИТЬ

        if (win)
        {
            Debug.Log("Ритм-игра пройдена! Можно разблокировать награду");
        }
    }

    public void CloseGame()
    {
        if (rhythmGameCanvas != null)
            rhythmGameCanvas.SetActive(false);
    }
}