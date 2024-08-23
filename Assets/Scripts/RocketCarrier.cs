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
        //Rocket carriers do not expect a response because they always destroy their target
        //Further Rocket Carriers are always destroyed when attacked
        other.BeEngaged(this);
    }


    //POLYMORPHISM
    public override void BeEngaged(Unit assailant)
    {

        base.Die();

        

    }

}
