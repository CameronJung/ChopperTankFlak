using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;
using static AITacticalValues;

public class HexIntel
{

    public readonly HexOverlay Tile;

    //A value of true means that the unit type is capable of attacking this hex
    private Dictionary<UnitType, bool> Threats = new Dictionary<UnitType, bool>
    {
        { UnitType.InfantrySquad, false },
        {UnitType.Helicopter, false },
        {UnitType.Tank, false },
        {UnitType.Flak, false},
        {UnitType.Artillery, false }

    };

    private AIIntelHandler informant;

    public HexIntel(HexOverlay hex)
    {
        informant = GameObject.Find(AIPATH).GetComponent<AIIntelHandler>();
        this.Tile = hex;
    }

    public void AffectedBy(Unit unit)
    {
        Threats[unit.GetUnitType()] = true;
        
        informant.ReportAffectedHex(this);
    }

    public void WipeIntel()
    {
        for(UnitType key = UnitType.InfantrySquad; key <= UnitType.Flak; key++)
        {
            Threats[key] = false;
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


    //This method is used only for debugging, it returns a string that represents the current threats the tile knows about
    public string GetDebugString()
    {
        string debug = "";

        for (UnitType key = UnitType.InfantrySquad; key <= UnitType.Artillery; key++)
        {
            
            if(Threats[key])
            {
                debug += key + "\n";
            }
        }

        return debug;
    }


}
