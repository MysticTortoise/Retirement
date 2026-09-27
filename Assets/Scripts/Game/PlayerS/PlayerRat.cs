
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    
    [SerializeField] private Material[] RatMats;

    [SerializeField] private float RespawnTime;
    private float deadTimer;

    [SerializeField] private float JumpChargeTime;
    private float jumpCharge;

    [SerializeField] private int LaunchBubbles;
    [SerializeField] private Material spriteMaterial;
    [SerializeField] private Sprite BallSprite;
    [SerializeField] private Sprite ArrowSprite;
    
    private RenderParams matRenderParms;
    private SpriteParams ballSpriteParms;
    private SpriteParams arrowSpriteParms;
    
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

    public bool IsDead()
    {
        return deadTimer > 0;
    }

    protected override void Start()
    {
        base.Start();
        
        matRenderParms = new RenderParams(spriteMaterial);
        ballSpriteParms = new SpriteParams(BallSprite);
        arrowSpriteParms = new SpriteParams(ArrowSprite);
    }

    public override void SetPlayerID(int id)
    {
        base.SetPlayerID(id);
        GetComponent<SpriteRenderer>().material = RatMats[id - 1];
    }

    protected override void Update()
    {
        base.Update();

        if (preppingLaunch)
        {
            jumpCharge = Mathf.Clamp(jumpCharge + Time.deltaTime / JumpChargeTime, 0, 1);
            DrawLaunchVis();
        }

        if (IsDead())
        {
            deadTimer -= Time.deltaTime;
            if (deadTimer <= 0)
            {
                RatRespawn();
            }
        }
        
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
            if (CanInput())
            {
                latchBalance -= moveInput.x * LatchInfluenceSpeed * Time.deltaTime;
            }
            
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

    private Vector3 GetBubblePos(float fac)
    {
        float maxDist = jumpCharge * 4;
        Vector3 targetPos = moveInput * (maxDist * fac);
        targetPos += transform.position;
        return targetPos;
    }

    private float GetBubbleRot()
    {
        return Mathf.Atan2(moveInput.y, moveInput.x);
    }

    private Matrix4x4 GetBubbleMatrix(float fac, bool hasRot)
    {
        Quaternion rot = hasRot ? Quaternion.Euler(0,0,GetBubbleRot() * Mathf.Rad2Deg) : Quaternion.identity;
        return Matrix4x4.TRS(GetBubblePos(fac), rot, Vector3.one);
    }
    
    private void DrawLaunchVis()
    {
        Matrix4x4[] ballMatrices = new Matrix4x4[LaunchBubbles];
        for (int i = 0; i < ballMatrices.Length; i++)
        {
            ballMatrices[i] = GetBubbleMatrix((float)i / ballMatrices.Length, false);
        }
        Graphics.RenderSpriteInstanced(matRenderParms, ballSpriteParms, 0, ballMatrices);
        Graphics.RenderSprite(matRenderParms, arrowSpriteParms, 0, GetBubbleMatrix(1, true));
    }

    public void RatKill()
    {
        if (IsDead())
            return;
        CancelLaunch();
        deadTimer = RespawnTime;
    }

    protected override bool CanInput()
    {
        return base.CanInput() && !IsDead();
    }

    private void RatRespawn()
    {
        transform.position = spawnPoint;
        RoundManager.instance.AddToScore(GetTeam(), -1);
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
        jumpCharge = 0;
    }

    private void CancelLaunch()
    {
        preppingLaunch = false;
        RBUtils.SetRBFreeze(rb, false);
    }

    private void Launch()
    {
        if (!preppingLaunch)
            return;
        preppingLaunch = false;
        launchTimer += Time.deltaTime;
        RBUtils.SetRBFreeze(rb, false);
        rb.linearVelocity = Vector2.zero;
        Vector2 launchForce = moveInput.normalized * LaunchForce * jumpCharge;
        
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
        if (!CanInput())
            return;
        if(ctx.started)
            PrepareLaunch();
        else if(ctx.canceled)
            Launch();
    }
    
#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.gray;
        Handles.Label(transform.position + Vector3.up, "angle - " + latchBalance);
    }
    #endif
}
