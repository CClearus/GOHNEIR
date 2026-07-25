using UnityEngine;
using System.Collections.Generic;

public class PlayerVision : MonoBehaviour
{
    public float viewDistance = 5f;
    [Range(0, 180)] public float viewAngle = 10f;
    public LayerMask sightBlockingLayers;
    public LayerMask monsterLayer;

    private static HashSet<string> seenMonsterIDs = new HashSet<string>();

    void Update()
    {
        CheckForMonsters();
    }

    void CheckForMonsters()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance, monsterLayer);

        foreach (Collider hit in hits)
        {
            Sightable sightable = hit.GetComponent<Sightable>();
            if (sightable == null || sightable.hasBeenSeen) continue;
            if (seenMonsterIDs.Contains(sightable.monsterID)) continue;

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle < viewAngle / 2f)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);

                if (!Physics.Raycast(transform.position, dirToTarget, distance, sightBlockingLayers))
                {
                    RegisterSighting(sightable);
                }
            }
        }
    }

    void RegisterSighting(Sightable sightable)
    {
        sightable.hasBeenSeen = true;
        seenMonsterIDs.Add(sightable.monsterID);

        // Decrease sanity
        SanitySystem.Instance.LoseSanity(sightable.sanityDamage);

        Debug.Log($"First sighting of {sightable.monsterID}! Sanity hit applied.");
    }
}