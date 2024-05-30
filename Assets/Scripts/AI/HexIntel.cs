using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;
using static AITacticalValues;

public class HexIntel
{

    //A value of true means that the unit type is capable of attacking this hex
    private Dictionary<UnitType, bool> Threats = new Dictionary<UnitType, bool>
    {
        { UnitType.InfantrySquad, false },
        {UnitType.Helicopter, false },
        {UnitType.Tank, false },
        {UnitType.Flak, false}
    };

    private AIIntelHandler informant;

    public HexIntel()
    {
        informant = GameObject.Find(AIPATH).GetComponent<AIIntelHandler>();
    }

    public void AffectedBy(Unit unit)
    {
        Threats[unit.GetUnitType()] = true;
    }

    public void WipeIntel()
    {
        foreach(KeyValuePair<UnitType, bool> kv in Threats)
        {
            Threats[kv.Key] = false;
        }
    }


    public int ThreatTo(Unit unit)
    {
        int danger = 0;

        if(unit.GetUnitType() == UnitType.InfantrySquad)
        {
            if (Threats.ContainsValue(true))
            {
                danger += MORTAL_DANGER;
            }
        }
        else
        {
            //unit is a vehicle

            if (Threats[unit.GetUnitType()])
            {
                danger += MILD_DANGER;
            }

            if(unit.GetUnitType() == UnitType.Helicopter && Threats[UnitType.Flak] ||
               unit.GetUnitType() == UnitType.Tank && Threats[UnitType.Helicopter] ||
               unit.GetUnitType() == UnitType.Flak && Threats[UnitType.Tank]) 
            {
                danger += MORTAL_DANGER;
            }
            else if(Threats[UniversalConstants.GetStrengthOf(unit)] && !Threats[unit.GetUnitType()])
            {
                //If there is a threat from a unit we are strong against than hunt it down
                danger += OPPORTUNITY;
            }
        }

        return danger;
    }



    /*
     * This method returns true if the enemy has the potential to destroy the Unit "unit" should it be on this space next turn
     * Currently this does not account for a unit be destroyed by its weakness during stalemate
     */
    public bool IsUnitThreatened(Unit unit)
    {
        bool danger = false;


        if(unit.GetUnitType() == UnitType.InfantrySquad)
        {
            danger = Threats.ContainsValue(true);
        }
        else
        {
            if (unit.GetUnitType() == UnitType.Helicopter && Threats[UnitType.Flak] ||
               unit.GetUnitType() == UnitType.Tank && Threats[UnitType.Helicopter] ||
               unit.GetUnitType() == UnitType.Flak && Threats[UnitType.Tank])
            {
                danger = true;
            }
            else if (Threats[unit.GetUnitType()])
            {
                danger = Threats[UnitType.InfantrySquad];
            }
        }


        return danger;
    }


}
