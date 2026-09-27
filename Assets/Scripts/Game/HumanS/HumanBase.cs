
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
    private static readonly int MovingID = Animator.StringToHash("Moving");
    private static readonly int RightAnimID = Animator.StringToHash("Right");
    private static readonly int DestYPalette = Shader.PropertyToID("_DestYPalette");
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
    protected Animator animator;
    protected BoxCollider2D box;

    private static Texture2D AllCombosColor;
    private static readonly int DstColorMapID = Shader.PropertyToID("_DstColorMap");

    private static void GenerateShirtAndPantsCombos()
    {
        List<Color> shirtColors = new()
        {
            new Color(0, 170 / 255.0f, 0, 1),
            new Color(114 / 255.0f, 79 / 255.0f, 1),
            new Color(1, 0, 89 / 255.0f)
        };

        List<Color> pantColors = new()
        {
            new Color(1, 0, 85 / 255.0f, 1),
            new Color(0, 25 / 255.0f, 87 / 255.0f),
            new Color(66 / 255.0f, 97 / 255.0f, 0)
        };

        int combos = shirtColors.Count * pantColors.Count;
        int colCount = 4;

        var tex = new Texture2D(colCount, combos);
        int row = 0;
        tex.filterMode = FilterMode.Point;

        var colors = new[]
        {
            Color.clear, Color.clear,
            new Color(1, 170 / 255.0f, 85 / 255.0f),
            new Color(2/3.0f,2/3.0f,2/3.0f)
        };

        for (int s = 0; s < shirtColors.Count; s++)
        {
            colors[0] = shirtColors[s];
            for (int p = 0; p < pantColors.Count; p++)
            {
                colors[1] = pantColors[s];
                tex.SetPixels(0, row, colCount, 1, colors);
                row++;
            }
        }

        AllCombosColor = tex;
        tex.Apply();
    }
    

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
        
        if(!AllCombosColor)
            GenerateShirtAndPantsCombos();
        
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        var sprRender = GetComponent<Renderer>();
        
        var propertyBlock = new MaterialPropertyBlock();
        
        sprRender.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(DestYPalette, Random.Range(0.0f, 1.0f));
        sprRender.SetPropertyBlock(propertyBlock);
        
        sprRender.material.SetTexture(DstColorMapID, AllCombosColor);
        
        GoIdle();
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
        UpdateAnims();
    }
    
    protected virtual void UpdateAnims()
    {
        animator.SetBool(MovingID, state != HumanState.Idle);
        if (rb.linearVelocityX > 0.1f)
        {
            animator.SetBool(RightAnimID, true);
        } else if (rb.linearVelocityX < -0.1f)
        {
            animator.SetBool(RightAnimID, false);
        }
    }

    protected virtual void IdleTick()
    {
        RBUtils.XDecelRB(rb, GetDeceleration());

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
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (state == HumanState.Walking)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, new Vector3(targetX, transform.position.y, 0));
        }
    }
    #endif
}
