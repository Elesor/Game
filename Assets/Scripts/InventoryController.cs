using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [SerializeField]
    private UIInventoryPage inventoryUI;

    public int inventorySize = 10;

    private void Start()
    {
        inventoryUI.InitializedInventoryUI(inventorySize);
    }

    public void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (inventoryUI.isActiveAndEnabled == false)
                inventoryUI.Show();
            else
                inventoryUI.Hide();
        }
    }
}