using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AImove : MonoBehaviour
{
    [SerializeField] private float speed = 3.5f;
    [SerializeField, NavMeshAgentType] private int agentTypeID;
    [SerializeField] private Transform target;

    [Header("Attack")]
    [SerializeField] private PatternSet1 attackPatternData;
    [SerializeField] private string attackPatternId = "1_1";

    [Header("Player Detection")]
    [Tooltip("Detection ignores walls/obstacles entirely - the AI always knows the player's position within range/FOV, as if predicting through cover.")]
    [SerializeField] private float detectionRange = 50f;
    [SerializeField, Range(0f, 360f)] private float detectionAngle = 90f;
    [SerializeField] private float stopDistance = 50f;
    [SerializeField] private float attackRotationSpeed = 180f;

    [Header("Cooldown Strafe (movement while waiting to attack again)")]
    [Tooltip("While on cooldown after an attack, the AI picks a random point within this radius of the player and strafes toward it, still rotating to face the player the whole time.")]
    [SerializeField] private float strafeRadius = 6f;
    [Tooltip("How often a new random strafe destination is picked while on cooldown.")]
    [SerializeField] private float strafeRetargetInterval = 1.5f;

    [Header("Aim Lock (must aim within this margin before firing)")]
    [Tooltip("General aim tolerance in degrees for any non-carrier pattern - the AI keeps rotating toward a perfect straight-line lock and only fires once within this margin. Larger = more likely to fire slightly off and miss.")]
    [SerializeField] private float aimMarginOfError = 15f;
    [Tooltip("Stricter aim tolerance required specifically for the carrier pattern (index 0 in the pattern data) - e.g. 5 degrees requires a near-perfect straight-line lock before it fires.")]
    [SerializeField] private float carrierPatternAimMargin = 5f;

    private NavMeshAgent agent;
    private bool isAttacking;
    private bool isLockedFiring;
    private float nextAttackAllowedTime;
    private float nextStrafeRetargetTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        ApplySettings();
    }

    private void OnValidate()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        ApplySettings();
    }

    private void ApplySettings()
    {
        if (agent == null) return;
        agent.speed = speed;
        agent.agentTypeID = agentTypeID;
    }

    private void Update()
    {
        if (target == null) return;

        bool playerDetected = DetectPlayer();
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Once locked into a carrier attack, stay stopped/facing-locked for its whole
        // duration even if detection blips (e.g. a freshly spawned carrier/bullet
        // briefly blocking the detection raycast) - only the pattern's own onComplete
        // callback should release this, not a transient loss of line-of-sight.
        if (isLockedFiring || (playerDetected && distanceToTarget <= stopDistance))
        {
            agent.updateRotation = false;

            bool onCooldown = !isLockedFiring && Time.time < nextAttackAllowedTime;
            if (onCooldown)
            {
                if (Time.time >= nextStrafeRetargetTime)
                {
                    nextStrafeRetargetTime = Time.time + strafeRetargetInterval;
                    Vector2 randomOffset = Random.insideUnitCircle * strafeRadius;
                    Vector3 strafeDestination = target.position + new Vector3(randomOffset.x, 0f, randomOffset.y);
                    agent.SetDestination(strafeDestination);
                }
                agent.isStopped = false;
            }
            else
            {
                agent.isStopped = true;
            }

            float aimAngle = float.MaxValue;
            if (!isLockedFiring)
            {
                Vector3 lookDirection = target.position - transform.position;
                lookDirection.y = 0f;
                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, attackRotationSpeed * Time.deltaTime);
                    aimAngle = Vector3.Angle(transform.forward, lookDirection);
                }
            }

            isAttacking = true;
            if (!isLockedFiring && Time.time >= nextAttackAllowedTime)
            {
                BulletPattern pattern = attackPatternData != null ? attackPatternData.GetPatternById(attackPatternId) : null;
                if (pattern != null && pattern.attackScript != null)
                {
                    bool isCarrierPattern = pattern.attackScript.IsCarrierPattern(attackPatternData, pattern);
                    float requiredAimMargin = isCarrierPattern ? carrierPatternAimMargin : aimMarginOfError;

                    // Wait for a near-perfect straight-line lock before firing - the AI keeps
                    // rotating every frame above, this just gates when it's allowed to shoot.
                    if (aimAngle <= requiredAimMargin)
                    {
                        if (isCarrierPattern)
                        {
                            isLockedFiring = true;
                            pattern.attackScript.Fire(attackPatternData, pattern, transform, this, () => isLockedFiring = false);
                        }
                        else
                        {
                            pattern.attackScript.Fire(attackPatternData, pattern, transform, this);
                        }
                        nextAttackAllowedTime = Time.time + pattern.cooldownAfterAttack;
                    }
                }
                else
                {
                    Debug.LogWarning($"{name}: could not fire - attackPatternData={(attackPatternData != null)}, " +
                        $"looked up id='{attackPatternId}' found pattern={(pattern != null)}, " +
                        $"attackScript assigned={(pattern != null && pattern.attackScript != null)}.");
                }
            }
        }
        else
        {
            isAttacking = false;
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(target.position);
        }
    }

    private bool DetectPlayer()
    {
        if (target == null) return false;

        Vector3 offset = target.position - transform.position;
        if (offset.magnitude > detectionRange) return false;

        float angleToTarget = Vector3.Angle(transform.forward, offset);
        return angleToTarget <= detectionAngle * 0.5f;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (agent != null) agent.speed = speed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 leftBoundary = Quaternion.Euler(0f, -detectionAngle * 0.5f, 0f) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0f, detectionAngle * 0.5f, 0f) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        if (target == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireSphere(target.position, 0.3f);
    }
}


public class NavMeshAgentTypeAttribute : PropertyAttribute
{
}
