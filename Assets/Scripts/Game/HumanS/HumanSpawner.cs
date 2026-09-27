
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class HumanSpawner : MonoBehaviour
{
    private static readonly int OpenID = Animator.StringToHash("Open");
    private HashSet<HumanBase> humans = new();

    [SerializeField] private float RespawnTimeAtOneHuman;
    [SerializeField] private int TargetHumanCount;
    [SerializeField] private float BoundsFactor;

    [SerializeField] private GameObject[] HumanPrefabs;
    [SerializeField] private LayerMask HumanColMask;

    private BoxCollider2D inRangeDontSpawnBox;
    private Animator animator;
    [SerializeField] private ContactFilter2D PlayerContactFilter;

    private float timer;

    private void Start()
    {
        for (int i = 0; i < TargetHumanCount; i++)
        {
            SpawnHumanExisting();
        }

        animator = GetComponent<Animator>();
        inRangeDontSpawnBox = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        humans.RemoveWhere(h => !h);
        
        int needToSpawn = TargetHumanCount - humans.Count;

        var cols = new Collider2D[4];
        inRangeDontSpawnBox.Overlap(PlayerContactFilter, cols);
        if (!cols.Any(c => c && c.GetComponent<PlayerBase>()))
        {
            timer += (1/RespawnTimeAtOneHuman) * needToSpawn * Time.deltaTime;
            if (timer >= 1)
            {
                SpawnHuman();
            }
        }
    }

    public void SpawnHumanEvent()
    {
        GameObject human = Instantiate(PickRandomHumanPrefab());
        var humanComp = human.GetComponent<HumanBase>();
        human.transform.position = transform.position;

        humans.Add(humanComp);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void SpawnHuman()
    {
        timer = 0;
        animator.SetTrigger(OpenID);
    }

    private GameObject PickRandomHumanPrefab()
    {
        return HumanPrefabs[Random.Range(0, HumanPrefabs.Length)];
    }

    private void SpawnHumanExisting()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, Vector2.left, 999f, HumanColMask);
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, Vector2.right, 999f, HumanColMask);
        float leftDist = leftHit.distance - BoundsFactor;
        float rightDist = rightHit.distance - BoundsFactor;

        float xPos = Random.Range(-leftDist, rightDist);

        GameObject human = Instantiate(PickRandomHumanPrefab());
        var humanComp = human.GetComponent<HumanBase>();
        human.transform.position = transform.position + Vector3.right * xPos;
        humans.Add(humanComp);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, Vector2.left, 999f, HumanColMask);
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, Vector2.right, 999f, HumanColMask);
        float leftDist = leftHit.distance - BoundsFactor;
        float rightDist = rightHit.distance - BoundsFactor;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position - (Vector3.right * leftDist), transform.position + (Vector3.right * rightDist)); 
    }
    #endif
}
