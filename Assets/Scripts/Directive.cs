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
        }

        //Do not consider geography in a combat situation
        //if (!capable.GetOccupiedHex().intel.IsUnitThreatened(capable))
        //{
            smartness += ConsiderGeography();
        //}
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
        int smart = 0;
        Unit other = this.destination.GetOccupiedBy();
        



        if (other != null)
        {
            BattleOutcome expectation = UniversalConstants.PredictBattleResult(this.capable, other);


            

            switch (expectation){
                case BattleOutcome.countered:

                    smart += WILL_BE_DESTROYED;
                    break;
                case BattleOutcome.stalemate:

                    smart += STARTS_STALEMATE + other.bounty;

                    if (destination.FindValidNeighborFor(capable).intel.IsStalemateRisky(other))
                    {
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
            
        }

        return smart;
    }



    /*
     * GEOGRAPHY
     * 
     * returns a modifier for smartness based on the movement of the unit
     */
    private int ConsiderGeography()
    {
        int smart = 0;
        bool distant = false;

        if (capable.GetMobility() == destination.distanceFrom)
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
        int smart = destination.intel.ThreatTo(capable);


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

    public int getSmartness()
    {
        return smartness;
    }

    public Vector3Int getDestinationCoords()
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

}
