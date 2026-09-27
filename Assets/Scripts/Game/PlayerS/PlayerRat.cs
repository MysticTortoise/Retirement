
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRat : PlayerBase
{
    private static readonly int AttachedID = Animator.StringToHash("Attached");

    [SerializeField] private float LaunchForce;

    private bool preppingLaunch;
    private float launchTimer;
    private bool isLaunching => launchTimer > 0;

    private KillableHuman latchedHuman;
    private float latchBalance;
    [SerializeField] private float LatchForceConversion;
    [SerializeField] private float LatchMaxAngle;
    [SerializeField] private float LatchInfluenceSpeed;
    
    public override GameTeam GetTeam()
    {
        return GameTeam.Rat;
    }

    protected override float GetAcceleration()
    {
        return isLaunching ? 0 : base.GetAcceleration();
    }

    protected override float GetDeceleration()
    {
        return isLaunching ? 0 : base.GetDeceleration();
    }

    protected override float GetMaxSpeed()
    {
        return isLaunching ? float.MaxValue : base.GetMaxSpeed();
    }

    protected override void Update()
    {
        base.Update();
        
        if (launchTimer > Time.fixedDeltaTime * 3 && IsGrounded())
        {
            launchTimer = 0;
        } else if (launchTimer > 0)
        {
            launchTimer += Time.deltaTime;
            KillableHuman human = CheckForOverlappingHuman();
            if (human)
                TryLatchHuman(human);
        }

        if (latchedHuman)
        {
            transform.position = latchedHuman.transform.position;
            latchBalance -= latchedHuman.rb.linearVelocityX * LatchForceConversion * Time.deltaTime;
            latchBalance -= moveInput.x * LatchInfluenceSpeed * Time.deltaTime;
            
            transform.rotation = Quaternion.Euler(0, 0, latchBalance);

            if (Mathf.Abs(latchBalance) > LatchMaxAngle)
            {
                ReleaseLatchHuman(latchedHuman);
            }
        }
        else
        {
            latchBalance = 0;
            transform.rotation = Quaternion.identity;
        }
    }

    protected override void UpdateAnims()
    {
        base.UpdateAnims();
        animator.SetBool(AttachedID, latchedHuman);
    }

    private void PrepareLaunch()
    {
        if (!IsGrounded())
            return;
        if (latchedHuman)
            return;
        
        RBUtils.SetRBFreeze(rb, true);
        preppingLaunch = true;
    }

    private void Launch()
    {
        if (!preppingLaunch)
            return;
        preppingLaunch = false;
        launchTimer += Time.deltaTime;
        RBUtils.SetRBFreeze(rb, false);
        rb.linearVelocity = Vector2.zero;
        Vector2 launchForce = moveInput.normalized * LaunchForce;
        
        rb.AddForce(launchForce, ForceMode2D.Impulse);
    }

    private void TryLatchHuman(KillableHuman human)
    {
        if (latchedHuman)
            return;
        if (!human.TryBeginAttacking(this))
            return;

        latchedHuman = human;
        RBUtils.SetRBFreeze(rb, true);
        launchTimer = 0;
    }

    public void ReleaseLatchHuman(KillableHuman human)
    {
        RBUtils.SetRBFreeze(rb, false);
        human.DetachAttacking(this);
        latchedHuman = null;
        launchTimer = 0;
    }

    public void InputAttack(InputAction.CallbackContext ctx)
    {
        if(ctx.started)
            PrepareLaunch();
        else if(ctx.canceled)
            Launch();
    }

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.gray;
        Handles.Label(transform.position + Vector3.up, "angle - " + latchBalance);
    }
}
