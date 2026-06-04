using UnityEngine;
using System.Collections.Generic;

public class CodingTasksSetup : MonoBehaviour
{
    public CodingMiniGame codingMiniGame;

    void Start()
    {
        if (codingMiniGame != null && codingMiniGame.tasks.Count == 0)
        {
            codingMiniGame.tasks = new List<CodingTask>
            {
                new CodingTask
                {
                    description = "Задача 1 (Основы условий):\nНапишите код, который возвращает true, если число x больше 5.\n\nПример решения:\nif (x > 5) { return true; }",
                    expectedOutput = "true",
                    requiredKeywords = new[] { "if", "return", ">" },
                    forbiddenKeywords = new[] { "else", "while", "for" },
                    testInput = "x=10"
                },
                new CodingTask
                {
                    description = "Задача 2 (Логическое И):\nНапишите код, который возвращает true, если x > 0 И y < 10.\n\nИспользуйте оператор && (логическое И)",
                    expectedOutput = "true",
                    requiredKeywords = new[] { "return", "&&" },
                    forbiddenKeywords = new[] { "if", "else" },
                    testInput = "x=5,y=5"
                },
                new CodingTask
                {
                    description = "Задача 3 (Логическое ИЛИ):\nНапишите код, который возвращает true, если isActive = true ИЛИ count > 0.\n\nИспользуйте оператор || (логическое ИЛИ)",
                    expectedOutput = "true",
                    requiredKeywords = new[] { "return", "||" },
                    forbiddenKeywords = new[] { "if" },
                    testInput = "isActive=false,count=10"
                }
            };
        }
    }
}