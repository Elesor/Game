using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activeQuests = new();
    public List<Quest> completedQuests = new(); // НОВОЕ: завершённые квесты
    private QuestUI questUI;

    public System.Action<QuestProgress> OnQuestProgressChanged; // НОВОЕ: событие

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questUI = FindObjectOfType<QuestUI>();
    }

    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID)) return;
        if (IsQuestCompleted(quest.questID)) return;

        activeQuests.Add(new QuestProgress(quest));
        questUI?.UpdateQuestUI();
    }

    // НОВЫЙ МЕТОД: обновить прогресс цели
    public void UpdateObjective(string questID, string objectiveID, int amount = 1)
    {
        QuestProgress progress = activeQuests.Find(q => q.quest.questID == questID);
        if (progress == null) return;

        foreach (var objective in progress.objectives)
        {
            if (objective.objectiveID == objectiveID)
            {
                objective.currentAmount += amount;
                if (objective.currentAmount > objective.requiredAmount)
                    objective.currentAmount = objective.requiredAmount;

                Debug.Log($"Objective updated: {objectiveID} ({objective.currentAmount}/{objective.requiredAmount})");
                break;
            }
        }

        questUI?.UpdateQuestUI();
        OnQuestProgressChanged?.Invoke(progress);

        // Проверяем, завершился ли квест
        if (progress.IsCompleted)
        {
            CompleteQuest(questID);
        }
    }

    // НОВЫЙ МЕТОД: завершить квест
    public void CompleteQuest(string questID)
    {
        QuestProgress progress = activeQuests.Find(q => q.quest.questID == questID);
        if (progress == null) return;

        activeQuests.Remove(progress);
        completedQuests.Add(progress.quest);

        Debug.Log($"Quest completed: {progress.quest.questName}");

        // Награда за квест (можно расширить)
        GiveQuestReward(progress.quest);

        questUI?.UpdateQuestUI();
    }

    // НОВЫЙ МЕТОД: награда за квест
    private void GiveQuestReward(Quest quest)
    {
        // Здесь можно добавить награды: опыт, предметы, деньги
        Debug.Log($"Reward for {quest.questName} received!");

        // Пример: выдать предмет из quest.rewardItem
        // InventoryController.Instance.AddItem(quest.rewardItem);
    }

    // НОВЫЙ МЕТОД: сдать квест вручную (через диалог с NPC)
    public bool SubmitQuest(string questID)
    {
        QuestProgress progress = activeQuests.Find(q => q.quest.questID == questID);
        if (progress == null)
        {
            Debug.Log("Quest not active");
            return false;
        }

        if (!progress.IsCompleted)
        {
            Debug.Log("Quest objectives not completed yet");
            return false;
        }

        CompleteQuest(questID);
        return true;
    }

    public bool IsQuestActive(string questID) => activeQuests.Any(q => q.quest.questID == questID);

    public bool IsQuestCompleted(string questID) => completedQuests.Any(q => q.questID == questID);

    public QuestProgress GetQuestProgress(string questID) => activeQuests.Find(q => q.quest.questID == questID);
}