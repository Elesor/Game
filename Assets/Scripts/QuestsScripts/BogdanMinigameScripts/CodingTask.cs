using System;

[Serializable]
public class CodingTask
{
    public string description;
    public string expectedOutput;
    public string[] requiredKeywords;
    public string[] forbiddenKeywords;
    public string testInput;
}