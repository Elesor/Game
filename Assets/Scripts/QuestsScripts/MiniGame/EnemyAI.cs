using UnityEngine;
using System;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public Transform target;
    public float speed = 2f;
    public int maxHealth = 2;
    private int currentHealth;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public event Action OnEnemyDeath;

    void Start()
    {
        currentHealth = maxHealth;

        // Ищем SpriteRenderer на себе ИЛИ на любом дочернем объекте
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            Debug.Log("SpriteRenderer найден на: " + spriteRenderer.gameObject.name);
        }
        else
        {
            Debug.LogWarning("У врага и его детей нет SpriteRenderer! Мигание цветом не будет работать.");
        }
    }

    void Update()
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            TakeDamage(1);
        }
        else if (other.CompareTag("Player"))
        {
            MiniGameManager manager = FindObjectOfType<MiniGameManager>();
            if (manager != null)
                manager.LoseLife();
            Die();
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;

        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        OnEnemyDeath?.Invoke();
        Destroy(gameObject);
    }
}