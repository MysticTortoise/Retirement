using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMonster : PlayerBase
{
    [SerializeField] protected float CrawlspaceSpeed;
    [SerializeField] protected float ConsumeTime;

    [SerializeField] private GameObject TendrilTrapPrefab;
    [SerializeField] private float MaxTendrilNearCeilingDepth;
    [SerializeField] private float MaxTendrilPenetration;
    [SerializeField] private LayerMask TendrilCastMask;
    [SerializeField] private float TendrilCooldown;

    private float humanConsumeTimer;
    private HumanBase currentConsumingHuman;

    private BoxCollider2D gloobBox;
    private SpriteRenderer gloobSprite;
    private BoxCollider2D bigBox;
    private SpriteRenderer bigSprite;

    [SerializeField] private GameObject TendrilVisualizerPrefab;
    private GameObject tendrilVisualizer;
    private List<TentacleTrap> traps = new(2);
    private float trapCooldown;

    public override GameTeam GetTeam()
    {
        return GameTeam.Demon;
    }

    protected override float GetMaxSpeed()
    {
        return IsInCrawlspace() ? CrawlspaceSpeed : MaxSpeed;
    }

    protected override void Start()
    {
        base.Start();
        gloobBox = GetComponents<BoxCollider2D>().First(b => b.enabled);
        bigBox = GetComponents<BoxCollider2D>().First(b => !b.enabled);
        gloobSprite = GetComponent<SpriteRenderer>();
        bigSprite = transform.Find("BigSprite").GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        tendrilVisualizer = Instantiate(TendrilVisualizerPrefab);
        var trapComp = tendrilVisualizer.GetComponent<TentacleTrap>();
        trapComp.owner = this;
        trapComp.visualOnly = true;

        UpdateFormStatus();
    }

    protected override void Update()
    {
        UpdateFormStatus();

        base.Update();

        CheckForRats();

        if (IsConsumingHuman())
            ConsumeHumanTick();

        var tendrilTransform = GetTendrilTransform();
        if (tendrilTransform != null && !IsTrapOnCooldown())
        {
            tendrilVisualizer.transform.position = (Vector3)(tendrilTransform?.pos);
            tendrilVisualizer.transform.rotation = (Quaternion)tendrilTransform?.rot;
        }
        else
        {
            tendrilVisualizer.transform.position = Vector3.up * 99999f;
        }

        if (IsTrapOnCooldown())
        {
            trapCooldown -= Time.deltaTime;
        }
        
        
    }

    private void CheckForRats()
    {
        ContactFilter2D filter = new()
        {
            layerMask = 1 << gameObject.layer,
            useLayerMask = true,
            useTriggers = false
        };
        var cols = new Collider2D[4];
        Physics2D.OverlapBox(box.bounds.center, box.bounds.size + (Vector3.one * 0.01f), 0, filter, cols);

        foreach (Collider2D col in cols)
        {
            if (!col)
                continue;
            var rat = col.GetComponent<PlayerRat>();
            if(!rat)
                continue;
            
            rat.RatKill();
        }
    }

    private void UpdateFormStatus()
    {
        if (IsInCrawlspace())
        {
            box = bigBox;
            bigBox.enabled = true;
            bigSprite.enabled = true;
            gloobBox.enabled = false;
            gloobSprite.enabled = false;
        }
        else
        {
            box = gloobBox;
            gloobBox.enabled = true;
            gloobSprite.enabled = true;
            bigBox.enabled = false;
            bigSprite.enabled = false;
        }
    }

    protected bool IsConsumingHuman()
    {
        return humanConsumeTimer > 0;
    }

    public bool IsTrapOnCooldown()
    {
        return trapCooldown > 0;
    }

    private (Vector2 pos, Quaternion rot)? GetTendrilTransform()
    {
        if (!IsGrounded() || moveInput.y > 0.6f)
        {
            // up tendril
            RaycastHit2D ceilingResult =
                Physics2D.Raycast(transform.position, Vector3.up, MaxTendrilNearCeilingDepth, TendrilCastMask);
            if (!ceilingResult.collider)
            {
                return null;
            }

            float ceilDist = ceilingResult.distance;

            RaycastHit2D penetrateTest =
                Physics2D.Raycast(
                    transform.position + Vector3.up * (ceilDist + MaxTendrilPenetration + 0.1f),
                    Vector3.down, MaxTendrilPenetration * 2, TendrilCastMask);

            if (penetrateTest.point.y <= ceilingResult.point.y + 0.01f)
            {
                return null;
            }

            Vector2 pos = penetrateTest.point;
            Quaternion quat;
            if (pos.y < transform.position.y)
            {
                quat = Quaternion.Euler(0, 0, 180);
            }
            else
            {
                quat = Quaternion.identity;
            }

            return (pos, quat);
        }

        // Down Tendril
        RaycastHit2D floorResult =
            Physics2D.Raycast(transform.position, Vector3.down, MaxTendrilNearCeilingDepth, TendrilCastMask);
        if (!floorResult.collider)
        {
            return null;
        }

        float floorDist = floorResult.distance;

        RaycastHit2D penetrateTestD =
            Physics2D.Raycast(
                transform.position + Vector3.down * (floorDist + MaxTendrilPenetration + 0.01f),
                Vector3.up, MaxTendrilPenetration * 2, TendrilCastMask);

        if (penetrateTestD.point.y >= floorResult.point.y - 0.01f)
        {
            return null;
        }

        Vector2 posD = penetrateTestD.point;
        Quaternion quatD;
        if (posD.y < transform.position.y)
        {
            quatD = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            quatD = Quaternion.identity;
        }

        return (posD, quatD);
    }

    private void TrySpawnTendrilTrap()
    {
        if (IsTrapOnCooldown())
            return;
        
        traps.RemoveAll(t => !t);
        while (traps.Count >= 2)
        {
            TentacleTrap trap = traps[0];
            trap.Disappear();
            traps.RemoveAt(0);
        }
        
        var tendrilTransform = GetTendrilTransform();
        if (tendrilTransform == null)
            return;

        GameObject tendrilObj = Instantiate(TendrilTrapPrefab);
        tendrilObj.transform.position = (Vector3)tendrilTransform?.pos;
        tendrilObj.transform.rotation = (Quaternion)tendrilTransform?.rot;

        var trapComp = tendrilObj.GetComponent<TentacleTrap>();
        trapComp.owner = this;
        traps.Add(trapComp);
        trapCooldown = TendrilCooldown;
    }

    public void InputTryLatch(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;
        if (IsInCrawlspace())
        {
            TrySpawnTendrilTrap();
        }
        else
        {
            KillableHuman human = CheckForOverlappingHuman();
            if (human)
                LatchOntoHumanBlob(human);
        }
    }

    protected void LatchOntoHumanBlob(KillableHuman human)
    {
        if (currentConsumingHuman)
            return;
        if (!human.TryBeginAttacking(this))
            return;
        transform.position = human.transform.position;
        RBUtils.SetRBFreeze(rb, true);
        humanConsumeTimer = float.Epsilon;
        currentConsumingHuman = human;
    }

    private void ConsumeHumanTick()
    {
        humanConsumeTimer += Time.deltaTime;
        if (humanConsumeTimer >= ConsumeTime)
        {
            FinishConsumingHuman();
        }
    }

    private void FinishConsumingHuman()
    {
        humanConsumeTimer = 0;
        KillHuman(currentConsumingHuman);
        RBUtils.SetRBFreeze(rb, false);
        currentConsumingHuman = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.up * MaxTendrilNearCeilingDepth);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + (Vector3.up * MaxTendrilNearCeilingDepth),
            Vector3.up * MaxTendrilPenetration);
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * MaxTendrilNearCeilingDepth);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + (Vector3.down * MaxTendrilNearCeilingDepth),
            Vector3.down * MaxTendrilPenetration);
    }
}