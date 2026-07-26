using System.Collections;
using UnityEngine;

public class MoveAndDisableCapsule : MonoBehaviour
{
    /// <summary>
    /// Starts the movement process in a given direction over a set distance.
    /// </summary>
    /// <param name="direction">World direction to move (e.g., Vector3.forward).</param>
    /// <param name="speed">Units per second.</param>
    /// <param name="distance">Total distance to travel before disabling.</param>
    public void MoveThenDisable(Vector3 direction, float speed, float distance)
    {
        StartCoroutine(MoveRoutine(direction, speed, distance));
    }

    private IEnumerator MoveRoutine(Vector3 direction, float speed, float distance)
    {
        Vector3 normalizedDir = direction.normalized;
        float distanceTraveled = 0f;

        while (distanceTraveled < distance)
        {
            // Calculate step size based on frame time
            float moveStep = speed * Time.deltaTime;

            // Prevent moving past the target distance on the final frame
            if (distanceTraveled + moveStep > distance)
            {
                moveStep = distance - distanceTraveled;
            }

            // Move the transform
            transform.Translate(normalizedDir * moveStep, Space.World);
            distanceTraveled += moveStep;

            // Wait until the next frame
            yield return null; 
        }

        // Disable the object once movement completes
        gameObject.SetActive(false);
    }
}