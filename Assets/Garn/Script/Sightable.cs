using UnityEngine;

// Attach this to every monster/entity that should trigger a sanity hit
public class Sightable : MonoBehaviour
{
    // Unique ID per monster TYPE (not per instance) so seeing
    // the same *kind* of monster twice doesn't trigger it again.
    // Leave blank to make every individual monster unique instead.
    public string monsterID = "ShadowCreature";

    public float sanityDamage = 20f;

    [HideInInspector] public bool hasBeenSeen = false;
}