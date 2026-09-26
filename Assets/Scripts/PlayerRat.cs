
public class PlayerRat : PlayerBase
{
    public override GameTeam GetTeam()
    {
        return GameTeam.Rat;
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
