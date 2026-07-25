using UnityEngine;
using System.Collections;

public class HallucinationBullet : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float warningDistance = 10f;

    [Header("Reaction")]
    public float reactionTime = 3f;

    private Transform player;

    private bool warningStarted = false;
    private bool destroyed = false;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("No GameObject with the tag 'Player' was found!");
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Move toward the player
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );

        // Start the reaction timer once
        if (distance <= warningDistance && !warningStarted)
        {
            StartCoroutine(HitCountdown());
        }
    }

    IEnumerator HitCountdown()
    {
        warningStarted = true;

        Debug.Log("DANGER! CLICK THE BULLET!");

        HallucinationBulletClick clickScript =
            GetComponent<HallucinationBulletClick>();

        if (clickScript != null)
        {
            clickScript.EnableTarget();
        }
        else
        {
            Debug.LogWarning("HallucinationBulletClick component is missing!");
        }

        yield return new WaitForSeconds(reactionTime);

        if (!destroyed)
        {
            HitPlayer();
        }
    }

    void HitPlayer()
    {
        Debug.Log("Hallucination bullet hit!");

        // Damage the player later:
        // FindObjectOfType<HealthSystem>().TakeDamage(20);

        Destroy(gameObject);
    }

    public void DestroyBullet()
    {
        destroyed = true;

        Debug.Log("Hallucination bullet destroyed!");

        Destroy(gameObject);
    }
}