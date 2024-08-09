using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class RocketCarrier : RangedUnit
{

    public override UniversalConstants.UnitType GetUnitType()
    {
        return UnitType.Artillery;
    }

    /*
     * The rocket carrier is defeated by any unit in standard combat
     */
    public override bool IsWeakTo(Unit foe)
    {
        return true;
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

        base.Die();

        

    }

}
