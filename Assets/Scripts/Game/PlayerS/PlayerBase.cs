using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public abstract class PlayerBase : MonoBehaviour
{
    private static readonly int MovingID = Animator.StringToHash("Moving");
    private static readonly int AirborneID = Animator.StringToHash("Airborne");
    private static readonly int RightID = Animator.StringToHash("Right");
    
    
    [SerializeField] protected float Acceleration;
    [SerializeField] protected float Deceleration;
    [SerializeField] protected float MaxSpeed;

    [SerializeField] protected float JumpForce;

    [SerializeField] protected LayerMask PlayerColMask;
    [SerializeField] protected LayerMask HumanTargetLayer;
    
    protected Rigidbody2D rb;
    protected BoxCollider2D box;
    
    protected Vector2 moveInput;
    
    protected Animator animator;

    public abstract GameTeam GetTeam();

    protected virtual float GetMaxSpeed()
    {
        return MaxSpeed;
    }

    protected virtual float GetAcceleration()
    {
        return Acceleration;
    }

    protected virtual float GetDeceleration()
    {
        return Deceleration;
    }

    protected virtual float GetJumpForce()
    {
        return JumpForce;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        MovementTick();
        UpdateAnims();
    }

    private static readonly float minTurnAmnt = 0.05f;
    
    protected virtual void UpdateAnims()
    {
        animator.SetBool(MovingID, Mathf.Abs(rb.linearVelocityX)  > 0.1f);
        animator.SetBool(AirborneID, !IsGrounded());
        if (moveInput.x > minTurnAmnt)
        {
            animator.SetBool(RightID, true);
        } else if (moveInput.x < -minTurnAmnt)
        {
            animator.SetBool(RightID, false);
        }
    }

    private void MovementTick()
    {
        if (Mathf.Abs(moveInput.x) <= 0.05)
        {
            RBUtils.XDecelRB(rb, GetDeceleration());
        }
        else
        {
            rb.AddForceX(moveInput.x * GetAcceleration() * Time.deltaTime);
        }
        
        RBUtils.LimitXSpeed(rb, GetMaxSpeed());

        cachedCrawlspace = 0;
    }

    public virtual void SetPlayerID(int id)
    {
        
    }

    public void Jump()
    {
        if (!IsGrounded())
            return;
        rb.AddForceY(GetJumpForce(), ForceMode2D.Impulse);
    }

    public void InputMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void InputJump(InputAction.CallbackContext context)
    {
        if(context.performed)
            Jump();
    }

    private int cachedCrawlspace;

    public bool IsInCrawlspace()
    {
        if (cachedCrawlspace == 0)
        {
            cachedCrawlspace = IsRBInCrawlspace(rb) ? 2 : 1;
        }
        return cachedCrawlspace == 2;
    }

    protected virtual ContactFilter2D GetColMask()
    {
        return new ContactFilter2D
        {
            layerMask = PlayerColMask,
            useLayerMask = true,
            useTriggers = false
        };
    }

    public bool IsGrounded()
    {
        Vector2 center = (Vector2)box.bounds.center - new Vector2(0, box.bounds.extents.y);
        var size = new Vector2(box.bounds.size.x * 0.95f, 0.1f);
        var cols = new Collider2D[2];
        Physics2D.OverlapBox(center, size, 0, GetColMask(), cols);
        return cols.Count(c => c && c != box) > 0;
    }
    
    public static bool IsPointInCrawlspace(Vector2 point)
    {
        return Physics2D.OverlapPoint(point, LayerMask.NameToLayer("CrawlspaceZone"));
    }

    public static bool IsRBInCrawlspace(Rigidbody2D rb)
    {
        List<Collider2D> col = new();
        ContactFilter2D contactFilter2D = new()
        {
            layerMask = 1 << LayerMask.NameToLayer("CrawlspaceZone"),
            useLayerMask = true,
            useTriggers = true,
        };
        return rb.Overlap(contactFilter2D, col) > 0;
    }

    private void OnDrawGizmos()
    {
        if (!rb)
            return;
        
        Handles.color = Color.gray;
        
        Handles.Label(transform.position, moveInput.ToString());

        Gizmos.color = Color.limeGreen;
        Vector2 center = (Vector2)box.bounds.center - new Vector2(0, box.bounds.extents.y);
        var size = new Vector2(box.bounds.size.x * 0.95f, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }

    public virtual void KillHuman(HumanBase target)
    {
        target.Kill();
        RoundManager.instance.AddToScore(GetTeam(), 1);
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    protected KillableHuman CheckForOverlappingHuman()
    {
        Collider2D humanCol = 
            Physics2D.OverlapBox(box.bounds.center, box.bounds.size, 0, HumanTargetLayer);

        if (!humanCol)
        {
            return null;
        }

        return humanCol.GetComponent<KillableHuman>();
    }
}
