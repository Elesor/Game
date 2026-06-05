using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activeQuests = new();
    public List<Quest> completedQuests = new();
    private QuestUI questUI;

    public System.Action<QuestProgress> OnQuestProgressChanged;

    [Header("Quest Completion Dialog")]
    public DialogueController dialogueController;
    public string completionDialogTitle = "Система";
    public string completionDialogMessage = "Студент: Станислав Загидулин. Активный участник секции программирования. Часто пропускает пары без причины." +
        "(секция программирования находится в компьютерной аудитории)";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        questUI = FindObjectOfType<QuestUI>();
    }

    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID)) return;
        if (IsQuestCompleted(quest.questID)) return;

        activeQuests.Add(new QuestProgress(quest));
        questUI?.UpdateQuestUI();
        Debug.Log($"Квест принят: {quest.questName}");
    }

    public void UpdateObjective(string questID, string objectiveID, int amount = 1)
    {
        Debug.Log($"UpdateObjective вызван: questID={questID}, objectiveID={objectiveID}, amount={amount}");

        QuestProgress progress = activeQuests.Find(q => q.quest.questID == questID);
        if (progress == null)
        {
            Debug.LogWarning($"Квест {questID} не найден в активных!");
            return;
        }

        foreach (var objective in progress.objectives)
        {
            if (objective.objectiveID == objectiveID)
            {
                objective.currentAmount += amount;
                if (objective.currentAmount > objective.requiredAmount)
                    objective.currentAmount = objective.requiredAmount;

                Debug.Log($"Objective обновлён: {objectiveID} ({objective.currentAmount}/{objective.requiredAmount})");
                break;
            }
        }

        questUI?.UpdateQuestUI();
        OnQuestProgressChanged?.Invoke(progress);

        if (progress.IsCompleted)
        {
            CompleteQuest(questID);
        }
    }

    public void CompleteQuest(string questID)
    {
        QuestProgress progress = activeQuests.Find(q => q.quest.questID == questID);
        if (progress == null) return;

        activeQuests.Remove(progress);
        completedQuests.Add(progress.quest);

        Debug.Log($"Квест завершён: {progress.quest.questName}");

        GiveQuestReward(progress.quest);

        // Вызываем обработку завершения квеста
        OnQuestCompleted(progress.quest);

        questUI?.UpdateQuestUI();
    }

    /// <summary>
    /// Обработка завершения квеста: показ диалога и выдача следующего квеста
    /// </summary>
    private void OnQuestCompleted(Quest completedQuest)
    {
        // Проверяем, что это квест "Зайти в деканат" (замените на ваш ID)
        if (completedQuest.questID == "GoDecanat2a8b7b32-9d1f-4f36-87e9-927941860654")
        {
            // Показываем диалоговое окно
            ShowCompletionDialog();

            // Выдаём следующий квест
            Quest nextQuest = GetQuestByID("talk_to_bogdan");
            if (nextQuest != null)
            {
                AcceptQuest(nextQuest);
                Debug.Log($"Выдан следующий квест: {nextQuest.questName}");
            }
            else
            {
                Debug.LogWarning("Следующий квест (talk_to_bogdan) не найден! Создайте его в папке Resources/Quests");
            }
        }
    }

    /// <summary>
    /// Показывает диалоговое окно с сообщением о завершении квеста
    /// </summary>
    private void ShowCompletionDialog()
    {
        if (dialogueController == null)
        {
            Debug.LogWarning("DialogueController не назначен в QuestController! Диалог не будет показан.");
            return;
        }

        dialogueController.ShowDialogueUI(true);
        dialogueController.SetNPCInfo(completionDialogTitle, null);
        dialogueController.SetDialogueText(completionDialogMessage);

        dialogueController.ClearChoice();

        dialogueController.CreateChoiceButton("Понятно", () => {
            dialogueController.ShowDialogueUI(false);
        });
    }

    private void GiveQuestReward(Quest quest)
    {
        Debug.Log($"Награда за квест {quest.questName} получена!");
        // Здесь можно добавить награды: опыт, предметы, деньги
        // InventoryController.Instance.AddItem(quest.rewardItem);
    }

    public bool SubmitQuest(string questID)
    {
        QuestProgress progress = activeQuests.Find(q => q.quest.questID == questID);
        if (progress == null)
        {
            Debug.Log("Квест не активен");
            return false;
        }

        if (!progress.IsCompleted)
        {
            Debug.Log("Цели квеста ещё не выполнены");
            return false;
        }

        CompleteQuest(questID);
        return true;
    }

    public bool IsQuestActive(string questID) => activeQuests.Any(q => q.quest.questID == questID);

    public bool IsQuestCompleted(string questID) => completedQuests.Any(q => q.questID == questID);

    public QuestProgress GetQuestProgress(string questID) => activeQuests.Find(q => q.quest.questID == questID);

    /// <summary>
    /// Находит квест по ID из папки Resources/Quests
    /// </summary>
    private Quest GetQuestByID(string questID)
    {
        Quest[] allQuests = Resources.LoadAll<Quest>("Quests");
        foreach (Quest q in allQuests)
        {
            if (q.questID == questID)
                return q;
        }
        return null;
    }
}