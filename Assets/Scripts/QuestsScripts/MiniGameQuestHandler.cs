using UnityEngine;

public class MiniGameQuestHandler : MonoBehaviour
{
    [Header("Quest Binding")]
    public string questID;
    public string objectiveID;
    public string miniGameName = "CodingMiniGame";

    private bool questCompleted = false;

    private void OnEnable()
    {
        // Подписываемся на событие завершения мини-игры
        CodingMiniGame.OnMiniGameCompleted += OnMiniGameCompleted;
    }

    private void OnDisable()
    {
        CodingMiniGame.OnMiniGameCompleted -= OnMiniGameCompleted;
    }

    private void OnMiniGameCompleted(bool success)
    {
        if (questCompleted) return;
        if (!success) return;

        // Проверяем активен ли квест
        if (!QuestController.Instance.IsQuestActive(questID)) return;

        // Обновляем прогресс
        QuestController.Instance.UpdateObjective(questID, objectiveID);
        questCompleted = true;

        Debug.Log($"Mini game completed for quest: {questID}");
    }
}