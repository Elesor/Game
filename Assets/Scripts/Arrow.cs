using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    public enum ArrowDirection { Left, Down, Up, Right }
    public ArrowDirection direction;
    public float speed = 150f;
    public bool isDestroyed = false;

    private RectTransform rectTransform;
    private RhythmGameManager gameManager;
    private Image arrowImage;

    public void Initialize(RhythmGameManager manager, ArrowDirection dir, Sprite sprite)
    {
        gameManager = manager;
        direction = dir;
        rectTransform = GetComponent<RectTransform>();
        arrowImage = GetComponent<Image>();

        if (arrowImage != null && sprite != null)
            arrowImage.sprite = sprite;
    }

    void Update()
    {
        if (isDestroyed) return;

        rectTransform.anchoredPosition += Vector2.down * speed * Time.deltaTime;

        // Удаляем только если улетела далеко за зону
        if (rectTransform.anchoredPosition.y < -1000f)
        {
            if (gameManager != null)
                gameManager.MissArrow();
            DestroyArrow();
        }
    }

    public void DestroyArrow()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (gameManager != null)
            gameManager.RemoveArrow(this);

        Destroy(gameObject);
    }

    public float GetYPosition()
    {
        return rectTransform != null ? rectTransform.anchoredPosition.y : -999f;
    }
}