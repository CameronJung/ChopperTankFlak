using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class Tank : TurretedUnit
{
    


    public override UnitType GetUnitType()
    {
        return UnitType.Tank;
    }



    public override bool IsWeakTo(Unit foe)
    {
        bool weak = false;

        if (foe.myState != UnitState.stalemate)
        {
            weak = foe.GetUnitType() == UnitType.Helicopter;
        }

        return weak || this.myState == UnitState.stalemate;
    }


    public override void ResolveCombat(Unit other) 
    {
        //We will not expect a response if we are responding
        if (other.myState != UnitState.stalemate && !other.waitingForResponse)
        {
            waitingForResponse = (other.GetUnitType() == this.GetUnitType() || other.GetUnitType() == UnitType.Helicopter);
        }
        other.BeEngaged(this);
    }


    //POLYMORPHISM
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
                if (assailant.GetUnitType() == UnitType.Helicopter || assailant.GetUnitType() == UnitType.Artillery)
                {
                    //Tanks are vulnerable to Helicopters
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
