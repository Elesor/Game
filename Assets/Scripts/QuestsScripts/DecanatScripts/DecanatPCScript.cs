using UnityEngine;

public class DecanatPCScript : MonoBehaviour, IInteractable
{
    public GameObject uiPanel;
    private bool wasPausedBeforePanel = false; // Запоминаем, была ли игра уже на паузе

    void Update()
    {
        if (uiPanel != null && uiPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (uiPanel != null)
        {
            bool isActive = !uiPanel.activeSelf;

            if (isActive)
                OpenPanel();
            else
                ClosePanel();
        }
    }

    public void ClosePanel()
    {
        if (uiPanel != null && uiPanel.activeSelf)
        {
            uiPanel.SetActive(false);
            SetCursorState(false);

            // Возвращаем паузу в предыдущее состояние (если пауза была вызвана не этим окном)
            if (!wasPausedBeforePanel)
                PauseController.SetPause(false);
        }
    }

    public void OpenPanel()
    {
        if (uiPanel != null && !uiPanel.activeSelf)
        {
            // Запоминаем, была ли игра уже на паузе до открытия панели
            //wasPausedBeforePanel = PauseController.IsGamePaused;

            // Если игра не на паузе, ставим её на паузу
            //if (!wasPausedBeforePanel)
             //   PauseController.SetPause(true);

            uiPanel.SetActive(true);
            SetCursorState(true);
        }
    }

    private void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}