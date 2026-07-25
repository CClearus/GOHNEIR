using System.Collections.Generic;
using UnityEngine;

public class RandomPrefabSpawner : MonoBehaviour
{
    [Header("Prefab Setup")]
    [Tooltip("The prefab to spawn (or leave empty if using the array below).")]
    [SerializeField] private GameObject prefabToSpawn;

    [Tooltip("Optional: Pool of different prefabs to pick from randomly.")]
    [SerializeField] private GameObject[] prefabPool;

    [Header("Position Setup")]
    [Tooltip("The parent GameObject containing all spawn point Transforms as its children.")]
    [SerializeField] private Transform spawnPositionsParent;

    [Tooltip("Number of instances to spawn (K positions).")]
    [SerializeField] private int kInstances = 5;

    [Header("Hierarchy Organization")]
    [Tooltip("Optional parent container to keep spawned instances organized.")]
    [SerializeField] private Transform spawnParent;

    private void Start()
    {
        SpawnPrefabs();
    }

    /// <summary>
    /// Spawns K instances at randomly chosen positions without duplicates.
    /// </summary>
    public void SpawnPrefabs()
    {
        // 1. Gather child transforms from the designated parent object
        List<Transform> spawnPositions = GetPositionsFromParent();

        if (spawnPositions.Count == 0)
        {
            Debug.LogWarning("[RandomPrefabSpawner] No valid spawn positions found under parent!");
            return;
        }

        if (prefabToSpawn == null && (prefabPool == null || prefabPool.Length == 0))
        {
            Debug.LogWarning("[RandomPrefabSpawner] No prefabs assigned to spawn!");
            return;
        }

        // 2. Ensure K does not exceed the total available child positions N
        int countToSpawn = Mathf.Clamp(kInstances, 0, spawnPositions.Count);

        // 3. Pick K unique positions using Fisher-Yates sampling
        List<Transform> selectedPositions = GetRandomUniqueElements(spawnPositions, countToSpawn);

        // 4. Instantiate prefabs at selected positions
        foreach (Transform targetTransform in selectedPositions)
        {
            GameObject selectedPrefab = GetPrefabToInstantiate();

            if (selectedPrefab != null)
            {
                Instantiate(selectedPrefab, targetTransform.position, targetTransform.rotation, spawnParent);
            }
        }
    }

    /// <summary>
    /// Retrieves all immediate child Transforms of spawnPositionsParent.
    /// </summary>
    private List<Transform> GetPositionsFromParent()
    {
        List<Transform> positions = new List<Transform>();

        if (spawnPositionsParent == null)
        {
            Debug.LogWarning("[RandomPrefabSpawner] Spawn Positions Parent is not assigned!");
            return positions;
        }

        // Iterating over a Transform directly loops through all its immediate child Transforms
        foreach (Transform child in spawnPositionsParent)
        {
            positions.Add(child);
        }

        return positions;
    }

    /// <summary>
    /// Partial Fisher-Yates shuffle: picks 'count' unique items randomly in O(K) time complexity.
    /// </summary>
    private List<T> GetRandomUniqueElements<T>(List<T> sourceList, int count)
    {
        List<T> pool = new List<T>(sourceList);
        List<T> result = new List<T>(count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(i, pool.Count);

            T temp = pool[i];
            pool[i] = pool[randomIndex];
            pool[randomIndex] = temp;

            result.Add(pool[i]);
        }

        return result;
    }

    /// <summary>
    /// Helper to pick either the single prefab or a random prefab from the pool.
    /// </summary>
    private GameObject GetPrefabToInstantiate()
    {
        if (prefabPool != null && prefabPool.Length > 0)
        {
            return prefabPool[Random.Range(0, prefabPool.Length)];
        }
        return prefabToSpawn;
    }
}