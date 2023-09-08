using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;
using static AITacticalValues;

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

    private HexState directiveType;

    private AIIntelHandler intel;


    //Constructor
    public Directive(Unit unit, HexOverlay hex, AIIntelHandler knowledge)
    {
        capable = unit;
        destination = hex;
        directiveType = hex.currState;
        intel = knowledge;
        ThinkThrough();
    }


    /*
     * THINKTHROUGH
     * 
     * This method sets the smartness value of this directive by making various considerations
     * */
    private void ThinkThrough() 
    {
        smartness = 0;
        if(directiveType == HexState.attackable)
        {
            smartness += ConsiderMatchup();
        }
        smartness += ConsiderGeography();
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
            if (capable.IsWeakTo(other))
            {
                smart += WILL_BE_DESTROYED;
            }
            else if (capable.GetUnitType() == other.GetUnitType() && capable.GetUnitType() != UnitType.InfantrySquad)
            {
                smart += STARTS_STALEMATE;
            }
            else
            {
                smart += DESTROYS_ENEMY;
            }

            if(other.myState == UniversalConstants.UnitState.stalemate)
            {
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

        if(capable.GetMobility() == destination.distanceFrom)
        {
            smart += MOVES_MAXIMUM;
        }

        int distPrev = (destination.myCoords - capable.prevTilePos).sqrMagnitude;
        int distCurr = (destination.myCoords - capable.myTilePos).sqrMagnitude;

        if (distCurr >= distPrev)
        {
            smart += MARCH_ON;
        }

        //compare distance to computer's base
        distPrev = (capable.myTilePos - intel.GetComputerBaseLoc()).sqrMagnitude;
        distCurr = (destination.myCoords - intel.GetComputerBaseLoc()).sqrMagnitude;
        if (distCurr >= distPrev)
        {
            smart += AWAY_FROM_HOME;
        }

        //Compare distance to player's base
        distPrev = (capable.myTilePos - intel.GetPlayerBaseLoc()).sqrMagnitude;
        distCurr = (destination.myCoords - intel.GetPlayerBaseLoc()).sqrMagnitude;
        if (distCurr <= distPrev)
        {
            smart += CLOSER_TO_ENEMY_BASE;
            if (capable.GetUnitType() == UnitType.InfantrySquad)
            {
                smart += INFANTRY_CLOSER_TO_ENEMY_BASE;
            }
        }

        if (capable.GetUnitType() == UnitType.InfantrySquad && destination.myCoords == intel.GetPlayerBaseLoc())
        {
            smart += INFANTRY_CAPTURES_BASE;
        }

        return smart;
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
}
