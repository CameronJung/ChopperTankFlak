using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class Flak : Unit
{
    private void Awake()
    {
        Enlist();
        PutOnBoard();
    }


    // Start is called before the first frame update
    void Start()
    {
        PaintUnit();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FollowOrders();
    }

    public override UnitType GetUnitType()
    {
        return UnitType.Flak;
    }


    public override void ResolveCombat(Unit other)
    {
        //We will not expect a response if we are responding
        if (other.myState != UnitState.stalemate && !other.waitingForResponse)
        {
            waitingForResponse = (other.GetUnitType() == this.GetUnitType() || other.GetUnitType() == UnitType.Tank);
        }
        other.BeEngaged(this);
    }


    public override void BeEngaged(Unit assailant)
    {
        

        if (assailant != this.stalematedWith)
        {
            if (myState == UnitState.stalemate)
            {
                //Any unit locked in stalemate is vulnerable
                this.stalematedWith.StalemateResolved();
                this.Die();
            }
            else
            {
                if (assailant.GetUnitType() == UnitType.Tank)
                {
                    //Flak Cars are vulnerable to Tanks
                    //manager.ReportActionComplete(this);
                    this.Die();
                }
                else
                {
                    if (assailant.GetUnitType() == this.GetUnitType())
                    {
                        //Alike units go into stalemate
                        EnterStalemate(assailant);
                        assailant.EnterStalemate(this);
                    }
                    //The unit is unscathed from the attack and will now retaliate
                    this.Retaliate(assailant);
                }
            }
        }

        if (this.waitingForResponse)
        {
            waitingForResponse = false;
        }
    }
}
