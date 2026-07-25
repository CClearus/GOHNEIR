using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HallucinationManager : MonoBehaviour
{
    [Header("References")]
    public SanitySystem sanity;
    public Transform player;


    [Header("Normal Hallucinations")]
    public GameObject[] hallucinationPrefabs;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;


    [Header("Normal Hallucination Delay")]
    public float minSpawnDelay = 5f;
    public float maxSpawnDelay = 15f;


    [Header("Hallucination Bullet")]
    public GameObject hallucinationBulletPrefab;
    public float bulletCooldown = 8f;
    public float bulletSpawnDistance = 30f;


    private GameObject currentHallucination;

    private List<int> remainingHallucinations = new List<int>();


    void Start()
    {
        ResetHallucinationPool();

        StartCoroutine(HallucinationLoop());
        StartCoroutine(BulletHallucinationLoop());
    }



    void ResetHallucinationPool()
    {
        remainingHallucinations.Clear();

        for (int i = 0; i < hallucinationPrefabs.Length; i++)
        {
            remainingHallucinations.Add(i);
        }
    }



    IEnumerator HallucinationLoop()
    {
        while (true)
        {
            // Normal hallucinations at 0 sanity
            if (sanity.currentSanity <= 0 && currentHallucination == null)
            {
                SpawnHallucination();
            }


            yield return new WaitForSeconds(
                Random.Range(minSpawnDelay, maxSpawnDelay)
            );
        }
    }



    IEnumerator BulletHallucinationLoop()
    {
        while(true)
        {
            // Bullet hallucination happens between 50 and 100 sanity
            if(sanity.currentSanity >= 0 && sanity.currentSanity <= 100)
            {
                TrySpawnBullet();
            }


            yield return new WaitForSeconds(bulletCooldown);
        }
    }



    void TrySpawnBullet()
    {
        // 100 sanity = rare
        // 50 sanity = common

        float chance = Mathf.Lerp(
            0.05f,
            0.4f,
            (100 - sanity.currentSanity) / 50f
        );


        if(Random.value <= chance)
        {
            SpawnBullet();
        }
    }



    void SpawnBullet()
    {
        Vector3 randomDirection = Random.onUnitSphere;

        Vector3 spawnPosition =
            player.position + randomDirection * bulletSpawnDistance;


        Instantiate(
            hallucinationBulletPrefab,
            spawnPosition,
            Quaternion.identity
        );


        Debug.Log("Hallucination bullet spawned");
    }



    void SpawnHallucination()
    {
        if (hallucinationPrefabs.Length == 0 || spawnPoints.Length == 0)
            return;


        if (remainingHallucinations.Count == 0)
        {
            ResetHallucinationPool();
        }


        int randomListIndex =
            Random.Range(0, remainingHallucinations.Count);


        int prefabIndex =
            remainingHallucinations[randomListIndex];


        remainingHallucinations.RemoveAt(randomListIndex);


        Transform spawn =
            spawnPoints[Random.Range(0, spawnPoints.Length)];


        currentHallucination = Instantiate(
            hallucinationPrefabs[prefabIndex],
            spawn.position,
            spawn.rotation
        );
    }



    public void HallucinationDisappeared()
    {
        currentHallucination = null;
    }
}