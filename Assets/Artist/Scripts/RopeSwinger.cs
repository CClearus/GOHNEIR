using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FixedAnchorRopeSwing : MonoBehaviour
{
    [Header("Anchor Setup")]
    [SerializeField] private Transform ropeAnchor;
    [SerializeField] private float maxAttachDistance = 20f;
    [SerializeField] private float maxRopeLength = 12f;
    [Tooltip("How long the rope hangs down when nobody is swinging on it.")]
    [SerializeField] private float idleRopeLength = 8f; 
    [SerializeField] private KeyCode swingKey = KeyCode.E;

    [Header("Jump & Detach")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private float jumpBoost = 8f;

    [Header("Visuals")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform ropeOrigin; // Hand or character attachment point

    [Header("Cable Physics")]
    [SerializeField] private float swingForce = 35f;
    [SerializeField] private float spring = 500f;
    [SerializeField] private float damper = 10f;
    [SerializeField] private float massScale = 4.5f;

    private SpringJoint joint;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (lineRenderer != null)
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2; // Always keep 2 points active
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(swingKey))
        {
            if (joint == null) TryStartSwing();
            else StopSwing();
        }

        if (joint != null && Input.GetKeyDown(jumpKey))
        {
            JumpOffRope();
        }
    }

    private void FixedUpdate()
    {
        if (joint != null)
        {
            ApplySwingForce();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void TryStartSwing()
    {
        if (ropeAnchor == null) return;

        float distance = Vector3.Distance(transform.position, ropeAnchor.position);

        if (distance <= maxAttachDistance)
        {
            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = ropeAnchor.position;

            float startingLength = Mathf.Min(distance, maxRopeLength);
            joint.maxDistance = startingLength;
            joint.minDistance = 0f;

            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;
        }
    }

    private void JumpOffRope()
    {
        StopSwing();
        rb.AddForce(Vector3.up * jumpBoost, ForceMode.Impulse);
    }

    public void StopSwing()
    {
        if (joint != null)
        {
            Destroy(joint);
        }
    }

    private void ApplySwingForce()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Transform cam = Camera.main != null ? Camera.main.transform : transform;
        Vector3 direction = (cam.forward * vertical + cam.right * horizontal);
        direction.y = 0;

        rb.AddForce(direction.normalized * swingForce, ForceMode.Force);
    }

    private void DrawRope()
    {
        if (lineRenderer == null || ropeAnchor == null) return;

        lineRenderer.positionCount = 2;

        if (joint != null)
        {
            // SWINGING STATE: Draw line from Player/Hand to Anchor Point
            Vector3 startPos = (ropeOrigin != null) ? ropeOrigin.position : transform.position;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, ropeAnchor.position);
        }
        else
        {
            // IDLE STATE: Draw default hanging rope vertically down from Anchor
            Vector3 anchorTop = ropeAnchor.position;
            Vector3 idleBottom = ropeAnchor.position + (Vector3.down * idleRopeLength);

            lineRenderer.SetPosition(0, anchorTop);
            lineRenderer.SetPosition(1, idleBottom);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (ropeAnchor != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ropeAnchor.position, maxAttachDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ropeAnchor.position, maxRopeLength);

            // Draw default hanging rope preview line in Editor (Yellow)
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(ropeAnchor.position, ropeAnchor.position + (Vector3.down * idleRopeLength));
        }
    }
}