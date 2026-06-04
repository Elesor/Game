using UnityEngine;

public class MenuExit : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("Игра закрылась");
        Application.Quit();
    }
}
