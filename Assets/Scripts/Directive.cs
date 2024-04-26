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
        if(directiveType == HexState.attackable)
        {
            smartness += ConsiderMatchup();
        }

        //Do not consider geography in a combat situation
        if (!capable.GetOccupiedHex().intel.IsUnitThreatened(capable))
        {
            smartness += ConsiderGeography();
        }
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
            if (capable.IsWeakTo(other))
            {
                smart += WILL_BE_DESTROYED;
            }
            else
            {
                
                if (capable.GetUnitType() == other.GetUnitType() && capable.GetUnitType() != UnitType.InfantrySquad)
                {
                    smart += STARTS_STALEMATE;
                }
                else
                {
                    if(other.GetUnitType() == UnitType.InfantrySquad)
                    {
                        smart += DESTROYS_INFANTRY;
                    }
                    else
                    {
                        smart += DESTROYS_VEHICLE;
                    }
                    
                }
                smart += other.bounty;
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
        bool distant = false;
        if(capable.GetMobility() == destination.distanceFrom)
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
            

            //The measure true distance function is expensive so only perform it when the distance travelled is significant
            if (distant)
            {
                
                if (CapableDistance > DestinationDistance)
                {
                    smart += CLOSER_TO_ENEMY_BASE;
                    if (capable.GetUnitType() == UnitType.InfantrySquad)
                    {
                        smart += INFANTRY_CLOSER_TO_ENEMY_BASE;
                    }
                }
            }
            

            if (capable.GetUnitType() == UnitType.InfantrySquad && destination.myCoords == intel.GetPlayerBaseLoc())
            {
                smart += INFANTRY_CAPTURES_BASE;
                Debug.Log("AI evaluated capture directive");
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
