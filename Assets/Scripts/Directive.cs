using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;
using static AITacticalValues;
using static GridHelper;

//The directive class is a representation of a move that a unit could make
public class Directive
{


    //This is a measure of how smart of a move this directive is to make
    //negative values represent dumb moves
    private int smartness;

    //The unit this directive originated from, ie the unit that is capable of executing the order
    private Unit capable;

    //This the bottom tile in the order stack, the tile you click on to execute a command
    private HexOverlay destination;
    private HexOverlay Safest = null;

    public  HexState  directiveType { get; private set; }

    private AIIntelHandler intel;

    private int CapableDistance = -1;
    private int DestinationDistance = -1;

    

    //Constructors
    public Directive(Unit unit, HexOverlay hex, AIIntelHandler knowledge)
    {
        capable = unit;
        destination = hex;
        directiveType = hex.currState;
        intel = knowledge;
        ThinkThrough();
    }

    public Directive(Unit unit, HexOverlay hex)
    {
        capable = unit;
        destination = hex;
        directiveType = hex.currState;


        ThinkThrough();
    }


    /*
     * THINKTHROUGH
     * 
     * This method sets/Updates the smartness value of this directive by making various considerations
     * */
    private void ThinkThrough() 
    {
        smartness = 0;
        if(directiveType == HexState.attackable || directiveType == HexState.snipe)
        {
            smartness += ConsiderMatchup();
            if(directiveType == HexState.attackable)
            {
                Safest = destination.FindSafestNeighbourFor(capable);
            }
            
        }

        smartness += ConsiderGeography();
        smartness += ConsiderThreats();
        
    }

    /*              CONSIDERATIONS              */

    /*
     * MATCHUP
     * 
     * This method will return a modification for smartness depending on the expected result of combat
     * 
     */
    private int ConsiderMatchup()
    {
        float smart = 0;
        Unit other = this.destination.GetOccupiedBy();
        



        if (other != null)
        {
            BattleOutcome expectation = UniversalConstants.PredictBattleResult(this.capable, other);


            

            switch (expectation){
                case BattleOutcome.countered:

                    smart += WILL_BE_DESTROYED;
                    break;
                case BattleOutcome.stalemate:
                    /*
                    smart += STARTS_STALEMATE + other.bounty;
                    
                    if (destination.FindSafestNeighbourFor(capable).intel.IsStalemateRisky(other))
                    {
                        smart += MILD_DANGER;
                    }
                    */

                    if(Safest == null)
                    {
                        Safest = destination.FindSafestNeighbourFor(capable);
                    }

                    List<Unit> enemies = Safest.GetAffectingUnitsFromFaction(other.GetAllegiance());
                    enemies.Remove(other);

                    List<Unit> allies = destination.GetAffectingUnitsFromFaction(capable.GetAllegiance());
                    allies.Remove(capable);

                    //Shortcoming this does not account for an allied or enemy unit relying on the same hex as capable

                    bool allieBreak = allies.Count > 0;
                    bool enemyBreak = enemies.Count > 0;


                    if(allieBreak && enemyBreak)
                    {
                        smart += Mathf.RoundToInt(BOTH_CAN_BREAK * (float)(STARTS_STALEMATE + other.bounty));
                    }
                    else if(allieBreak || enemyBreak)
                    {
                        if (allieBreak)
                        {
                            smart += Mathf.RoundToInt(ONLY_ALLIE_CAN_BREAK * (float)(STARTS_STALEMATE + other.bounty));
                        }
                        else
                        {
                            smart += Mathf.RoundToInt(ONLY_ENEMY_CAN_BREAK * (float)(STARTS_STALEMATE + other.bounty));
                            smart += MORTAL_DANGER;
                        }
                    }
                    else
                    {
                        smart += Mathf.RoundToInt(BOTH_CAN_BREAK * (float)(STARTS_STALEMATE + other.bounty));
                        smart += MILD_DANGER;
                    }
                    



                    break;
                case BattleOutcome.destroyed:

                    if(other.GetUnitType() == UnitType.InfantrySquad)
                    {
                        smart += DESTROYS_INFANTRY;
                    }
                    else if(other.GetUnitType() == UnitType.Artillery)
                    {
                        smart += DESTROYS_ARTILLERY;
                    }
                    else
                    {
                        smart += DESTROYS_VEHICLE;
                    }

                    if(capable.GetUnitType() == UnitType.InfantrySquad)
                    {
                        //Prioritize destroying unirs with infantry, as this is an efficient use of firepower
                        smart += INFANTRY_DESTROYS_UNIT;
                    }

                    smart += other.bounty;

                    break;

            }
            
            if(other.myState == UniversalConstants.UnitState.stalemate)
            {
                //Avoid leaving stalemates unresolved
                smart += RESOLVES_STALEMATE;

                if(capable.GetUnitType() == UniversalConstants.UnitType.InfantrySquad)
                {
                    smart += INFANTRY_ENDS_STALEMATE;
                }
            }

            smart *= GetDestructionPriority(capable, other);
        }
        
        return Mathf.RoundToInt(smart);
    }



    /*
     * GEOGRAPHY
     * 
     * returns a modifier for smartness based on the movement of the unit
     */
    private int ConsiderGeography()
    {
        int smart = 0;
        //bool distant = false;


        /*
        if (capable.GetMobility() <= destination.distanceFrom)
        {
            smart += MOVES_MAXIMUM;
            distant = true;
        }


        int distDest = GridHelper.CalcTilesBetweenGridCoords(capable.prevTilePos, destination.myCoords);

        
        int distCurr = GridHelper.CalcTilesBetweenGridCoords(capable.prevTilePos, capable.myTilePos);

        
        if (distCurr < distDest)
        {
            smart += MARCH_ON;
        }
        
        if (intel != null)
        {
            

            if (distant)
            {
                
                if (CapableDistance > DestinationDistance)
                {
                    
                    if (capable.GetUnitType() == UnitType.InfantrySquad)
                    {
                        smart += INFANTRY_CLOSER_TO_ENEMY_BASE;
                    }
                    else if(capable.GetUnitType() == UnitType.Artillery)
                    {
                        smart += ARTILLERY_CLOSER_TO_BASE;
                    }
                    else
                    {
                        smart += CLOSER_TO_ENEMY_BASE;
                    }
                }
            }
            

            if (capable.GetUnitType() == UnitType.InfantrySquad && destination.myCoords == intel.GetPlayerBaseLoc())
            {
                smart += INFANTRY_CAPTURES_BASE;
            }
        }
        */
        if(capable.GetAllegiance() == Faction.ComputerTeam)
        {
            UnitLeader leader = capable.GetComponent<UnitLeader>();

            if (leader.HasWaypoint())
            {
                HexOverlay waypoint = leader.WayPoint;

                int dGoal = GridHelper.CalcTilesBetweenGridCoords(destination.myCoords, waypoint.myCoords);
                int dUnit = CalcTilesBetweenGridCoords(capable.myTilePos, destination.myCoords);

                //This calculation is true for a wedge shape opening toward the waypoint
                if (dGoal <= capable.GetMobility() && (dUnit + dGoal <= capable.GetMobility() + 1))
                {
                    smart += TOWARDS_WAYPOINT;

                    smart += Mathf.RoundToInt((float)(dUnit) / (float)(capable.GetMobility()) * FURTHER_BONUS);
                    
                    //if(destination.myCoords == waypoint.myCoords) { smart += ON_WAYPOINT; }
                }
                
                if(dUnit < dGoal)
                {
                    smart += AWAY_FROM_WAYPOINT;
                }


            }


        }



        if(intel != null)
        {
            if (capable.GetUnitType() == UnitType.InfantrySquad && destination.myCoords == intel.GetPlayerBaseLoc())
            {
                smart += INFANTRY_CAPTURES_BASE;

            }
        }
        

        if (capable.IsCapturing() && capable.myTilePos == destination.myCoords)
        {
            smart += COMPLETES_CAPTURE;
        }

        return smart;
    }


    /*
     * Threats
     * 
     * Return a modifier based on what player units will be able to attack next turn
     * 
     */
    private int ConsiderThreats()
    {
        

        float destSafety;

        if (Safest != null)
        {
            destSafety = Safest.intel.ThreatAnalysis(capable);
        }
        else
        {
            destSafety = destination.intel.ThreatAnalysis(capable);
        }

        int smart = Mathf.RoundToInt(destSafety);

        if(destSafety > capable.CurrentSafety)
        {
            smart += SAFER_POSITION_BONUS;
        }

        return smart;
    }


    override public string ToString()
    {
        string info = "Move for " + capable.ToString();
        info += " unit moves to " + destination.myCoords;
        info += " Move has a smartness of: " + smartness;
        info += " Unit threatened? " + capable.GetOccupiedHex().intel.IsUnitThreatened(capable);

        return info;
    }



    /*          ACCESSORS           */

    public int GetSmartness()
    {
        return smartness;
    }

    public Vector3Int GetDestinationCoords()
    {

        return this.destination.myCoords;
    }

    public Unit GetUnit()
    {
        return this.capable;
    }

    public Unit GetOccupant()
    {
        return destination.GetOccupiedBy();
    }



    public void SetDistances(int currDist, int destDist)
    {
        this.CapableDistance = currDist;
        this.DestinationDistance = destDist;

        this.ThinkThrough();
    }


    public void ShowSmartnessOnBoard()
    {
        string description = "G:" + this.ConsiderGeography() + "\nR:" + this.ConsiderThreats() + " U:" + destination.GetAffectingUnits().Count + "\nT:" + this.smartness;
        this.destination.DisplayMessageOnBoard(description);
    }

}
