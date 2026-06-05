using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    public Button resetButton;

    void Start()
    {
        resetButton.onClick.AddListener(() => {
            SaveController saveController = FindObjectOfType<SaveController>();
            if (saveController != null)
            {
                saveController.DeleteSave();
            }
        });
    }
}