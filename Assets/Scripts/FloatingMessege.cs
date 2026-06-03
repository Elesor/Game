using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FloatingMessage : MonoBehaviour
{
    public GameObject messageCanvas;
    public Text messageText;

    void Start()
    {
        if (messageCanvas != null)
            messageCanvas.SetActive(false);
    }

    public void ShowMessage(string msg, float duration = 2f)
    {
        // Сначала скрываем старое сообщение
        HideMessage();
        // Запускаем новое
        StartCoroutine(DisplayMessage(msg, duration));
    }

    public void HideMessage()
    {
        StopAllCoroutines();
        if (messageCanvas != null)
            messageCanvas.SetActive(false);
    }

    IEnumerator DisplayMessage(string msg, float duration)
    {
        if (messageText != null)
            messageText.text = msg;

        if (messageCanvas != null)
            messageCanvas.SetActive(true);

        yield return new WaitForSeconds(duration);

        if (messageCanvas != null)
            messageCanvas.SetActive(false);
    }
}