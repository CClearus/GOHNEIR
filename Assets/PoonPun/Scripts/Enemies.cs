using UnityEngine;

public class Enemies : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 30f;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
