using UnityEngine;

public class Collision : MonoBehaviour
{
    public Grapple grapple;

    void OnCollisionStay()
    {
        Ray lookRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(lookRay, out RaycastHit hit, grappleRange, grappleableLayers))
        {
            grappling = true;
            targetPos = hit.point;
        }
    }
}
