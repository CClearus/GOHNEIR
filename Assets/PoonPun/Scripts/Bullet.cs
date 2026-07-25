using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount);
}

[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Tags the bullet passes through instead of colliding with (e.g. other bullets, the enemies that fired it).")]
    [SerializeField] private string[] ignoreTags = { "Bullet", "Enemies" };

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null) damageable.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        foreach (string tag in ignoreTags)
        {
            if (other.CompareTag(tag)) return;
        }

        // Anything else (walls, obstacles, etc.) stops the bullet.
        Debug.Log($"{name}: destroyed after hitting '{other.name}' (tag='{other.tag}') at {transform.position}, time {Time.time:F3}");
        Destroy(gameObject);
    }
}
