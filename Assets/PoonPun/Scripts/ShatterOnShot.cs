using UnityEngine;

public interface IShotImpactReceiver
{
    void ReceiveShotImpact(Vector3 hitPoint, Vector3 shotDirection, float force);
}

// Attach to the parent of a set of already-broken-apart child pieces (e.g. a crate
// modeled as several separate chunks parented under one empty). On the first pellet
// hit the children are unparented and switched to physics-driven Rigidbodies; every
// pellet hit (including the first) then applies an explosion force from that pellet's
// exact impact point, so pieces near the shot scatter harder than pieces further away.
public class ShatterOnShot : MonoBehaviour, IShotImpactReceiver
{
    [Header("Explosion Force")]
    [Tooltip("Multiplies the force passed in from the gun (GunToggle's Shot Impact Force).")]
    [SerializeField] private float forceMultiplier = 1f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float upwardsModifier = 0.5f;
    [Tooltip("Extra random spin applied to each piece so they don't all tumble identically.")]
    [SerializeField] private float randomTorque = 5f;

    [Header("Cleanup")]
    [Tooltip("Disables the parent's own Renderer/Collider once shattered (the intact prop disappears, leaving only the flying pieces).")]
    [SerializeField] private bool disableParentVisuals = true;
    [Tooltip("Seconds before debris pieces are destroyed. 0 = never.")]
    [SerializeField] private float debrisLifetime = 8f;

    private Rigidbody[] childBodies;
    private bool activated;

    public void ReceiveShotImpact(Vector3 hitPoint, Vector3 shotDirection, float force)
    {
        if (!activated) Activate();

        float appliedForce = force * forceMultiplier;
        foreach (Rigidbody body in childBodies)
        {
            if (body == null) continue;
            body.AddExplosionForce(appliedForce, hitPoint, explosionRadius, upwardsModifier, ForceMode.Impulse);
            body.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);
        }
    }

    private void Activate()
    {
        activated = true;

        int childCount = transform.childCount;
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++) children[i] = transform.GetChild(i);

        childBodies = new Rigidbody[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = children[i];
            child.SetParent(null, worldPositionStays: true);

            Rigidbody body = child.GetComponent<Rigidbody>();
            if (body == null) body = child.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = false;
            body.useGravity = true;

            childBodies[i] = body;

            if (debrisLifetime > 0f) Destroy(child.gameObject, debrisLifetime);
        }

        if (disableParentVisuals)
        {
            if (TryGetComponent(out Renderer parentRenderer)) parentRenderer.enabled = false;
            if (TryGetComponent(out Collider parentCollider)) parentCollider.enabled = false;
        }
    }
}
