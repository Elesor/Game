using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    [Header("Игрок и перемещение")]
    public GameObject player;
    public Transform arenaSpawnPoint;
    public Transform worldSpawnPoint;

    [Header("Арена и камеры")]
    public GameObject arenaZone;
    public Camera mainCamera;
    public Camera arenaCamera;

    [Header("Враги")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    [Header("UI")]
    public GameObject gameUIPanel;
    public Text scoreText;
    public Text waveProgressText;
    public FloatingMessage floatingMessage;

    private PlayerShoot playerShoot;
    private int score = 0;
    private int currentWave = 1;
    private int enemiesRemaining = 0;
    private bool isMinigameActive = false;
    private int totalWaves = 5;

    // Флаг для предотвращения многократного обновления квеста
    private bool questCompletedTriggered = false;

    void Start()
    {
        gameUIPanel.SetActive(false);

        if (player != null)
        {
            playerShoot = player.GetComponent<PlayerShoot>();
            if (playerShoot != null)
                playerShoot.DisableShooting();
        }

        if (arenaZone != null)
            arenaZone.SetActive(false);

        if (arenaCamera != null)
            arenaCamera.gameObject.SetActive(false);
    }

    public void StartMinigame()
    {
        StartCoroutine(TransitionToArena());
    }

    IEnumerator TransitionToArena()
    {
        isMinigameActive = true;
        questCompletedTriggered = false; // Сбрасываем флаг при новом запуске

        if (player != null && arenaSpawnPoint != null)
            player.transform.position = arenaSpawnPoint.position;

        if (arenaZone != null)
            arenaZone.SetActive(true);

        if (arenaCamera != null && mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
            arenaCamera.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.1f);

        score = 0;
        currentWave = 1;
        gameUIPanel.SetActive(true);

        if (playerShoot != null)
            playerShoot.EnableShooting();

        UpdateWaveUI();
        StartWave();
    }

    void StartWave()
    {
        if (enemyPrefab == null) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        int enemiesToSpawn = 3 + currentWave;
        enemiesRemaining = enemiesToSpawn;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
        }

        UpdateWaveUI();
        UpdateScoreUI();
    }

    void UpdateWaveUI()
    {
        if (waveProgressText != null)
            waveProgressText.text = "Волна " + currentWave + " из " + totalWaves;
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyPrefab, spawn.position, Quaternion.identity);
        EnemyAI ai = enemy.GetComponent<EnemyAI>();

        if (ai != null)
        {
            ai.target = player.transform;
            ai.OnEnemyDeath += HandleEnemyDeath;
        }
    }

    void HandleEnemyDeath()
    {
        enemiesRemaining--;
        score += 10;
        UpdateScoreUI();

        if (enemiesRemaining <= 0 && currentWave < totalWaves)
        {
            currentWave++;
            StartWave();
        }
        else if (enemiesRemaining <= 0 && currentWave >= totalWaves)
        {
            EndMinigame(true);
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Очки: " + score;
    }

    public void EndMinigame(bool win)
    {
        StartCoroutine(TransitionFromArena(win));
    }

    IEnumerator TransitionFromArena(bool win)
    {
        isMinigameActive = false;

        if (floatingMessage != null)
        {
            if (win)
                floatingMessage.ShowMessage("КОМПЬЮТЕР ДОСТУПЕН!", 2f);
            else
                floatingMessage.ShowMessage("ВЫ ПРОИГРАЛИ!", 2f);
        }

        // Удаляем всех врагов
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
            Destroy(enemy);

        if (gameUIPanel != null)
            gameUIPanel.SetActive(false);

        if (playerShoot != null)
            playerShoot.DisableShooting();

        if (player != null && worldSpawnPoint != null)
            player.transform.position = worldSpawnPoint.position;

        if (arenaZone != null)
            arenaZone.SetActive(false);

        if (arenaCamera != null && mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
            arenaCamera.gameObject.SetActive(false);
        }

        // ===== ОБНОВЛЯЕМ КВЕСТ ПРИ ПОБЕДЕ =====
        if (win && !questCompletedTriggered)
        {
            questCompletedTriggered = true;
            UpdateQuestProgress();
        }

        yield return new WaitForSeconds(2.2f);
    }

    /// <summary>
    /// Обновляет прогресс квеста после победы в мини-игре
    /// </summary>
    private void UpdateQuestProgress()
    {
        // Пытаемся получить QuestController
        QuestController controller = QuestController.Instance;

        // Если Instance == null, ищем на сцене вручную
        if (controller == null)
        {
            controller = FindObjectOfType<QuestController>();

            if (controller == null)
            {
                Debug.LogError("❌ QuestController НЕ НАЙДЕН! Добавьте его на сцену.");
                Debug.LogError("Совет: создайте пустой GameObject -> QuestController -> добавьте скрипт");
                return;
            }

            Debug.Log("✅ QuestController найден через FindObjectOfType!");
        }

        string questID = "GoDecanat2a8b7b32-9d1f-4f36-87e9-927941860654";
        string objectiveID = "go_to_decanat";

        if (controller.IsQuestActive(questID))
        {
            controller.UpdateObjective(questID, objectiveID, 1);
            Debug.Log($"✅ Квест обновлён: {questID}");
        }
        else
        {
            Debug.Log($"⚠️ Квест {questID} не активен. Активные квесты:");
            foreach (var q in controller.activeQuests)
            {
                Debug.Log($"   - {q.quest.questID}");
            }
        }
    }

    public void LoseLife()
    {
        if (isMinigameActive)
            EndMinigame(false);
    }
}