using UnityEngine;

public class BogdanQuest : MonoBehaviour, IInteractable
{
    [Header("Quest Settings")]
    public string questID = "talk_to_bogdan";  // ID квеста, который нужно выполнить

    [Header("Dialogue")]
    public string npcName = "Богдан";
    public string dialogueBeforeComplete = "Привет! Я слышал, ты помог деканату. Отличная работа!";
    public string dialogueAfterComplete = "Спасибо за помощь! Обращайся ещё.";

    [Header("Next Quest (optional)")]
    public string nextQuestID = "";  // ID следующего квеста (если есть)

    private bool questCompleted = false;
    private DialogueController dialogueController;

    void Start()
    {
        dialogueController = DialogueController.Instance;
    }

    public bool CanInteract()
    {
        // Можно взаимодействовать, если квест активен или уже завершён (для повторного диалога)
        return true;
    }

    public void Interact()
    {
        // Проверяем, активен ли квест
        if (!questCompleted && QuestController.Instance != null && QuestController.Instance.IsQuestActive(questID))
        {
            // Завершаем квест
            QuestController.Instance.CompleteQuest(questID);
            questCompleted = true;

            // Показываем диалог с сообщением о завершении
            ShowDialogue(dialogueBeforeComplete);

            // Выдаём следующий квест (если есть)
            if (!string.IsNullOrEmpty(nextQuestID))
            {
                Quest nextQuest = GetQuestByID(nextQuestID);
                if (nextQuest != null)
                {
                    QuestController.Instance.AcceptQuest(nextQuest);
                    Debug.Log($"Выдан следующий квест: {nextQuest.questName}");
                }
            }
        }
        else
        {
            // Если квест уже выполнен, показываем обычный диалог
            ShowDialogue(dialogueAfterComplete);
        }
    }

    private void ShowDialogue(string message)
    {
        if (dialogueController == null)
        {
            Debug.LogWarning("DialogueController не найден!");
            return;
        }

        dialogueController.ShowDialogueUI(true);
        dialogueController.SetNPCInfo(npcName, null);
        dialogueController.SetDialogueText(message);

        dialogueController.ClearChoice();

        dialogueController.CreateChoiceButton("Понятно", () => {
            dialogueController.ShowDialogueUI(false);
        });
    }

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