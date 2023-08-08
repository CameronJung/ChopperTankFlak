using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

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
     * Modifications:
     * -No combat: 0
     * -will be destroyed: -3
     * -enter stalemate: +1
     * -will destroy enemy +2
     * -will resolve stalemate: +1
     * -resolve stalemate with infantry: +1
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
                smart = -3;
            }
            else if (capable.GetUnitType() == other.GetUnitType())
            {
                smart = 1;
            }
            else
            {
                smart = 2;
            }

            if(other.myState == UniversalConstants.UnitState.stalemate)
            {
                smart++;

                if(capable.GetUnitType() == UniversalConstants.UnitType.InfantrySquad)
                {
                    smart++;
                }
            }
        }

        return smart;
    }



    /*
     * GEOGRAPHY
     * 
     * returns a modifier for smartness based on the movement of the unit
     * 
     * Further from computer base: +1
     * -Travels as far as possible: +1
     * -moves away from previous location: +1
     * -closer to player base: +1
     * -Infantry moves closer to player base +1
     * -Infantry moves onto player's base: +10
     */
    private int ConsiderGeography()
    {
        int smart = 0;

        if(capable.GetMobility() == destination.distanceFrom)
        {
            smart += 1;
        }

        int distPrev = (destination.myCoords - capable.prevTilePos).sqrMagnitude;
        int distCurr = (destination.myCoords - capable.myTilePos).sqrMagnitude;

        if (distCurr >= distPrev)
        {
            smart += 1;
        }

        //compare distance to computer's base
        distPrev = (capable.myTilePos - intel.GetComputerBaseLoc()).sqrMagnitude;
        distCurr = (destination.myCoords - intel.GetComputerBaseLoc()).sqrMagnitude;
        if (distCurr >= distPrev)
        {
            smart += 1;
        }

        //Compare distance to player's base
        distPrev = (capable.myTilePos - intel.GetPlayerBaseLoc()).sqrMagnitude;
        distCurr = (destination.myCoords - intel.GetPlayerBaseLoc()).sqrMagnitude;
        if (distCurr <= distPrev)
        {
            smart += 1;
            if (capable.GetUnitType() == UnitType.InfantrySquad)
            {
                smart += 1;
            }
        }

        if (capable.GetUnitType() == UnitType.InfantrySquad && destination.myCoords == intel.GetPlayerBaseLoc())
        {
            smart += 10;
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
}
