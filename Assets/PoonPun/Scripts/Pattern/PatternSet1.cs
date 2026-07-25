using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternSet1", menuName = "Scriptable Objects/PatternSet1")]
public class PatternSet1 : ScriptableObject
{
    [SerializeField] private List<BulletPattern> patterns = new List<BulletPattern>();

    public List<BulletPattern> Patterns => patterns;

    public BulletPattern GetPatternById(string id)
    {
        foreach (BulletPattern pattern in patterns)
        {
            if (pattern.id == id) return pattern;
        }
        return null;
    }
}

[System.Serializable]
public class BulletPattern
{
    public string id;
    public PatternSet1Att attackScript;
    [Tooltip("Delay after this attack fully finishes before it can be fired again.")]
    public float cooldownAfterAttack = 2f;
    public List<BulletData> bullets = new List<BulletData>();
}

[System.Serializable]
public class BulletData
{
    public GameObject bulletPrefab;

    [InspectorName("M1 Target - Spawn/Loop Position")]
    [Tooltip("Where the carrier travels to from the enemy's own position (m1). Speed/duration for this leg is set on PatternSet1Att's 'M1 Approach Duration' field.")]
    public Vector3 spawnPosition;

    [InspectorName("M2 - Shot Move / Waypoints")]
    [Tooltip("Index 0 pattern: moveSteps[0] is the trajectory (m2) each looped shot travels at normal bullet speed. Other patterns: walked through in order as plain waypoints.")]
    public List<BulletMoveStep> moveSteps = new List<BulletMoveStep>();

    public float damage;
}

[System.Serializable]
public class BulletMoveStep
{
    [Tooltip("How far to move (local offset relative to the firer), e.g. (60,0,0) = move 60 units on X.")]
    public Vector3 move;
    [Tooltip("How long this move takes, in seconds. E.g. move=60 on X with duration=1 moves 60 units in 1 second.")]
    public float duration = 1f;
}
