
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum HumanState
{
    Idle,
    Walking,
    Attacked,
}

public class HumanBase : MonoBehaviour
{
    [SerializeField] protected float MoveSpeed;
    [SerializeField] protected float Acceleration;
    [SerializeField] protected float Deceleration;

    [SerializeField] protected LayerMask HumanColMask;
    
    [SerializeField] protected float MinIdleWalkAmount;
    [SerializeField] protected float NearEnoughAmount;

    [SerializeField] protected float MinIdleTime;
    [SerializeField] protected float MaxIdleTime;

    private float targetX;
    private HumanState state;

    [NonSerialized] public Rigidbody2D rb;
    protected BoxCollider2D box;


    protected virtual float GetAcceleration()
    {
        return Acceleration;
    }

    protected virtual float GetDeceleration()
    {
        return Deceleration;
    }

    protected virtual float GetMaxSpeed()
    {
        return MoveSpeed;
    }

    private void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        BeginIdleWalk();
    }

    protected virtual void Update()
    {
        switch (state)
        {
            case HumanState.Idle:
                IdleTick();
                break;
            case HumanState.Walking:
                WalkTick();
                break;
            default:
                break;
        }

        RBUtils.LimitXSpeed(rb, GetMaxSpeed());
    }

    protected virtual void IdleTick()
    {
        rb.AddForceX(RBUtils.GetDecelSpeed(rb, GetDeceleration()));

        targetX -= Time.deltaTime;
        if (targetX <= 0)
        {
            BeginIdleWalk();
        }
    }

    protected virtual void WalkTick()
    {
        if (Mathf.Abs(transform.position.x - targetX) < NearEnoughAmount)
        {
            GoIdle();
        }

        float sign = Mathf.Sign(targetX - transform.position.x);
        rb.AddForceX(Acceleration * sign * Time.deltaTime);
    }

    protected float GetMaxDistanceInDirection(bool right)
    {
        float sign = right ? 1 : -1;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * sign, 999f, HumanColMask);
        return hit.distance;
    }

    protected virtual void EnterState(HumanState newState)
    {
        state = newState;
    }

    protected void GoIdle()
    {
        EnterState(HumanState.Idle);
        targetX = Random.Range(MinIdleTime, MaxIdleTime);
    }

    public void BeginIdleWalk()
    {
        float leftMaxDist = GetMaxDistanceInDirection(false);
        float rightMaxDist = GetMaxDistanceInDirection(true);

        float rand = Random.Range(0f, leftMaxDist + rightMaxDist);
        bool dir = rand > leftMaxDist;
        float maxDist = dir ? rightMaxDist : leftMaxDist;

        float signedDir = dir ? 1 : -1;

        float amountToWalk = Random.Range(MinIdleWalkAmount, maxDist - box.bounds.size.x) * signedDir;
        targetX = transform.position.x + amountToWalk;
        EnterState(HumanState.Walking);
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (state == HumanState.Walking)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, new Vector3(targetX, transform.position.y, 0));
        }
    }
}
