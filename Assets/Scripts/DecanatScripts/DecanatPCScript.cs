using UnityEngine;

public class DecanatPCScript : MonoBehaviour, IInteractable
{
    public GameObject uiPanel;

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (uiPanel != null)
        {
            bool isOpening = !uiPanel.activeSelf;

            uiPanel.SetActive(isOpening);

            if (isOpening)
            {
                PauseController.Instance.PauseGame();
            }
            else
            {
                PauseController.Instance.ResumeGame();
            }
            Cursor.visible = isOpening;
            Cursor.lockState = isOpening ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}