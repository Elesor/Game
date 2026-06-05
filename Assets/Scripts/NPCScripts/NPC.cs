using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;

    private enum QuestState { NotStarted, InProgress, Completed }
    private QuestState questState = QuestState.NotStarted;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null) return;

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        SyncQuestState();

        if (questState == QuestState.NotStarted)
        {
            dialogueIndex = 0;
        }
        else if (questState == QuestState.InProgress)
        {
            dialogueIndex = dialogueData.questInProgressIndex;
        }
        else if (questState == QuestState.Completed)
        {
            dialogueIndex = dialogueData.questCompletedIndex;
        }

        isDialogueActive = true;
        dialogueIndex = 0;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);

        DisplayCurrentLine();
    }

    private void SyncQuestState()
    {
        if (dialogueData.quest == null) return;

        string questID = dialogueData.quest.questID;

        if (QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;
        }
        else
        {
            questState |= QuestState.NotStarted;
        }
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialoguelines[dialogueIndex]);
            isTyping = false;
        }

        dialogueUI.ClearChoice();

        if (dialogueData.endgDialoguesLines.Length > dialogueIndex && dialogueData.endgDialoguesLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(dialogueChoice);
                return;
            }
        }

        if (++dialogueIndex < dialogueData.dialoguelines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        if (dialogueData.voicesound != null)
        {
            AudioSource.PlayClipAtPoint(dialogueData.voicesound, transform.position, dialogueData.voicepath);
        }

        foreach (char letter in dialogueData.dialoguelines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text + letter);
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (dialogueIndex < dialogueData.autoProgressLines.Length &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choisces.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            bool givesQuest = choice.givesQuest[i];
            dialogueUI.CreateChoiceButton(choice.choisces[i], () => ChooseOption(nextIndex, givesQuest));
        }
    }

    void ChooseOption(int nextIndex, bool givesQuest)
    {
        if (givesQuest)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }
        else
        {
            // НОВОЕ: проверяем, можно ли сдать квест
            if (dialogueData.quest != null && QuestController.Instance.IsQuestActive(dialogueData.quest.questID))
            {
                bool submitted = QuestController.Instance.SubmitQuest(dialogueData.quest.questID);
                if (submitted)
                {
                    questState = QuestState.Completed;
                    Debug.Log("Quest submitted!");
                }
            }
        }

        dialogueIndex = nextIndex;
        dialogueUI.ClearChoice();
        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;

        // Добавьте проверку на null
        if (dialogueUI != null)
        {
            dialogueUI.SetDialogueText("");
            dialogueUI.ShowDialogueUI(false);
        }

        // ИСПРАВЛЕНО: обращаемся к npcName через dialogueData
        if (dialogueData != null && dialogueData.npcName == "Далиба Богдан")
        {
            CompleteCurrentQuest();
        }
    }

    public void CompleteCurrentQuest()
    {
        string questID = "talk_to_bogdan";

        if (QuestController.Instance != null && QuestController.Instance.IsQuestActive(questID))
        {
            QuestController.Instance.CompleteQuest(questID);
            Debug.Log($"Квест {questID} выполнен!");
        }
    }

}