
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class KillableHuman : HumanBase
{
    private static readonly int PanicID = Animator.StringToHash("Panic");
    protected List<PlayerBase> attackedBy = new();

    [SerializeField] public float RatSoloKillTime;
    [SerializeField] public float RatComboFactor;
    [SerializeField] public float SwayForce;
    
    [NonSerialized] public float humanKillTimer;
    private float killTimerSwayOffset;

    protected override void Update()
    {
        base.Update();

        if (attackedBy.Any(p => p.GetTeam() == GameTeam.Rat))
        {
            humanKillTimer += (Mathf.Pow(attackedBy.Count, RatComboFactor) / RatSoloKillTime) * Time.deltaTime;
            
            

            if (humanKillTimer >= 1)
            {
                PlayerBase firstPlayer = attackedBy[0];
                foreach (PlayerRat playerBase in attackedBy.Cast<PlayerRat>().ToArray())
                {
                    playerBase.ReleaseLatchHuman(this);
                }

                firstPlayer.KillHuman(this);
            }
            
            rb.AddForceX(Mathf.Sin((humanKillTimer + killTimerSwayOffset) * 10) * SwayForce * Time.deltaTime);
        }
    }

    protected override void UpdateAnims()
    {
        base.UpdateAnims();
        animator.SetBool(PanicID, attackedBy.Any(p => p.GetTeam() == GameTeam.Rat));
    }

    private void BeginAttacking()
    {
        EnterState(HumanState.Attacked);
        if(attackedBy[0].GetTeam() == GameTeam.Demon)
            RBUtils.SetRBFreeze(rb, true);
        killTimerSwayOffset = Random.Range(0f, Mathf.PI * 2);
    }

    public bool TryBeginAttacking(PlayerBase srcPlayer)
    {
        if (attackedBy.Count == 0)
        {
            // first attacker
            attackedBy.Add(srcPlayer);
            BeginAttacking();
            return true;
        }

        // Not first attacker
        bool isOnSameTeam = attackedBy.Any(p => p.GetTeam() == srcPlayer.GetTeam());
        if (isOnSameTeam)
        {
            attackedBy.Add(srcPlayer);
        }
        return isOnSameTeam;
    }

    public void DetachAttacking(PlayerBase srcPlayer)
    {
        attackedBy.Remove(srcPlayer);
        if (attackedBy.Count == 0)
        {
            AllDoneAttacking();
        }
    }

    public void AllDoneAttacking()
    {
        RBUtils.SetRBFreeze(rb, false);
        GoIdle();
        humanKillTimer = 0;
    }
}
