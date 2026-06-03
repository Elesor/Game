using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activeQuests = new();
    private QuestUI questUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questUI = FindObjectOfType<QuestUI>(); // Добавлены ()
    }

    public void AcceptQuest(Quest quest) // Изменен тип параметра
    {
        if (IsQuestActive(quest.questID)) return;

        activeQuests.Add(new QuestProgress(quest));
        questUI.UpdateQuestUI(); // Должен быть public в классе QuestUI
    }

    public bool IsQuestActive(string questID) => activeQuests.Exists(q => q.QuestId == questID);
}