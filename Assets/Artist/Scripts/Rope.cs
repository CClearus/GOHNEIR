using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HangingRope : MonoBehaviour
{
    [Header("Rope Dimensions")]
    [SerializeField] private float ropeLength = 8f;
    [SerializeField] private float grabRadius = 3f;

    [Header("Controls")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Physics & Motion")]
    [SerializeField] private float swingForce = 35f;
    [SerializeField] private float jumpBoost = 8f;
    [SerializeField] private float spring = 500f;
    [SerializeField] private float damper = 10f;

    private LineRenderer lineRenderer;
    private Transform playerTransform;
    private Rigidbody playerRb;
    private SpringJoint joint;
    private bool isSwinging = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        // Fix Unity Culling and World Space bugs on initialization
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        lineRenderer.allowOcclusionWhenDynamic = false;
    }

    private void Update()
    {
        // Find player physics root
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerRb = playerObj.GetComponentInParent<Rigidbody>();
                if (playerRb != null)
                {
                    playerTransform = playerRb.transform;
                }
            }
            return;
        }

        Vector3 ropeBottomPos = GetRopeBottomPosition();
        float distanceToRopeBottom = Vector3.Distance(playerTransform.position, ropeBottomPos);

        // Attach / Detach hotkey
        if (Input.GetKeyDown(interactKey))
        {
            if (!isSwinging && distanceToRopeBottom <= grabRadius)
            {
                AttachPlayer();
            }
            else if (isSwinging)
            {
                DetachPlayer();
            }
        }

        // Jump off logic
        if (isSwinging && Input.GetKeyDown(jumpKey))
        {
            JumpOff();
        }
    }

    private void FixedUpdate()
    {
        if (isSwinging && playerRb != null)
        {
            ApplySwingForce();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private Vector3 GetRopeBottomPosition()
    {
        return transform.position + (Vector3.down * ropeLength);
    }

    private void AttachPlayer()
    {
        isSwinging = true;

        joint = playerTransform.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = transform.position;

        float currentDist = Vector3.Distance(playerTransform.position, transform.position);
        joint.maxDistance = Mathf.Min(currentDist, ropeLength);
        joint.minDistance = 0f;

        joint.spring = spring;
        joint.damper = damper;
    }

    private void DetachPlayer()
    {
        isSwinging = false;
        if (joint != null)
        {
            Destroy(joint);
        }
    }

    private void JumpOff()
    {
        DetachPlayer();
        if (playerRb != null)
        {
            playerRb.AddForce(Vector3.up * jumpBoost, ForceMode.Impulse);
        }
    }

    private void ApplySwingForce()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Transform cam = Camera.main != null ? Camera.main.transform : transform;
        Vector3 direction = (cam.forward * vertical + cam.right * horizontal);
        direction.y = 0;

        playerRb.AddForce(direction.normalized * swingForce, ForceMode.Force);
    }

    private void DrawRope()
    {
        if (lineRenderer == null) return;

        Vector3 startPos = transform.position;
        Vector3 endPos;

        if (isSwinging && playerTransform != null)
        {
            // Offset point +1 unit up so it attaches near shoulders/head, NOT inside character mesh
            endPos = playerTransform.position - (Vector3.up * 1f);
        }
        else
        {
            endPos = GetRopeBottomPosition();
        }

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        // Fallback: Red line in Scene view to verify calculation independently
        Debug.DrawLine(startPos, endPos, Color.red);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 bottom = GetRopeBottomPosition();
        Gizmos.DrawLine(transform.position, bottom);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(bottom, grabRadius);
    }
}