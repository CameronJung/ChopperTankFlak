using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UniversalConstants;
using static AITacticalValues;
using static ThreatAnalysisVariables;

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



    /*
     * Threat ananlysis
     * 
     * 
     * 
     */
    public float ThreatAnalysis(Unit unit)
    {
        float risk = 0.0f;

        float instaKillRisk = 0.0f;
        float stalemateRisk = 0.0f;

        float instaKillCoverage = 0.0f;
        float stalemateCoverage = 0.0f;


        int numEnemies = 0;
        int numAllies = 0;

        List<Unit> affectors = this.Tile.GetAffectingUnits();
        Debug.Assert(affectors.Count >= 1, "Hex Affects aren't being updated.");
        affectors.Remove(unit);

        Dictionary<UnitType, List<Unit>> enemies = new Dictionary<UnitType, List<Unit>>();
        Dictionary<UnitType, List<Unit>> allies = new Dictionary<UnitType, List<Unit>>();

        UnitType[] unitTypes = (UnitType[])Enum.GetValues(typeof(UnitType));

        foreach(UnitType value in unitTypes)
        {
            enemies.Add(value, new List<Unit>());
            allies.Add(value, new List<Unit>());
        }


        foreach(Unit factor in affectors)
        {
            if(unit.GetAllegiance() != factor.GetAllegiance())
            {
                //Enemy unit
                enemies[factor.GetUnitType()].Add(factor);
                numEnemies++;
                BattleOutcome outcome = PredictBattleResult(factor, unit);


                //The +1 accounts for the currently hypothetical movement of unit, it also prevents a divide by zero error
                int options = factor.PossibleAttacks[outcome] +1;

                if(options >= 0)
                {
                    switch (outcome)
                    {
                        case BattleOutcome.destroyed:
                            instaKillRisk = Mathf.Min(ThreatAnalysisVariables.MaximumKillThreat, instaKillRisk + (MaximumKillThreat / (float)options));
                            break;
                        case BattleOutcome.stalemate:
                            stalemateRisk = Mathf.Min(ThreatAnalysisVariables.MaximumStalemateThreat, stalemateRisk + (MaximumStalemateThreat / (float)options));
                            break;
                    }
                }
            }
            else
            {
                //allied unit
                allies[factor.GetUnitType()].Add(factor);
                numAllies++;
            }
        }


        foreach(UnitType unitType in unitTypes)
        {
            if(enemies[unitType].Count > 0)
            {
                //An enemy of this type is present
                



                foreach (Unit enemy in enemies[unitType])
                {
                    BattleOutcome outcome = PredictBattleResult(enemy, unit);
                    UnitType foil = UniversalConstants.GetWeaknessOf(enemy);

                    switch (outcome)
                    {
                        case BattleOutcome.destroyed:
                            if(allies[foil].Count > 0)
                            {
                                instaKillCoverage = Mathf.Min(MaximumKillCoverage, instaKillCoverage + (MaximumKillCoverage * ((float)allies[foil].Count/(float)enemies[unitType].Count)));
                            }
                            
                            break;
                        case BattleOutcome.stalemate:
                            stalemateCoverage = Mathf.Min(MaximumStalemateCoverage, stalemateCoverage + (MaximumStalemateCoverage * ((float)numAllies / (float)enemies[unitType].Count)));
                            break;
                    }
                }

                

            }
        }


        if(numEnemies < 2 && stalemateRisk > 0.0f)
        {
            // a stalemate is still detrimental, but less important if the player can't resolve it
            stalemateRisk = Mathf.Min(stalemateRisk, 1.0f);
        }
        else if(numAllies == 0)
        {
            //stalemates should be considered a greater risk if there is no unit available to resolve it
            stalemateCoverage = 0.0f;
            stalemateRisk *= 1.5f;
        }

        //The risk reduction of target saturation & coverage should only reduce the rating and not increase it
        risk = Mathf.Min(instaKillCoverage + stalemateCoverage -instaKillRisk - stalemateRisk, 0.0f);
        //risk = -instaKillRisk - stalemateRisk;
        Debug.Assert(this.Tile.GetAffectingUnits().Contains(unit), "Conducted threat analysis on hex that couldn't be reached");
        

        return risk;
    }


}
