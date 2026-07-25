using UnityEngine;

public class Hallucination : MonoBehaviour
{
    public float disappearDistance = 2f;

    private Transform player;
    private HallucinationManager manager;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        manager = FindObjectOfType<HallucinationManager>();
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) <= disappearDistance)
        {
            manager.HallucinationDisappeared();
            Destroy(gameObject);
        }
    }
}