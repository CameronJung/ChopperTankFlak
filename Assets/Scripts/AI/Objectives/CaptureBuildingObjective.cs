using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class CaptureBuildingObjective : Objective
{

    private BuildingOverlay Target;

    //Only infantry can capture buildings so all other units are unsuitable for this objective
    protected Dictionary<UnitType, float> UnitTypeSuitability = new Dictionary<UnitType, float>
    {
        {UnitType.InfantrySquad, 1.0f },
        {UnitType.Helicopter, 0.0f },
        {UnitType.Tank, 0.0f },
        {UnitType.Flak, 0.0f },
        {UnitType.Artillery, 0.0f },
    };


    public CaptureBuildingObjective(BuildingOverlay building)
    {
        base.InitializeObjective();
        Target = building;
        TacticalImportance = MAXTACTICALIMPORTANCE;
    }


    public override Vector3Int GetGoalDestination()
    {
        return Target.myCoords;
    }

    public override float EvaluateViability()
    {
        float viability = -1.0f;

        if(MilitaryMan.GetMilitary(Faction.ComputerTeam).CountUnitType(UnitType.InfantrySquad) > 0)
        {
            viability = 1.0f;
        }

        return viability;
    }


    public override float EvaluateSuitablitity(Unit unit)
    {
        float viability = this.EvaluateUnitViability(unit);
        
        
        
        return UnitTypeSuitability[unit.GetUnitType()];
    }

    public override float EvaluateUnitViability(Unit candidate)
    {
        return UnitTypeSuitability[candidate.GetUnitType()];
    }


    public override string ToString()
    {
        return "To capture building at " + this.Target.myCoords;
    }
}
