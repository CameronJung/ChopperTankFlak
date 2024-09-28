using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class DestroyUnitObjective : Objective
{

    private Unit target;

    public DestroyUnitObjective(Unit enemy)
    {
        base.InitializeObjective();
        target = enemy;
        //Currently there are only headquarters, if more building types are added this will need to be changed
        TacticalImportance = MAXTACTICALIMPORTANCE;
    }

    public override float EvaluateSuitablitity(Unit unit)
    {
        throw new System.NotImplementedException();
    }

    public override Vector3Int GetGoalDestination()
    {
        return target.myTilePos;
    }

    public override float EvaluateViability()
    {
        throw new System.NotImplementedException();
    }
}
