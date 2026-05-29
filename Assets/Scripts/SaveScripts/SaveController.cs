using System.IO;
using Unity.Cinemachine;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;

    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        LoadGame();
    }

    public void SaveGame()
    {
        // Находим игрока (предполагается, что игрок один)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        CinemachineCamera cinemachineCamera = FindObjectOfType<CinemachineCamera>();
        if (cinemachineCamera == null) return;

        SaveData saveData = new SaveData
        {
            playerPosition = player.transform.position,
            // Для CinemachineCamera нужно использовать правильное свойство
            mapBoundary = cinemachineCamera.GetComponent<CinemachineConfiner2D>()?.BoundingShape2D?.gameObject.name ?? ""
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            // Загружаем позицию игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = saveData.playerPosition;
            }

            // Загружаем Boundary для камеры
            CinemachineCamera cinemachineCamera = FindObjectOfType<CinemachineCamera>();
            if (cinemachineCamera != null && !string.IsNullOrEmpty(saveData.mapBoundary))
            {
                CinemachineConfiner2D confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();
                if (confiner != null)
                {
                    GameObject boundaryObject = GameObject.Find(saveData.mapBoundary);
                    if (boundaryObject != null)
                    {
                        confiner.BoundingShape2D = boundaryObject.GetComponent<PolygonCollider2D>();
                    }
                }
            }
        }
        else
        {
            SaveGame();
        }
    }
}