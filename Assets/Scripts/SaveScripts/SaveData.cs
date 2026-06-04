using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary; // The boundary name for the map;
    public List<InventorySaveData> inventorySaveData;
    public List<QuestSaveData> questSaveData; // НОВОЕ: список квестов
}