using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AImove : MonoBehaviour
{
    [SerializeField] private float speed = 3.5f;
    [SerializeField, NavMeshAgentType] private int agentTypeID;
    [SerializeField] private Transform target;

    private NavMeshAgent agent;

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
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
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
        if (target == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireSphere(target.position, 0.3f);
    }
}

public class NavMeshAgentTypeAttribute : PropertyAttribute
{
}
