using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CodingMiniGame : MonoBehaviour, IInteractable
{
    [Header("UI Elements")]
    public GameObject codePanel;
    public TMP_InputField codeInput;
    public TextMeshProUGUI taskText;
    public TextMeshProUGUI resultText;
    public Button submitButton;
    public Button closeButton;

    [Header("Tasks")]
    public List<CodingTask> tasks;
    private int currentTaskIndex = 0;
    private bool isGameActive = false;
    private bool wasUIModeActiveBefore = false;

    void Start()
    {
        // Добавляем 2D коллайдер если его нет
        if (GetComponent<BoxCollider2D>() == null)
        {
            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
        }

        if (codePanel != null)
            codePanel.SetActive(false);

        if (submitButton != null)
            submitButton.onClick.AddListener(CheckCode);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCodePanel);

        if (tasks == null || tasks.Count == 0)
        {
            Debug.LogError("Нет задач! Добавьте задачи в инспекторе или через код.");
            if (taskText != null)
                taskText.text = "Ошибка: Нет задач для отображения!";
            return;
        }

        LoadTask(0);
    }

    void Update()
    {
        if (codePanel != null && codePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCodePanel();
        }
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (codePanel != null)
        {
            if (!codePanel.activeSelf)
                OpenCodePanel();
            else
                CloseCodePanel();
        }
    }

    void OpenCodePanel()
    {
        if (codePanel == null || codePanel.activeSelf) return;

        // Запоминаем, был ли уже активен UI режим
        wasUIModeActiveBefore = PauseController.IsUIModeActive;

        // Включаем UI режим через PauseController (он отключит движение и покажет курсор)
        if (!wasUIModeActiveBefore)
        {
            PauseController.Instance.EnableUIMode();
        }

        codePanel.SetActive(true);

        if (codeInput != null) codeInput.text = "";
        if (resultText != null) resultText.text = "";
        isGameActive = true;

        Debug.Log("Мини-игра открыта, UI режим активен");
    }

    public void CloseCodePanel()
    {
        if (codePanel == null || !codePanel.activeSelf) return;

        codePanel.SetActive(false);

        // Выключаем UI режим только если его не было до открытия панели
        if (!wasUIModeActiveBefore)
        {
            PauseController.Instance.DisableUIMode();
        }

        isGameActive = false;

        Debug.Log("Мини-игра закрыта, UI режим деактивирован");
    }

    void CheckCode()
    {
        if (tasks == null || tasks.Count == 0)
        {
            if (resultText != null)
            {
                resultText.text = "Ошибка: Нет загруженных задач!";
                resultText.color = Color.red;
            }
            return;
        }

        if (currentTaskIndex < 0 || currentTaskIndex >= tasks.Count)
        {
            if (resultText != null)
            {
                resultText.text = "Ошибка: Неверный индекс задачи!";
                resultText.color = Color.red;
            }
            return;
        }

        if (codeInput == null) return;

        string userCode = codeInput.text;
        CodingTask currentTask = tasks[currentTaskIndex];

        if (currentTask == null)
        {
            if (resultText != null)
            {
                resultText.text = "Ошибка: Текущая задача не определена!";
                resultText.color = Color.red;
            }
            return;
        }

        CodeInterpreter interpreter = new CodeInterpreter();
        bool isCorrect = interpreter.ValidateCode(userCode, currentTask);

        if (resultText != null)
        {
            if (isCorrect)
            {
                resultText.text = "Правильно! Задача решена!";
                resultText.color = Color.green;

                if (currentTaskIndex + 1 < tasks.Count)
                {
                    currentTaskIndex++;
                    LoadTask(currentTaskIndex);
                }
                else
                {
                    resultText.text += "\nПоздравляю! Вы завершили все задачи!";
                    if (submitButton != null) submitButton.interactable = false;
                }
            }
            else
            {
                resultText.text = "Неправильно. Попробуйте еще раз!";
                resultText.color = Color.red;
            }
        }
    }

    void LoadTask(int index)
    {
        if (tasks == null || tasks.Count == 0)
        {
            Debug.LogError("Нельзя загрузить задачу: список задач пуст!");
            return;
        }

        if (index < 0 || index >= tasks.Count)
        {
            Debug.LogError($"Неверный индекс задачи для загрузки: {index}, всего задач: {tasks.Count}");
            return;
        }

        CodingTask task = tasks[index];
        if (task == null)
        {
            Debug.LogError($"Задача с индексом {index} равна null!");
            return;
        }

        if (taskText != null)
            taskText.text = task.description;

        if (codeInput != null)
            codeInput.text = "";

        if (resultText != null)
            resultText.text = "";

        if (submitButton != null)
            submitButton.interactable = true;
    }
}