using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowDialogueUI(bool show)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(show);

        // Управление режимом курсора через PauseController
        if (show)
        {
            PauseController.Instance.EnableUIMode();
        }
        else
        {
            PauseController.Instance.DisableUIMode();
        }
    }

    // Оставляем ТОЛЬКО ОДИН метод SetDialogueText (с проверкой на null)
    public void SetDialogueText(string text)
    {
        if (dialogueText != null)
            dialogueText.text = text;
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        if (nameText != null)
            nameText.text = npcName;
        if (portraitImage != null)
            portraitImage.sprite = portrait;
    }

    public void ClearChoice()
    {
        if (choiceContainer != null)
        {
            foreach (Transform child in choiceContainer)
                Destroy(child.gameObject);
        }
    }



    public GameObject CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClick)
    {
        if (choiceButtonPrefab == null || choiceContainer == null)
            return null;

        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);

        TMP_Text buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
            buttonText.text = choiceText;

        Button button = choiceButton.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(onClick);

        return choiceButton;
    }
}