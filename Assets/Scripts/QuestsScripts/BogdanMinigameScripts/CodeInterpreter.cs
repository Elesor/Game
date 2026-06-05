using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class CodeInterpreter
{
    public bool ValidateCode(string code, CodingTask task)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        string originalCode = code;
        code = code.ToLower().Trim();

        if (task.requiredKeywords != null)
        {
            foreach (string keyword in task.requiredKeywords)
            {
                if (!code.Contains(keyword.ToLower()))
                {
                    Debug.Log($"Отсутствует ключевое слово: {keyword}");
                    return false;
                }
            }
        }

        if (task.forbiddenKeywords != null)
        {
            foreach (string keyword in task.forbiddenKeywords)
            {
                if (code.Contains(keyword.ToLower()))
                {
                    Debug.Log($"Использовано запрещенное слово: {keyword}");
                    return false;
                }
            }
        }

        return ExecuteTest(originalCode, task);
    }

    bool ExecuteTest(string code, CodingTask task)
    {
        try
        {
            string result = InterpretCode(code, task.testInput);
            Debug.Log($"Результат выполнения: '{result}' (ожидалось: '{task.expectedOutput}')");
            return result == task.expectedOutput;
        }
        catch (System.Exception e)
        {
            Debug.Log($"Ошибка выполнения: {e.Message}");
            return false;
        }
    }

    string InterpretCode(string code, string input)
    {
        var variables = ParseInput(input);

        // Удаляем лишние пробелы, переносы строк и табуляции
        code = Regex.Replace(code, @"\s+", " ");
        code = code.Trim();

        // Более гибкий паттерн для if (условие) { return значение; }
        string ifPattern = @"if\s*\(\s*(.+?)\s*\)\s*\{\s*return\s+(.+?);?\s*\}";
        Match ifMatch = Regex.Match(code, ifPattern, RegexOptions.IgnoreCase);

        if (ifMatch.Success)
        {
            string condition = ifMatch.Groups[1].Value;
            string returnValue = ifMatch.Groups[2].Value;

            bool conditionResult = EvaluateCondition(condition, variables);

            if (conditionResult)
            {
                object value = EvaluateExpression(returnValue, variables);
                return value.ToString().ToLower();
            }
            else
            {
                return "false";
            }
        }

        // Паттерн для простого return выражение;
        string returnPattern = @"return\s+(.+);";
        Match returnMatch = Regex.Match(code, returnPattern, RegexOptions.IgnoreCase);

        if (returnMatch.Success)
        {
            string expression = returnMatch.Groups[1].Value;
            object result = EvaluateExpression(expression, variables);
            return result.ToString().ToLower();
        }

        return "false";
    }

    // Добавьте этот вспомогательный метод
    bool EvaluateCondition(string condition, Dictionary<string, object> variables)
    {
        try
        {
            object result = EvaluateExpression(condition, variables);
            if (result is bool)
                return (bool)result;
            else if (result is int)
                return (int)result != 0;
            else if (result is string)
                return !string.IsNullOrEmpty((string)result);
            else if (result is double)
                return (double)result != 0;
            else
                return false;
        }
        catch
        {
            return false;
        }
    }
    Dictionary<string, object> ParseInput(string input)
    {
        var variables = new Dictionary<string, object>();

        if (string.IsNullOrEmpty(input))
            return variables;

        var parts = input.Split(',');
        foreach (var part in parts)
        {
            var kvp = part.Split('=');
            if (kvp.Length == 2)
            {
                string key = kvp[0].Trim();
                string value = kvp[1].Trim();

                if (int.TryParse(value, out int intValue))
                    variables[key] = intValue;
                else if (bool.TryParse(value, out bool boolValue))
                    variables[key] = boolValue;
                else
                    variables[key] = value;
            }
        }

        return variables;
    }

    object EvaluateExpression(string expression, Dictionary<string, object> variables)
    {
        expression = expression.Trim();

        // Заменяем переменные на их значения
        foreach (var variable in variables)
        {
            string pattern = $@"\b{Regex.Escape(variable.Key)}\b";
            string replacement = variable.Value.ToString();
            expression = Regex.Replace(expression, pattern, replacement, RegexOptions.IgnoreCase);
        }

        // Обработка скобок (простейшая)
        while (expression.Contains("("))
        {
            var match = Regex.Match(expression, @"\(([^()]+)\)");
            if (match.Success)
            {
                string innerExpression = match.Groups[1].Value;
                object innerResult = EvaluateExpression(innerExpression, variables);
                expression = expression.Replace($"({innerExpression})", innerResult.ToString());
            }
            else break;
        }

        // Обработка && (логическое И) - с учетом приоритета
        if (expression.Contains("&&"))
        {
            var parts = SplitByOperator(expression, "&&");
            bool left = (bool)EvaluateExpression(parts[0], variables);
            bool right = (bool)EvaluateExpression(parts[1], variables);
            return left && right;
        }

        // Обработка || (логическое ИЛИ)
        if (expression.Contains("||"))
        {
            var parts = SplitByOperator(expression, "||");
            bool left = (bool)EvaluateExpression(parts[0], variables);
            bool right = (bool)EvaluateExpression(parts[1], variables);
            return left || right;
        }

        // Обработка сравнений (сначала >, >=, <, <=, потом ==, !=)

        // >=
        if (expression.Contains(">="))
        {
            var parts = SplitByOperator(expression, ">=");
            double left = Convert.ToDouble(EvaluateExpression(parts[0], variables));
            double right = Convert.ToDouble(EvaluateExpression(parts[1], variables));
            return left >= right;
        }

        // <=
        if (expression.Contains("<="))
        {
            var parts = SplitByOperator(expression, "<=");
            double left = Convert.ToDouble(EvaluateExpression(parts[0], variables));
            double right = Convert.ToDouble(EvaluateExpression(parts[1], variables));
            return left <= right;
        }

        // >
        if (expression.Contains(">"))
        {
            var parts = SplitByOperator(expression, ">");
            double left = Convert.ToDouble(EvaluateExpression(parts[0], variables));
            double right = Convert.ToDouble(EvaluateExpression(parts[1], variables));
            return left > right;
        }

        // <
        if (expression.Contains("<"))
        {
            var parts = SplitByOperator(expression, "<");
            double left = Convert.ToDouble(EvaluateExpression(parts[0], variables));
            double right = Convert.ToDouble(EvaluateExpression(parts[1], variables));
            return left < right;
        }

        // == (равенство)
        if (expression.Contains("=="))
        {
            var parts = SplitByOperator(expression, "==");
            string left = EvaluateExpression(parts[0], variables).ToString();
            string right = EvaluateExpression(parts[1], variables).ToString();
            return left == right;
        }

        // != (неравенство)
        if (expression.Contains("!="))
        {
            var parts = SplitByOperator(expression, "!=");
            string left = EvaluateExpression(parts[0], variables).ToString();
            string right = EvaluateExpression(parts[1], variables).ToString();
            return left != right;
        }

        // Арифметические операции (простейшие)
        if (expression.Contains("+") && !expression.Contains("&&") && !expression.Contains("||"))
        {
            var parts = SplitByOperator(expression, "+");
            double left = Convert.ToDouble(EvaluateExpression(parts[0], variables));
            double right = Convert.ToDouble(EvaluateExpression(parts[1], variables));
            return left + right;
        }

        if (expression.Contains("-") && !expression.Contains("&&") && !expression.Contains("||"))
        {
            var parts = SplitByOperator(expression, "-");
            double left = Convert.ToDouble(EvaluateExpression(parts[0], variables));
            double right = Convert.ToDouble(EvaluateExpression(parts[1], variables));
            return left - right;
        }

        if (expression.Contains("*"))
        {
            var parts = SplitByOperator(expression, "*");
            double left = Convert.ToDouble(EvaluateExpression(parts[0], variables));
            double right = Convert.ToDouble(EvaluateExpression(parts[1], variables));
            return left * right;
        }

        if (expression.Contains("/"))
        {
            var parts = SplitByOperator(expression, "/");
            double left = Convert.ToDouble(EvaluateExpression(parts[0], variables));
            double right = Convert.ToDouble(EvaluateExpression(parts[1], variables));
            if (right == 0) return 0;
            return left / right;
        }

        // Базовые типы
        if (bool.TryParse(expression, out bool boolResult))
            return boolResult;

        if (int.TryParse(expression, out int intResult))
            return intResult;

        if (double.TryParse(expression, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double doubleResult))
            return doubleResult;

        // Строка
        if (expression.StartsWith("\"") && expression.EndsWith("\""))
            return expression.Trim('"');

        return expression;
    }

    string[] SplitByOperator(string expression, string operatorSymbol)
    {
        // Разделяем по оператору, но не внутри скобок
        int depth = 0;
        int splitIndex = -1;

        for (int i = 0; i < expression.Length; i++)
        {
            char c = expression[i];
            if (c == '(') depth++;
            else if (c == ')') depth--;
            else if (depth == 0 && i + operatorSymbol.Length <= expression.Length)
            {
                string sub = expression.Substring(i, operatorSymbol.Length);
                if (sub == operatorSymbol)
                {
                    splitIndex = i;
                    break;
                }
            }
        }

        if (splitIndex != -1)
        {
            string left = expression.Substring(0, splitIndex).Trim();
            string right = expression.Substring(splitIndex + operatorSymbol.Length).Trim();
            return new string[] { left, right };
        }

        return new string[] { expression, "" };
    }
}