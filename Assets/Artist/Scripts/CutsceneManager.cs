using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public MoveAndDisableCapsule capsuleScript;
    void OnTriggerEnter(Collider other)
    {
        capsuleScript.MoveThenDisable(Vector3.back, speed: 25f, distance: 50f);
    }
}
