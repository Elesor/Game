using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIInventoryPage : MonoBehaviour
{
    [SerializeField]
    private UIInventoryItem itemPrefab;

    [SerializeField]
    private RectTransform conentPanel;

    List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

    public void InitializedInventoryUI(int inventorysize)
    {
        for (int i = 0; i < inventorysize; i++)
        {
            UIInventoryItem uiTem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            uiTem.transform.SetParent(conentPanel);
            listOfUIItems.Add(uiTem);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

}
