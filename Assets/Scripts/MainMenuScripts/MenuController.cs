using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;

    void Start()
    {
        menuCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (!menuCanvas.activeSelf && PauseController.IsGamePaused)
        {
            return;
        }

        bool newMenuState = !menuCanvas.activeSelf;

        if (newMenuState)
        {
            menuCanvas.SetActive(true);
            PauseController.Instance.PauseGame();
        }
        else
        {
            menuCanvas.SetActive(false);
            PauseController.Instance.ResumeGame();
        }
    }
}