using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RhythmGameManager : MonoBehaviour
{
    [Header("Настройки")]
    public float gameDuration = 60f;
    public int targetScore = 10;
    public float spawnInterval = 1.5f;
    public float arrowSpeed = 150f;
    public float hitZoneY = -200f;
    public float perfectThreshold = 50f;
    public float goodThreshold = 100f;
    private bool isPanelActive = false;  // добавить в начало класса
    private bool isExiting = false;
    private float lastHitTime = 0f;
    private float hitCooldown = 0.15f;

    [Header("Зоны")]
    public RectTransform spawnArea;

    [Header("Префабы и спрайты")]
    public GameObject arrowPrefab;
    public Sprite leftArrowSprite;
    public Sprite downArrowSprite;
    public Sprite upArrowSprite;
    public Sprite rightArrowSprite;

    [Header("UI")]
    public GameObject rhythmGameCanvas;
    public Text scoreText;
    public Text timerText;
    public Text feedbackText;
    public GameObject resultPanel;
    public Text resultText;
    public Button restartButton;
    public Button exitButton;

    [Header("Визуал зоны попадания")]
    public Image hitZoneImage;
    public Color hitZoneNormalColor = new Color(0f, 1f, 0f, 0.3f);
    public Color hitZoneActiveColor = new Color(1f, 1f, 0f, 0.5f);
    public float hitZoneFlashDuration = 0.1f;

    [Header("Визуал обратной связи")]
    public Text hitFeedbackText;  // отдельный текст для всплывающих надписей
    public float feedbackDisplayTime = 0.5f;
    private Coroutine feedbackCoroutine;

    private int currentScore = 0;
    private float currentTime;
    private bool isGameActive = false;
    private List<Arrow> activeArrows = new List<Arrow>();
    private Coroutine spawnCoroutine;
    private float feedbackTimer = 0f;
    private Player playerMovement;
    private Coroutine hitZoneFlashCoroutine;

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
        // Находим игрока и его движение
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<Player>();
            if (playerMovement == null)
                Debug.LogError("PlayerMovement не найден на игроке!");
            else
                Debug.Log("PlayerMovement найден");
        }
        else
        {
            Debug.LogError("Player не найден с тегом 'Player'!");
        }

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (feedbackText != null)
            feedbackText.text = "";

        if (hitZoneImage != null)
            hitZoneImage.color = hitZoneNormalColor;
    }

    void Update()
    {
        // Если панель открыта - НИЧЕГО НЕ ДЕЛАЕМ
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

    void CheckHit(Arrow.ArrowDirection pressedDir)
    {
        // ЗАЩИТА ОТ СПАМА
        if (Time.time - lastHitTime < hitCooldown) return;

        FlashHitZone();

        Arrow targetArrow = null;
        float closestDistance = float.MaxValue;

        // Ищем ближайшую стрелку в зоне попадания
        foreach (Arrow arrow in activeArrows)
        {
            if (arrow != null && !arrow.isDestroyed && arrow.direction == pressedDir)
            {
                float distance = Mathf.Abs(arrow.GetYPosition() - hitZoneY);
                // Игнорируем слишком далёкие стрелки
                if (distance < 150f && distance < closestDistance)
                {
                    closestDistance = distance;
                    targetArrow = arrow;
                }
            }
        }

        if (targetArrow != null && !targetArrow.isDestroyed)
        {
            lastHitTime = Time.time;  // ТОЛЬКО ЕСЛИ НАШЛИ СТРЕЛКУ

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
                // НЕ УДАЛЯЕМ СТРЕЛКУ
            }
        }
        else
        {
            ShowFeedback("MISS!", Color.red);
            ShowHitFeedback("MISS!", Color.red);
        }
    }

    void FlashHitZone()
    {
        if (hitZoneImage == null) return;

        if (hitZoneFlashCoroutine != null)
            StopCoroutine(hitZoneFlashCoroutine);
        hitZoneFlashCoroutine = StartCoroutine(FlashHitZoneCoroutine());
    }

    IEnumerator FlashHitZoneCoroutine()
    {
        hitZoneImage.color = hitZoneActiveColor;
        yield return new WaitForSeconds(hitZoneFlashDuration);
        hitZoneImage.color = hitZoneNormalColor;
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

    public void MissArrow()
    {
        ShowFeedback("MISS!", Color.red);
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

    void ShowHitFeedback(string text, Color color)
    {
        if (hitFeedbackText == null) return;

        // Останавливаем предыдущую анимацию
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        // Запускаем новую
        feedbackCoroutine = StartCoroutine(DisplayHitFeedback(text, color));
    }

    IEnumerator DisplayHitFeedback(string text, Color color)
    {
        hitFeedbackText.text = text;
        hitFeedbackText.color = color;
        hitFeedbackText.gameObject.SetActive(true);

        // Анимация увеличения
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

        yield return new WaitForSeconds(feedbackDisplayTime);

        // Анимация исчезновения
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

    private void DisablePlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log("Движение игрока ВЫКЛЮЧЕНО");
        }
        else
        {
            Debug.LogError("playerMovement = null! Не могу отключить движение!");
        }
    }

    private void EnablePlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            Debug.Log("Движение игрока ВКЛЮЧЕНО");
        }
        else
        {
            Debug.LogError("playerMovement = null! Не могу включить движение!");
        }
    }

    public void StartGame()
    {
        Debug.Log("=== StartGame вызван ===");

        // Сбрасываем флаг выхода
        isExiting = false;

        // Отключаем движение игрока
        DisablePlayerMovement();

        // Скрываем панель результата, если она была открыта
        if (resultPanel != null)
            resultPanel.SetActive(false);

        // Сбрасываем очки и время
        currentScore = 0;
        currentTime = gameDuration;
        isGameActive = true;

        // Очищаем старые стрелки
        foreach (Arrow arrow in activeArrows)
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }
        activeArrows.Clear();

        // Останавливаем старую корутину спавна
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        UpdateScoreUI();
        UpdateTimerUI();

        // Запускаем спавн стрелок
        spawnCoroutine = StartCoroutine(SpawnArrows());

        Debug.Log("=== StartGame завершён, isGameActive = " + isGameActive);
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

        Debug.Log("=== EndGame вызван, win = " + win + " ===");

        isGameActive = false;

        // Останавливаем спавн
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        // Удаляем все стрелки
        foreach (Arrow arrow in activeArrows)
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }
        activeArrows.Clear();

        // ПОКАЗЫВАЕМ ПАНЕЛЬ И БЛОКИРУЕМ ВСЁ
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            isPanelActive = true;  // <- ЭТО БЛОКИРУЕТ ОБРАБОТКУ В UPDATE
        }

        // Вызываем событие
        if (OnGameEnd != null)
            OnGameEnd(win);
    }

    public void RestartGame()
    {
        Debug.Log("=== RestartGame вызван ===");

        // РАЗБЛОКИРУЕМ
        isPanelActive = false;
        isExiting = false;

        // Скрываем панель
        if (resultPanel != null)
            resultPanel.SetActive(false);

        // Очищаем текст
        if (feedbackText != null)
            feedbackText.text = "";
        if (hitFeedbackText != null)
            hitFeedbackText.gameObject.SetActive(false);

        // Запускаем игру
        StartGame();
    }

    public void ExitGame()
    {
        Debug.Log("=== ExitGame вызван ===");

        // РАЗБЛОКИРУЕМ ВСЁ
        isPanelActive = false;
        isExiting = false;
        isGameActive = false;

        // Останавливаем спавн
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        // Удаляем стрелки
        foreach (Arrow arrow in activeArrows)
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }
        activeArrows.Clear();

        // Включаем движение
        EnablePlayerMovement();

        // Скрываем панель
        if (resultPanel != null)
            resultPanel.SetActive(false);

        // Выключаем Canvas
        if (rhythmGameCanvas != null)
            rhythmGameCanvas.SetActive(false);
    }
}