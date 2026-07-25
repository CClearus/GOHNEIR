using UnityEngine;

public class FPSCameraLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform playerBody;
    [SerializeField] PlayerMovementdiddy playerMovement;

    [Header("Look")]
    [SerializeField] float mouseSensitivity = 200f;
    [SerializeField] float minPitch = -90f;
    [SerializeField] float maxPitch = 90f;

    [Header("Crouch")]
    [SerializeField] float crouchCameraYOffset = -0.5f;

    float pitch;
    float standLocalY;
    bool wasCrouching;

    public float Pitch => pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        standLocalY = transform.localPosition.y;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 0.01f;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 0.01f;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

        if (playerMovement != null && playerMovement.IsCrouching != wasCrouching)
        {
            wasCrouching = playerMovement.IsCrouching;
            Vector3 pos = transform.localPosition;
            pos.y = wasCrouching ? standLocalY + crouchCameraYOffset : standLocalY;
            transform.localPosition = pos;
        }
    }
}
