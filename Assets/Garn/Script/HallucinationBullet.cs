using UnityEngine;
using System.Collections;

public class HallucinationBullet : MonoBehaviour
{
    public float speed = 8f;

    public float warningDistance = 10f;

    public float reactionTime = 3f; // <-- MORE TIME HERE

    private Transform player;

    private bool warningStarted = false;
    private bool destroyed = false;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    void Update()
    {
        if(player == null) return;


        float distance = Vector3.Distance(
            transform.position,
            player.position
        );


        // Move toward player
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );


        // Start warning earlier
        if(distance <= warningDistance && !warningStarted)
        {
            StartCoroutine(HitCountdown());
        }
    }



    IEnumerator HitCountdown()
    {
        warningStarted = true;


        Debug.Log("DANGER! CLICK THE BULLET!");


        // Enable crosshair target here
        GetComponent<HallucinationBulletClick>()
        .EnableTarget();



        yield return new WaitForSeconds(reactionTime);



        if(!destroyed)
        {
            HitPlayer();
        }
    }



    void HitPlayer()
    {
        Debug.Log("Hallucination bullet hit!");

        Destroy(gameObject);
    }



    public void DestroyBullet()
    {
        destroyed = true;

        Destroy(gameObject);
    }
}