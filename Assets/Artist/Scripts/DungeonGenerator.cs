using System.Collections.Generic;
using UnityEngine;

public class SafeDungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Layout Settings")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private int targetRoomCount = 12;
    [SerializeField] private float roomSize = 20f;

    [Header("The 5 Core Room Prefabs")]
    [Tooltip("1 door facing NORTH (+Z)")]
    [SerializeField] private GameObject deadEndPrefab;

    [Tooltip("2 doors facing NORTH (+Z) & SOUTH (-Z)")]
    [SerializeField] private GameObject straightPrefab;

    [Tooltip("2 doors facing NORTH (+Z) & EAST (+X)")]
    [SerializeField] private GameObject cornerPrefab;

    [Tooltip("3 doors facing NORTH (+Z), EAST (+X), & SOUTH (-Z)")]
    [SerializeField] private GameObject tJunctionPrefab;

    [Tooltip("4 doors facing NORTH, EAST, SOUTH, & WEST")]
    [SerializeField] private GameObject crossroadPrefab;

    private bool[,] grid;
    private List<Vector2Int> occupiedPositions = new List<Vector2Int>();

    private void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        ClearOldDungeon();

        // SAFETY FIX 1: Cap rooms so they can never exceed total grid cells
        int maxCapacity = gridWidth * gridHeight;
        int safeTargetRooms = Mathf.Clamp(targetRoomCount, 1, maxCapacity);

        grid = new bool[gridWidth, gridHeight];
        occupiedPositions.Clear();

        CreateLayout(safeTargetRooms);
        SpawnRooms();
    }

    private void CreateLayout(int safeTargetRooms)
    {
        Vector2Int currentPos = new Vector2Int(gridWidth / 2, gridHeight / 2);
        grid[currentPos.x, currentPos.y] = true;
        occupiedPositions.Add(currentPos);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // SAFETY FIX 2: Hard breakout counter to prevent infinite while loops
        int safetyCounter = 0;
        int maxSafetyLimit = 10000;

        while (occupiedPositions.Count < safeTargetRooms)
        {
            safetyCounter++;
            if (safetyCounter > maxSafetyLimit)
            {
                Debug.LogWarning($"[DungeonGenerator] Infinite loop safety triggered! Generated {occupiedPositions.Count} / {safeTargetRooms} requested rooms.");
                break;
            }

            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            Vector2Int nextPos = currentPos + dir;

            // Ensure step stays within grid bounds
            if (nextPos.x >= 0 && nextPos.x < gridWidth && nextPos.y >= 0 && nextPos.y < gridHeight)
            {
                if (!grid[nextPos.x, nextPos.y])
                {
                    grid[nextPos.x, nextPos.y] = true;
                    occupiedPositions.Add(nextPos);
                }
                currentPos = nextPos;
            }
            else
            {
                // SAFETY FIX 3: Backtrack to a random existing room if trapped at an edge
                currentPos = occupiedPositions[Random.Range(0, occupiedPositions.Count)];
            }
        }
    }

    private void SpawnRooms()
    {
        foreach (Vector2Int pos in occupiedPositions)
        {
            bool n = IsCellOccupied(pos + Vector2Int.up);
            bool s = IsCellOccupied(pos + Vector2Int.down);
            bool e = IsCellOccupied(pos + Vector2Int.right);
            bool w = IsCellOccupied(pos + Vector2Int.left);

            GetPrefabAndRotation(n, s, e, w, out GameObject prefab, out float rotationY);

            if (prefab != null)
            {
                Vector3 spawnPos = new Vector3(pos.x * roomSize, 0f, pos.y * roomSize);
                Quaternion spawnRot = Quaternion.Euler(0f, rotationY, 0f);

                Instantiate(prefab, spawnPos, spawnRot, transform);
            }
        }
    }

    private void GetPrefabAndRotation(bool n, bool s, bool e, bool w, out GameObject prefab, out float rotationY)
    {
        int doorCount = (n ? 1 : 0) + (s ? 1 : 0) + (e ? 1 : 0) + (w ? 1 : 0);

        switch (doorCount)
        {
            case 1:
                prefab = deadEndPrefab;
                if (n) rotationY = 0f;
                else if (e) rotationY = 90f;
                else if (s) rotationY = 180f;
                else rotationY = 270f;
                break;

            case 2:
                if (n && s)
                {
                    prefab = straightPrefab;
                    rotationY = 0f;
                }
                else if (e && w)
                {
                    prefab = straightPrefab;
                    rotationY = 90f;
                }
                else
                {
                    prefab = cornerPrefab;
                    if (n && e) rotationY = 0f;
                    else if (e && s) rotationY = 90f;
                    else if (s && w) rotationY = 180f;
                    else rotationY = 270f;
                }
                break;

            case 3:
                prefab = tJunctionPrefab;
                if (!w) rotationY = 0f;
                else if (!n) rotationY = 90f;
                else if (!e) rotationY = 180f;
                else rotationY = 270f;
                break;

            case 4:
                prefab = crossroadPrefab;
                rotationY = Random.Range(0, 4) * 90f;
                break;

            default:
                prefab = null;
                rotationY = 0f;
                break;
        }
    }

    private bool IsCellOccupied(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= gridWidth || pos.y < 0 || pos.y >= gridHeight)
            return false;
        return grid[pos.x, pos.y];
    }

    private void ClearOldDungeon()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}