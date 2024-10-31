using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public static class AITacticalValues
{
    /*
     * This class serves as a single point of truth for the constants used by the AI to rank potential movements
     */

    //Combat values
    public const int DESTROYS_VEHICLE = 7;
    public const int DESTROYS_ARTILLERY = 8;
    public const int DESTROYS_INFANTRY = 4;
    public const int STARTS_STALEMATE = 3;
    public const int RESOLVES_STALEMATE = 3;
    public const int WILL_BE_DESTROYED = -7;
    public const int INFANTRY_ENDS_STALEMATE = 2;
    public const int INFANTRY_DESTROYS_UNIT = 1;


    //Threat analysis
    public const int MORTAL_DANGER = -5;
    public const int MILD_DANGER = -1;
    public const int OPPORTUNITY = +3;
    //Artillery
    public const int ARTILLERY_THREATENS_VEHICLE = -2;
    public const int ARTILLERY_THREATENS_INFANTRY = -1;
    public const int ARTILLERY_THREATENED = -2;

    //Movement values
    public const int MARCH_ON = 0;
    public const int MOVES_MAXIMUM = 1;
    public const int AWAY_FROM_HOME = 1;
    public const int CLOSER_TO_ENEMY_BASE = 3;
    public const int INFANTRY_CLOSER_TO_ENEMY_BASE = 4;
    public const int ARTILLERY_CLOSER_TO_BASE = 1;
    public const int INFANTRY_CAPTURES_BASE = 15;
    public const int COMPLETES_CAPTURE = 5;


    //Unit Leader Navigation values
    public const int TOWARDS_WAYPOINT = 2;
    public const int AWAY_FROM_WAYPOINT = -1;
    public const int ON_WAYPOINT = 1;
    public const int FURTHER_BONUS = 3;



    //Bounties
    public const int CAN_DESTROY_VEHICLE = 6;
    public const int CAN_DESTROY_INFANTRY = 4;
    public const int CAN_CAPTURE_BASE = 15;



    //Value increase of a factions last unit of a specific type
    //Losing all of a type of unit represents a significant loss of combat potential
    public const float LAST_OF_TYPE_VALUE_BONUS = 0.75f;

    //Amount the rating of a move should be increased if it brings the unit to a safer position than it is currently on
    public const int SAFER_POSITION_BONUS = 2;

    //The value of destroying an enemy unit  or protecting an allied unit
    public static readonly Dictionary<UnitType, float> UNIT_VALUES = new Dictionary<UnitType, float>
    {
        {UnitType.InfantrySquad, 1.75f },
        {UnitType.Helicopter, 3.0f},
        {UnitType.Tank, 3.0f},
        {UnitType.Flak, 3.0f},
        {UnitType.Artillery, 3.5f},
    };


    //Risk assessment corefficients for stalemates
    public const float ONLY_ENEMY_CAN_BREAK = 0.1f;
    public const float ONLY_ALLIE_CAN_BREAK = 1.0f;
    public const float NONE_CAN_BREAK = 0.5f;
    public const float BOTH_CAN_BREAK = 0.75f;




    //These values represent how valuable it is for a unit to destroy a unit in a situation where the attack deals the kill
    public static readonly Dictionary<UnitType, Dictionary<UnitType, float>> ATTACK_WITH_PRIORITIES = new Dictionary<UnitType, Dictionary<UnitType, float>>
    {
        { UnitType.InfantrySquad, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 1.0f},
                {UnitType.Helicopter, 0.1f},
                {UnitType.Tank, 0.1f},
                {UnitType.Flak, 0.1f},
                {UnitType.Artillery, 1.25f},
            }
        },
        { UnitType.Helicopter, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 0.25f},
                {UnitType.Helicopter, 1.0f},
                {UnitType.Tank, 1.5f},
                {UnitType.Flak, 0.1f},
                {UnitType.Artillery, 0.75f},
            }
        },
        { UnitType.Tank, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 0.25f},
                {UnitType.Helicopter, 0.1f},
                {UnitType.Tank, 1.0f},
                {UnitType.Flak, 1.5f},
                {UnitType.Artillery, 0.75f},
            }
        },
        { UnitType.Flak, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 0.25f},
                { UnitType.Helicopter, 1.5f},
                { UnitType.Tank, 0.1f},
                { UnitType.Flak, 1.0f},
                { UnitType.Artillery, 0.75f},
            }
        },
        {
    UnitType.Artillery, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 0.25f},
                {UnitType.Helicopter, 1.25f},
                {UnitType.Tank, 1.25f},
                {UnitType.Flak, 1.25f},
                {UnitType.Artillery, 1.5f},
            }
        }
    };


    public static readonly Dictionary<UnitType, Dictionary<UnitType, float>> END_STALEMATE_PRIORITIES = new Dictionary<UnitType, Dictionary<UnitType, float>>
    {
        { UnitType.InfantrySquad, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 1.5f},
                {UnitType.Helicopter, 1.5f},
                {UnitType.Tank, 1.5f},
                {UnitType.Flak, 1.5f},
                {UnitType.Artillery, 1.5f},
            }
        },
        { UnitType.Helicopter, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 1.5f},
                {UnitType.Helicopter, 1.125f},
                {UnitType.Tank, 0.25f},
                {UnitType.Flak, 1.25f},
                {UnitType.Artillery, 0.75f},
            }
        },
        { UnitType.Tank, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 1.5f},
                {UnitType.Helicopter, 1.25f},
                {UnitType.Tank, 1.125f},
                {UnitType.Flak, 0.25f},
                {UnitType.Artillery, 0.75f},
            }
        },
        { UnitType.Flak, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 0.25f},
                { UnitType.Helicopter, 0.25f},
                { UnitType.Tank, 1.25f},
                { UnitType.Flak, 1.125f},
                { UnitType.Artillery, 0.75f},
            }
        },
        {
    UnitType.Artillery, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 0.75f},
                {UnitType.Helicopter, 0.75f},
                {UnitType.Tank, 0.75f},
                {UnitType.Flak, 0.75f},
                {UnitType.Artillery, 0.5f},
            }
        }
    };


    public static float GetDestructionPriority(Unit Attacker, Unit Victim)
    {

        if (Victim.IsInStalemate())
        {
            return END_STALEMATE_PRIORITIES[Attacker.GetUnitType()][Victim.GetUnitType()];
        }
        else
        {
            return ATTACK_WITH_PRIORITIES[Attacker.GetUnitType()][Victim.GetUnitType()];
        }
        
    }
}
