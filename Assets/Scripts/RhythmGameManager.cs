using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RhythmGameManager : MonoBehaviour
{
    [Header("Настройки игры")]
    public float gameDuration = 60f;
    public int targetScore = 10;
    public float spawnInterval = 1.5f;
    public float arrowSpeed = 150f;
    public float hitZoneY = -200f;
    public float perfectThreshold = 50f;
    public float goodThreshold = 100f;

    [Header("Зоны")]
    public RectTransform spawnArea;
    public Image hitZoneVisual;
    public Color normalColor = new Color(0f, 1f, 0f, 0.3f);
    public Color hitColor = new Color(1f, 1f, 0f, 0.5f);

    [Header("Префабы и спрайты")]
    public GameObject arrowPrefab;
    public Sprite leftArrowSprite;
    public Sprite downArrowSprite;
    public Sprite upArrowSprite;
    public Sprite rightArrowSprite;

    [Header("UI")]
    public Text scoreText;
    public Text timerText;
    public Text feedbackText;
    public GameObject resultPanel;
    public Text resultText;
    public Button restartButton;
    public Button exitButton;
    public GameObject rhythmGameCanvas;

    [Header("Визуал обратной связи")]
    public Text hitFeedbackText;

    private int currentScore = 0;
    private float currentTime;
    private bool isGameActive = false;
    private List<Arrow> activeArrows = new List<Arrow>();
    private Coroutine spawnCoroutine;
    private float feedbackTimer = 0f;
    private Coroutine feedbackCoroutine;
    private bool isExiting = false;
    private bool isPanelActive = false;
    private float lastHitTime = 0f;
    private float hitCooldown = 0.15f;

    public System.Action<bool> OnGameEnd;

    private Dictionary<Arrow.ArrowDirection, KeyCode> keyMap = new Dictionary<Arrow.ArrowDirection, KeyCode>()
    {
        { Arrow.ArrowDirection.Left, KeyCode.A },
        { Arrow.ArrowDirection.Down, KeyCode.S },
        { Arrow.ArrowDirection.Up, KeyCode.W },
        { Arrow.ArrowDirection.Right, KeyCode.D }
    };

    void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (feedbackText != null)
            feedbackText.text = "";
        if (hitFeedbackText != null)
            hitFeedbackText.gameObject.SetActive(false);

        if (hitZoneVisual != null)
            hitZoneVisual.color = normalColor;
    }

    void Update()
    {
        if (isPanelActive) return;
        if (!isGameActive) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            EndGame(false);
            return;
        }
        UpdateTimerUI();

        if (feedbackTimer > 0)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0 && feedbackText != null)
                feedbackText.text = "";
        }

        foreach (var kvp in keyMap)
        {
            if (Input.GetKeyDown(kvp.Value))
            {
                CheckHit(kvp.Key);
            }
        }
    }

    void FlashHitZone()
    {
        if (hitZoneVisual == null) return;
        StartCoroutine(FlashHitZoneCoroutine());
    }

    IEnumerator FlashHitZoneCoroutine()
    {
        hitZoneVisual.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        hitZoneVisual.color = normalColor;
    }

    void CheckHit(Arrow.ArrowDirection pressedDir)
    {
        FlashHitZone();

        if (Time.time - lastHitTime < hitCooldown) return;

        Arrow targetArrow = null;
        float closestDistance = float.MaxValue;

        foreach (Arrow arrow in activeArrows)
        {
            if (arrow != null && !arrow.isDestroyed && arrow.direction == pressedDir)
            {
                float distance = Mathf.Abs(arrow.GetYPosition() - hitZoneY);
                if (distance < 150f && distance < closestDistance)
                {
                    closestDistance = distance;
                    targetArrow = arrow;
                }
            }
        }

        if (targetArrow != null && !targetArrow.isDestroyed)
        {
            lastHitTime = Time.time;

            if (closestDistance <= perfectThreshold)
            {
                AddScore(10);
                ShowFeedback("PERFECT!", new Color(1f, 0.8f, 0f));
                ShowHitFeedback("PERFECT!", new Color(1f, 0.8f, 0f));
                targetArrow.DestroyArrow();
            }
            else if (closestDistance <= goodThreshold)
            {
                AddScore(5);
                ShowFeedback("GOOD!", Color.green);
                ShowHitFeedback("GOOD!", Color.green);
                targetArrow.DestroyArrow();
            }
            else
            {
                ShowFeedback("MISS!", Color.red);
                ShowHitFeedback("MISS!", Color.red);
            }
        }
        else
        {
            ShowFeedback("MISS!", Color.red);
            ShowHitFeedback("MISS!", Color.red);
        }
    }

    void ShowHitFeedback(string text, Color color)
    {
        if (hitFeedbackText == null) return;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(DisplayHitFeedback(text, color));
    }

    IEnumerator DisplayHitFeedback(string text, Color color)
    {
        hitFeedbackText.text = text;
        hitFeedbackText.color = color;
        hitFeedbackText.gameObject.SetActive(true);

        Vector3 originalScale = hitFeedbackText.transform.localScale;
        hitFeedbackText.transform.localScale = Vector3.one * 0.5f;

        float timer = 0f;
        while (timer < 0.2f)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(0.5f, 1.2f, timer / 0.2f);
            hitFeedbackText.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        hitFeedbackText.transform.localScale = Vector3.one * 1.2f;
        yield return new WaitForSeconds(0.5f);

        timer = 0f;
        Color startColor = color;
        while (timer < 0.2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / 0.2f);
            hitFeedbackText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        hitFeedbackText.gameObject.SetActive(false);
        hitFeedbackText.transform.localScale = originalScale;
        hitFeedbackText.color = color;
    }

    void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();

        if (currentScore >= targetScore)
        {
            EndGame(true);
        }
    }

    void ShowFeedback(string text, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = text;
            feedbackText.color = color;
            feedbackTimer = 0.5f;
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Счёт: " + currentScore + " / " + targetScore;
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = "Время: " + Mathf.CeilToInt(currentTime);
    }

    public void StartGame()
    {
        Debug.Log("=== StartGame вызван ===");

        // Сбрасываем флаги
        isExiting = false;
        isPanelActive = false;

        // Сбрасываем игровые переменные
        currentScore = 0;
        currentTime = gameDuration;
        isGameActive = true;

        // Удаляем все старые стрелки
        foreach (Arrow arrow in activeArrows)
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }
        activeArrows.Clear();

        // Останавливаем старую корутину спавна
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        // ПРИНУДИТЕЛЬНО ОБНОВЛЯЕМ UI
        if (scoreText != null)
        {
            scoreText.text = "Счёт: 0 / " + targetScore;
            Debug.Log("ScoreText обновлён: " + scoreText.text);
        }
        else
        {
            Debug.LogError("scoreText = null! Заполни ссылку в инспекторе!");
        }

        if (timerText != null)
        {
            timerText.text = "Время: " + gameDuration;
            Debug.Log("TimerText обновлён: " + timerText.text);
        }
        else
        {
            Debug.LogError("timerText = null! Заполни ссылку в инспекторе!");
        }

        if (feedbackText != null)
            feedbackText.text = "";

        if (hitFeedbackText != null)
            hitFeedbackText.gameObject.SetActive(false);

        // Скрываем панель результата, если она была открыта
        if (resultPanel != null)
            resultPanel.SetActive(false);

        // Запускаем спавн стрелок
        spawnCoroutine = StartCoroutine(SpawnArrows());

        Debug.Log("=== StartGame завершён, isGameActive = " + isGameActive + " ===");
    }

    IEnumerator SpawnArrows()
    {
        while (isGameActive)
        {
            SpawnRandomArrow();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomArrow()
    {
        Arrow.ArrowDirection randomDir = (Arrow.ArrowDirection)Random.Range(0, 4);
        SpawnArrow(randomDir);
    }

    void SpawnArrow(Arrow.ArrowDirection dir)
    {
        if (arrowPrefab == null) return;

        GameObject newArrowObj = Instantiate(arrowPrefab, spawnArea);
        Arrow arrow = newArrowObj.GetComponent<Arrow>();

        if (arrow == null)
        {
            Destroy(newArrowObj);
            return;
        }

        Sprite selectedSprite = null;
        switch (dir)
        {
            case Arrow.ArrowDirection.Left: selectedSprite = leftArrowSprite; break;
            case Arrow.ArrowDirection.Down: selectedSprite = downArrowSprite; break;
            case Arrow.ArrowDirection.Up: selectedSprite = upArrowSprite; break;
            case Arrow.ArrowDirection.Right: selectedSprite = rightArrowSprite; break;
        }

        arrow.Initialize(this, dir, selectedSprite);
        arrow.speed = arrowSpeed;

        RectTransform rect = newArrowObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(0, spawnArea.anchoredPosition.y);
        }

        activeArrows.Add(arrow);
    }

    public void RemoveArrow(Arrow arrow)
    {
        if (activeArrows.Contains(arrow))
            activeArrows.Remove(arrow);
    }

    void EndGame(bool win)
    {
        if (isExiting) return;
        isExiting = true;

        isGameActive = false;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        foreach (Arrow arrow in activeArrows)
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }
        activeArrows.Clear();

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            isPanelActive = true;

            if (resultText != null)
            {
                if (win)
                    resultText.text = "ПОБЕДА!";
                else
                    resultText.text = "ПОРАЖЕНИЕ!";
                resultText.color = win ? Color.yellow : Color.red;
            }
        }

        if (OnGameEnd != null)
            OnGameEnd(win);
    }

    public void RestartGame()
    {
        isPanelActive = false;
        isExiting = false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (feedbackText != null)
            feedbackText.text = "";
        if (hitFeedbackText != null)
            hitFeedbackText.gameObject.SetActive(false);

        StartGame();
    }

    public void ExitGame()
    {
        isPanelActive = false;
        isExiting = false;
        isGameActive = false;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        foreach (Arrow arrow in activeArrows)
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }
        activeArrows.Clear();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (rhythmGameCanvas != null)
            rhythmGameCanvas.SetActive(false);

        if (OnGameEnd != null)
            OnGameEnd(false);
    }
}