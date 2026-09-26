using UnityEngine;

public class PlayerMonster : PlayerBase
{
    [SerializeField] protected float CrawlspaceSpeed;

    protected override float GetMaxSpeed()
    {
        return IsInCrawlspace() ? CrawlspaceSpeed : MaxSpeed;
    }

    /*protected override void Update()
    {
        base.Update();
    }*/
}
