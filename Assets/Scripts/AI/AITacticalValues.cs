using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AITacticalValues
{
    /*
     * This class serves as a single point of truth for the integer constants used by the AI to rank potential movements
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
    public const int MILD_DANGER = -2;
    public const int OPPORTUNITY = +2;
    //Artillery
    public const int ARTILLERY_THREATENS_VEHICLE = -3;
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

    //Bounties
    public const int CAN_DESTROY_VEHICLE = 6;
    public const int CAN_DESTROY_INFANTRY = 4;
    public const int CAN_CAPTURE_BASE = 15;
}
