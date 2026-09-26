using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMonster : PlayerBase
{
    [SerializeField] protected float CrawlspaceSpeed;
    [SerializeField] protected float ConsumeTime;

    private float humanConsumeTimer;
    private HumanBase currentConsumingHuman;

    private BoxCollider2D gloobBox;
    private SpriteRenderer gloobSprite;
    private BoxCollider2D bigBox;
    private SpriteRenderer bigSprite;

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
        
        UpdateFormStatus();
    }

    protected override void Update()
    {
        UpdateFormStatus();
        
        base.Update();

        if (IsConsumingHuman())
            ConsumeHumanTick();
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

    public void InputTryLatch(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;
        KillableHuman human = CheckForOverlappingHuman();
        if (human)
            LatchOntoHumanBlob(human);
    }

    protected void LatchOntoHumanBlob(KillableHuman human)
    {
        if (currentConsumingHuman)
            return;
        if (!human.TryBeginAttacking(this))
            return;
        transform.position = human.transform.position;
        RBUtils.SetRBFreeze(rb,true);
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
        RBUtils.SetRBFreeze(rb,false);
        currentConsumingHuman = null;
    }
}
