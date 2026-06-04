using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestSaveData
{
    public string questID;
    public List<QuestObjectiveSaveData> objectives;
}

[System.Serializable]
public class QuestObjectiveSaveData
{
    public string objectiveID;
    public int currentAmount;
}