using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] objectPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        SpawnAtRandom();
    }

    private void SpawnAtRandom()
    {
        if (spawnPoints.Length == 0) return;

        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        foreach (GameObject prefab in objectPrefabs)
        {
            if (availablePoints.Count == 0) break;

            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform selectedPoint = availablePoints[randomIndex];

            Instantiate(prefab, selectedPoint.position, selectedPoint.rotation);

            availablePoints.RemoveAt(randomIndex);
        }
    }
}