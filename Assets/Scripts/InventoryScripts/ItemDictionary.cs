using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ItemDictionary : MonoBehaviour
{
    public List<Item> itemPrefabs;
    private Dictionary<int, GameObject> itemDictionary;

    private void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();

        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            itemPrefabs[i].ID = i + 1;
        }

        foreach (Item item in itemPrefabs)
        {
            itemDictionary[item.ID] = item.gameObject;
        }
    }

    public GameObject GetItemPrefab(int itemID)
    {
        itemDictionary.TryGetValue(itemID, out GameObject prefab);
        
        if (prefab == null)
        {
            Debug.LogWarning($"Предмет с ID {itemID} не найден в dictionary");
        }

        return prefab;
    }
}
