using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Base Exits (Unrotated state)")]
    public bool openNorth;
    public bool openSouth;
    public bool openEast;
    public bool openWest;

    /// <summary>
    /// Checks if this room layout matches required exits when rotated by Y degrees.
    /// </summary>
    public bool MatchesExitsWithRotation(bool reqN, bool reqS, bool reqE, bool reqW, int yRotation)
    {
        int steps = (yRotation / 90) % 4;

        bool effN = GetRotatedExit(steps, 0); // 0 = North
        bool effE = GetRotatedExit(steps, 1); // 1 = East
        bool effS = GetRotatedExit(steps, 2); // 2 = South
        bool effW = GetRotatedExit(steps, 3); // 3 = West

        return effN == reqN && effS == reqS && effE == reqE && effW == reqW;
    }

    private bool GetRotatedExit(int steps, int targetDir)
    {
        bool[] original = { openNorth, openEast, openSouth, openWest };
        int originalIndex = (targetDir - steps + 4) % 4;
        return original[originalIndex];
    }
}