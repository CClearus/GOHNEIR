using UnityEngine;

public class HallucinationBulletClick : MonoBehaviour
{

    private bool canClick = false;


    public void EnableTarget()
    {
        canClick = true;

        Debug.Log("Shoot the hallucination!");
    }


    void OnMouseDown()
    {
        if(canClick)
        {
            Debug.Log("Bullet destroyed!");

            GetComponent<HallucinationBullet>()
            .DestroyBullet();
        }
    }
}