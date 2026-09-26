using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerBase : MonoBehaviour
{
    [SerializeField] protected float Acceleration;
    [SerializeField] protected float Deceleration;
    [SerializeField] protected float MaxSpeed;

    [SerializeField] protected float JumpForce;

    [SerializeField] protected LayerMask PlayerColMask;
    
    private Rigidbody2D rb;
    private BoxCollider2D box;
    
    private Vector2 moveInput;

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
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        MovementTick();
    }

    private void MovementTick()
    {
        if (Mathf.Abs(moveInput.x) <= 0.01)
        {
            rb.AddForceX(RBUtils.GetDecelSpeed(rb, GetDeceleration()));
        }
        else
        {
            rb.AddForceX(moveInput.x * GetAcceleration() * Time.deltaTime);
        }
        
        RBUtils.LimitXSpeed(rb, GetMaxSpeed());

        cachedCrawlspace = 0;
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

    public bool IsGrounded()
    {
        Vector2 center = (Vector2)box.bounds.center - new Vector2(0, box.bounds.extents.y);
        var size = new Vector2(box.bounds.size.x * 0.95f, 0.1f);
        ContactFilter2D contactFilter2D = new()
        {
            layerMask = PlayerColMask,
            useLayerMask = true,
            useTriggers = false
        };
        var cols = new Collider2D[2];
        Physics2D.OverlapBox(center, size, 0, contactFilter2D, cols);
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
        Handles.color = Color.gray;
        
        Handles.Label(transform.position, "is in cs: " + IsInCrawlspace());

        Gizmos.color = Color.limeGreen;
        Vector2 center = (Vector2)box.bounds.center - new Vector2(0, box.bounds.extents.y);
        var size = new Vector2(box.bounds.size.x * 0.95f, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}
