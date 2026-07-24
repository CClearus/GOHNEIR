using UnityEngine;
using System;

public class Controls : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 1f;
    Vector3 rotation;
    Movement movement;
    public Rigidbody body;

    void Start()
    {
        rotation = Vector3.zero;
        Cursor.lockState = CursorLockMode.Locked;
        movement = GetComponent<Movement>();
    }

    void Update()
    {

        // Look
        rotation.x -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotation.y += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);
        transform.rotation = Quaternion.Euler(rotation);

        // Movement
        int rLDir = Convert.ToInt32(Input.GetKey(KeyCode.D)) - Convert.ToInt32(Input.GetKey(KeyCode.A));
        int fBDir = Convert.ToInt32(Input.GetKey(KeyCode.W)) - Convert.ToInt32(Input.GetKey(KeyCode.S));
        Vector3 dir = new Vector3(rLDir, 0f, fBDir).normalized;
        movement.Move(Quaternion.Euler(0f, rotation.y, 0f) * dir);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && (Physics.Raycast(transform.position, Vector3.down, 1.5f, LayerMask.GetMask("Default")) || movement.IsWallSliding))
        {
            movement.Jump();
        }

    }
}
