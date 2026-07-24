using UnityEngine;

public class Grapple : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody body;
    [SerializeField] private Camera playerCamera;

    [Header("Grapple Settings")]
    [SerializeField] private float grappleRange = 30f;           // Increased for usable distance
    [SerializeField] private float grappleAcceleration = 25f;    // Pull force
    [SerializeField] private float minDistanceToTarget = 2f;     // Detaches when close enough
    [SerializeField] private LayerMask grappleableLayers = ~0;   // Layers you can hook onto

    public bool grappling { get; private set; } = false;
    private Vector3 targetPos;

    void Awake()
    {
        // Fallbacks if not assigned in Inspector
        if (body == null) body = GetComponent<Rigidbody>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        // 1. Handle Input in Update(), NEVER in FixedUpdate()
        if (Input.GetMouseButtonDown(1) && !grappling)
        {
            StartGrapple();
        }
        else if (grappling && (Input.GetMouseButtonUp(1) || Input.GetKeyDown(KeyCode.Space)))
        {
            StopGrapple();
        }
    }

    void FixedUpdate()
    {
        if (!grappling) return;

        float distance = Vector3.Distance(transform.position, targetPos);

        // Auto-stop when reaching target
        if (distance <= minDistanceToTarget)
        {
            StopGrapple();
            return;
        }

        // Apply force towards the grapple point
        Vector3 dir = (targetPos - transform.position).normalized;
        body.AddForce(dir * grappleAcceleration, ForceMode.Acceleration);
    }

    private void StartGrapple()
    {
        Ray lookRay = playerCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(lookRay, out RaycastHit hit, grappleRange, grappleableLayers))
        {
            grappling = true;
            targetPos = hit.point;
        }
    }

    private void StopGrapple()
    {
        grappling = false;
    }
}