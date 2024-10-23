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
    public const int TOWARDS_WAYPOINT = 3;
    public const int AWAY_FROM_WAYPOINT = -1;
    public const int ON_WAYPOINT = 1;
    public const int CLOSENESS_BONUS = 3;



    //Bounties
    public const int CAN_DESTROY_VEHICLE = 6;
    public const int CAN_DESTROY_INFANTRY = 4;
    public const int CAN_CAPTURE_BASE = 15;

    //These values represent how valuable it is for a unit to destroy a unit in a situation where the attack deals the kill
    public static readonly Dictionary<UnitType, Dictionary<UnitType, float>> DESTROY_WITH_PRIORITIES = new Dictionary<UnitType, Dictionary<UnitType, float>>
    {
        { UnitType.InfantrySquad, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 1.0f},
                {UnitType.Helicopter, 1.5f},
                {UnitType.Tank, 1.5f},
                {UnitType.Flak, 1.5f},
                {UnitType.Artillery, 1.25f},
            }
        },
        { UnitType.Helicopter, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 0.25f},
                {UnitType.Helicopter, 1.0f},
                {UnitType.Tank, 1.0f},
                {UnitType.Flak, 1.25f},
                {UnitType.Artillery, 0.75f},
            }
        },
        { UnitType.Tank, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 0.25f},
                {UnitType.Helicopter, 1.25f},
                {UnitType.Tank, 1.0f},
                {UnitType.Flak, 1.0f},
                {UnitType.Artillery, 0.75f},
            }
        },
        { UnitType.Flak, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 0.25f},
                { UnitType.Helicopter, 1.0f},
                { UnitType.Tank, 1.25f},
                { UnitType.Flak, 1.0f},
                { UnitType.Artillery, 0.75f},
            }
        },
        {
    UnitType.Artillery, new Dictionary<UnitType, float>
            {
                {UnitType.InfantrySquad, 0.25f},
                {UnitType.Helicopter, 1.25f},
                {UnitType.Tank, 1.0f},
                {UnitType.Flak, 1.0f},
                {UnitType.Artillery, 0.5f},
            }
        }
    };

    public static float GetDestructionPriority(Unit Attacker, Unit Victim)
    {
        return DESTROY_WITH_PRIORITIES[Attacker.GetUnitType()][ Victim.GetUnitType()];
    }
}
