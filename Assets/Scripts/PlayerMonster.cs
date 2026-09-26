using UnityEngine;

public class PlayerMonster : PlayerBase
{
    [SerializeField] protected float CrawlspaceSpeed;

    public override GameTeam GetTeam()
    {
        return GameTeam.Demon;
    }

    protected override float GetMaxSpeed()
    {
        return IsInCrawlspace() ? CrawlspaceSpeed : MaxSpeed;
    }

    protected override void Update()
    {
        base.Update();

        HumanBase human = CheckForOverlappingHuman();
        if (human)
        {
            KillHuman(human);
        }
    }
}
