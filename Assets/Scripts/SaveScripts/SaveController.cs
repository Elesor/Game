using System.IO;
using Unity.Cinemachine;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;

    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();
        
        LoadGame();
    }

    public void SaveGame()
    {

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        CinemachineCamera cinemachineCamera = FindObjectOfType<CinemachineCamera>();
        if (cinemachineCamera == null) return;

        CinemachineConfiner2D confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();

        string boundaryName = "";
        if (confiner != null && confiner.BoundingShape2D != null)
        {
            boundaryName = confiner.BoundingShape2D.gameObject.name;
        }

        SaveData saveData = new SaveData
        {
            playerPosition = player.transform.position,
            mapBoundary = boundaryName,
            inventorySaveData = inventoryController.GetInventoryItems()

        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log($"Game saved at {saveLocation}");
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = saveData.playerPosition;
                Debug.Log($"Player position loaded: {saveData.playerPosition}");
            }

            CinemachineCamera cinemachineCamera = FindObjectOfType<CinemachineCamera>();
            if (cinemachineCamera != null && !string.IsNullOrEmpty(saveData.mapBoundary))
            {
                CinemachineConfiner2D confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();
                if (confiner != null)
                {
                    GameObject boundaryObject = GameObject.Find(saveData.mapBoundary);
                    if (boundaryObject == null)
                    {
                        boundaryObject = GameObject.Find("Camera Confiner");
                    }

                    if (boundaryObject != null)
                    {
                        PolygonCollider2D polygonCollider = boundaryObject.GetComponent<PolygonCollider2D>();
                        if (polygonCollider != null)
                        {
                            confiner.BoundingShape2D = polygonCollider;
                            Debug.Log($"Boundary loaded: {boundaryObject.name}");
                        }
                        else
                        {
                            Debug.LogWarning("Camera Confiner object has no PolygonCollider2D!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Boundary object '{saveData.mapBoundary}' not found!");
                    }
                }
            }
            inventoryController.SetInventoryItems(saveData.inventorySaveData);
        }
        else
        {
            SaveGame();
        }
    }
}