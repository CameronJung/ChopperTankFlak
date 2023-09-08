using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AITacticalValues
{
    /*
     * This class serves as a single point of truth for the integer constants used by the AI to rank potential movements
     */

    //Combat values
    public const int DESTROYS_ENEMY = 3;
    public const int STARTS_STALEMATE = 1;
    public const int RESOLVES_STALEMATE = 2;
    public const int WILL_BE_DESTROYED = -3;
    public const int INFANTRY_ENDS_STALEMATE = 1;

    //Movement values
    public const int MARCH_ON = 1;
    public const int MOVES_MAXIMUM = 1;
    public const int AWAY_FROM_HOME = 1;
    public const int CLOSER_TO_ENEMY_BASE = 1;
    public const int INFANTRY_CLOSER_TO_ENEMY_BASE = 1;
    public const int INFANTRY_CAPTURES_BASE = 10;
}
