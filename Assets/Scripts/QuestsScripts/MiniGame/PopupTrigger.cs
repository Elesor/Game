using UnityEngine;
using UnityEngine.UI;

public class PopupTrigger : MonoBehaviour
{
    public MiniGameManager miniGameManager;
    public GameObject warningText;
    public GameObject virusButton;

    public GameObject popupPanel;

    private bool isComputerUnlocked = false;

    void Start()
    {
        if (warningText != null)
            warningText.SetActive(true);
        if (virusButton != null)
            virusButton.SetActive(true);
    }

    public void OnStartMinigameButton()
    {
        if (!isComputerUnlocked)
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            miniGameManager.StartMinigame();
        }
    }

    public void UnlockComputer()
    {
        isComputerUnlocked = true;

        // Просто скрываем панель после победы
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}