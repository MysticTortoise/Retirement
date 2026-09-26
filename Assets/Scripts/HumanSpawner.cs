
using System;
using System.Collections.Generic;
using UnityEngine;

public class HumanSpawner : MonoBehaviour
{
    private HashSet<HumanBase> humans = new();

    [SerializeField] private float RespawnTimeAtOneHuman;
    [SerializeField] private int TargetHumanCount;

    [SerializeField] private GameObject HumanPrefab;

    private float timer;

    private void Update()
    {
        humans.RemoveWhere(h => !h);
        
        int needToSpawn = TargetHumanCount - humans.Count;

        timer += (1/RespawnTimeAtOneHuman) * needToSpawn * Time.deltaTime;
        if (timer >= 1)
        {
            SpawnHuman();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void SpawnHuman()
    {
        timer = 0;
        GameObject human = Instantiate(HumanPrefab);
        var humanComp = human.GetComponent<HumanBase>();
        human.transform.position = transform.position;

        humans.Add(humanComp);
    }
}
