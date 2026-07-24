using UnityEngine;

public class Movement : MonoBehaviour
{
    public Rigidbody body;
    [SerializeField] float speed = 1f;
    [SerializeField] float jumpForce = 10f;
    
    public void Move(Vector3 dir)
    {
        body.linearVelocity -= Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
        body.linearVelocity += dir * speed;
    }

    public void Jump()
    {
        body.AddForce(Vector3.up * jumpForce, ForceMode.Acceleration);
        Debug.Log("Jumped");
    }
}
