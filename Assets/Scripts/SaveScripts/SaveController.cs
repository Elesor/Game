using System.IO;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;

    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();

        LoadGame();
    }

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        CinemachineCamera cinemachineCamera = FindObjectOfType<CinemachineCamera>();
        if (cinemachineCamera == null) return;

        CinemachineConfiner2D confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();

        string boundaryName = "";
        if (confiner != null && confiner.BoundingShape2D != null)
        {
            boundaryName = confiner.BoundingShape2D.gameObject.name;
        }

        SaveData saveData = new SaveData
        {
            playerPosition = player.transform.position,
            mapBoundary = boundaryName,
            inventorySaveData = inventoryController != null ? inventoryController.GetInventoryItems() : new List<InventorySaveData>(),
            questSaveData = GetQuestSaveData() // НОВОЕ: сохраняем квесты
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log($"Game saved at {saveLocation}");
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = saveData.playerPosition;
                Debug.Log($"Player position loaded: {saveData.playerPosition}");
            }

            CinemachineCamera cinemachineCamera = FindObjectOfType<CinemachineCamera>();
            if (cinemachineCamera != null && !string.IsNullOrEmpty(saveData.mapBoundary))
            {
                CinemachineConfiner2D confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();
                if (confiner != null)
                {
                    GameObject boundaryObject = GameObject.Find(saveData.mapBoundary);
                    if (boundaryObject == null)
                    {
                        boundaryObject = GameObject.Find("Camera Confiner");
                    }

                    if (boundaryObject != null)
                    {
                        PolygonCollider2D polygonCollider = boundaryObject.GetComponent<PolygonCollider2D>();
                        if (polygonCollider != null)
                        {
                            confiner.BoundingShape2D = polygonCollider;
                            Debug.Log($"Boundary loaded: {boundaryObject.name}");
                        }
                        else
                        {
                            Debug.LogWarning("Camera Confiner object has no PolygonCollider2D!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Boundary object '{saveData.mapBoundary}' not found!");
                    }
                }
            }

            if (inventoryController != null)
                inventoryController.SetInventoryItems(saveData.inventorySaveData);

            // НОВОЕ: загружаем квесты
            LoadQuestSaveData(saveData.questSaveData);
        }
        else
        {
            SaveGame();
        }
    }

    // НОВЫЙ МЕТОД: получить данные о квестах для сохранения
    private List<QuestSaveData> GetQuestSaveData()
    {
        List<QuestSaveData> questSaveDataList = new List<QuestSaveData>();

        if (QuestController.Instance == null) return questSaveDataList;

        foreach (var questProgress in QuestController.Instance.activeQuests)
        {
            QuestSaveData questSaveData = new QuestSaveData
            {
                questID = questProgress.quest.questID,
                objectives = new List<QuestObjectiveSaveData>()
            };

            foreach (var objective in questProgress.objectives)
            {
                questSaveData.objectives.Add(new QuestObjectiveSaveData
                {
                    objectiveID = objective.objectiveID,
                    currentAmount = objective.currentAmount
                });
            }

            questSaveDataList.Add(questSaveData);
        }

        return questSaveDataList;
    }

    // НОВЫЙ МЕТОД: загрузить данные о квестах
    private void LoadQuestSaveData(List<QuestSaveData> savedQuests)
    {
        if (QuestController.Instance == null)
        {
            Debug.LogWarning("QuestController not found, cannot load quests");
            return;
        }

        if (savedQuests == null || savedQuests.Count == 0)
        {
            Debug.Log("No quest save data found");
            return;
        }

        // Очищаем текущие активные квесты
        QuestController.Instance.activeQuests.Clear();

        foreach (var savedQuest in savedQuests)
        {
            // Ищем оригинальный квест по ID
            Quest quest = FindQuestByID(savedQuest.questID);
            if (quest == null)
            {
                Debug.LogWarning($"Quest with ID '{savedQuest.questID}' not found!");
                continue;
            }

            // Создаём прогресс квеста
            QuestProgress questProgress = new QuestProgress(quest);

            // Восстанавливаем прогресс целей
            foreach (var objectiveProgress in questProgress.objectives)
            {
                var savedObjective = savedQuest.objectives.Find(o => o.objectiveID == objectiveProgress.objectiveID);
                if (savedObjective != null)
                {
                    objectiveProgress.currentAmount = savedObjective.currentAmount;
                }
            }

            QuestController.Instance.activeQuests.Add(questProgress);
        }

        // Обновляем UI квестов
        QuestUI questUI = FindObjectOfType<QuestUI>();
        if (questUI != null)
        {
            questUI.UpdateQuestUI();
        }

        Debug.Log($"Loaded {savedQuests.Count} quests");
    }

    // Вспомогательный метод для поиска квеста по ID
    private Quest FindQuestByID(string questID)
    {
        // Ищем все загруженные Quest ScriptableObjects
        Quest[] allQuests = Resources.LoadAll<Quest>("");

        foreach (Quest quest in allQuests)
        {
            if (quest.questID == questID)
                return quest;
        }

        return null;
    }

    public void DeleteSave()
    {
        if (File.Exists(saveLocation))
        {
            File.Delete(saveLocation);
            Debug.Log("Save file deleted!");

            // Перезагружаем текущую сцену
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
        else
        {
            Debug.Log("No save file found to delete");
        }
    }
}