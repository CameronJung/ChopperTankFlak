using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;
using static AITacticalValues;

public class DestroyUnitObjective : Objective
{

    private Unit target;

    public DestroyUnitObjective(Unit enemy, float numUnits)
    {
        base.InitializeObjective();
        target = enemy;
        //Currently there are only headquarters, if more building types are added this will need to be changed
        TacticalImportance = MAXTACTICALIMPORTANCE / numUnits + target.bounty;
    }

    public override float EvaluateSuitablitity(Unit unit)
    {
        float rating = ((float)PredictBattleResult(unit, target) + 1.0f) * 0.5f;
        rating *= GetDestructionPriority(unit, target);

        return rating;
    }


    public override float EvaluateUnitViability(Unit candidate)
    {
        return ((float)PredictBattleResult(candidate, target));
    }

    public override Vector3Int GetGoalDestination()
    {
        return target.myTilePos;
    }

    public override float EvaluateViability()
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        return "To Destroy " + target.ToString();
    }
}
