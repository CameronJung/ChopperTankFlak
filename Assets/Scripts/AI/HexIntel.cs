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
        if (unit.GetAllegiance() == Faction.PlayerTeam)
        {
            Threats[unit.GetUnitType()] = true;
        }
        
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

        bool trinityUnit = (unit.GetUnitType() == UnitType.Helicopter ||
                    unit.GetUnitType() == UnitType.Tank ||
                    unit.GetUnitType() == UnitType.Flak);

        bool artilleryThreat = Threats[UnitType.Artillery];

        bool trinityThreat = (Threats[UnitType.Helicopter] || Threats[UnitType.Tank] || Threats[UnitType.Flak]);

        

        bool infantryThreat = Threats[UnitType.InfantrySquad];
        bool anyThreat = Threats.ContainsValue(true);


        bool artilleryOnly = artilleryThreat && !(infantryThreat || trinityThreat);

        //don't bother if their is no threat
        if (anyThreat)
        {
            if (trinityUnit)
            {
                //Trinity vehicles only need to worry about certain units
                artilleryOnly = artilleryThreat && !(Threats[UniversalConstants.GetWeaknessOf(unit)] || Threats[unit.GetUnitType()]);

                if (artilleryOnly)
                {
                    danger = ARTILLERY_THREATENS_VEHICLE;
                }
                else
                {
                    if (Threats[unit.GetUnitType()])
                    {
                        danger += MILD_DANGER;
                    }

                    if (artilleryThreat ||
                        unit.GetUnitType() == UnitType.Helicopter && Threats[UnitType.Flak] ||
                        unit.GetUnitType() == UnitType.Tank && Threats[UnitType.Helicopter] ||
                        unit.GetUnitType() == UnitType.Flak && Threats[UnitType.Tank])
                    {
                        danger += MORTAL_DANGER;
                    }
                    else if (infantryThreat || Threats[UniversalConstants.GetStrengthOf(unit)] && !Threats[unit.GetUnitType()])
                    {
                        //If there is a threat from a unit we are strong against than hunt it down
                        danger += OPPORTUNITY;
                    }

                    
                }
            }
            else
            {
            

                if(unit.GetUnitType() == UnitType.InfantrySquad)
                {
                    if (artilleryOnly)
                    {
                        danger = ARTILLERY_THREATENS_INFANTRY;
                    }
                    else
                    {
                        danger = MORTAL_DANGER;
                    }
                }
                else
                {
                    
                    //The AI should be especially careful with its artillery
                    danger = MORTAL_DANGER;
                    
                }
            }
        }

        


        /*
        if(unit.GetUnitType() == UnitType.InfantrySquad || unit.GetUnitType() == UnitType.Artillery)
        {
            if (Threats.ContainsValue(true))
            {
                danger += MORTAL_DANGER;
            }
        }
        else
        {
            //unit is a titular vehicle

            if (Threats[unit.GetUnitType()])
            {
                danger += MILD_DANGER;
            }

            if(Threats[UnitType.Artillery] ||
                unit.GetUnitType() == UnitType.Helicopter && Threats[UnitType.Flak] ||
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
        */
        return danger;
    }



    /*
     * This method returns true if the enemy has the potential to destroy the Unit "unit" should it be on this space next turn
     * Currently this does not account for a unit be destroyed by its weakness during stalemate
     */
    public bool IsUnitThreatened(Unit unit)
    {
        bool danger = false;


        if(unit.GetUnitType() == UnitType.InfantrySquad || unit.GetUnitType() == UnitType.Artillery)
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


    /*
     * Is Stalemate Risky
     * 
     * This method is intended to check if there is a threat involved with going into stalemate with the unit in "stalemated"
     * 
     * !note! this method is to be called on the hex intel that the hypothetical unit that starts the stalemate will be on
     * 
     * !FLAW! this method will not detect a threat from the same unit type as stalemated
     * 
     */
    public bool IsStalemateRisky(Unit stalemated)
    {
        bool risky = false;

        foreach( KeyValuePair<UnitType, bool> pair in Threats)
        {
            risky = risky || (pair.Value && (stalemated.GetUnitType() != pair.Key));
        }

        return risky;
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
