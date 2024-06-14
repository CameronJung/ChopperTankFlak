using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UniversalConstants : object
{
    public const string MAPPATH = "GameBoard/Map";
    public const string MANAGERPATH = "GameManager";
    public const string AIPATH = "EnemyCommander";
    public const string AUDIOPATH = "AudioSettings";

    //The number of turns it takes to capture a building
    public const int BUILDINGCAPFRESH = 2;


    //The square of the world distance between two adjacent hexes
    public const float HEXDISTANCESQUARED = 1.0f;
    public const float HEXWIDTHWORLDUNITS = 0.8659766f;
    public const float HEXHEIGHTWORLDUNITS = 1.0f;

    //Constants for unit types using enums makes code more readable
    public enum UnitType : int
    {
        InfantrySquad = 0,
        Helicopter = 1,
        Tank = 2,
        Flak = 3
    }


    public enum HexDirection : int
    {
        UpRight = 0,
        Up = 1,
        UpLeft = 2,
        DownLeft = 3,
        Down = 4,
        DownRight = 5,
    }

    public enum Faction: int
    {
        ComputerTeam = 0,
        PlayerTeam = 1
    }

    public enum HexState: int
    {
        unreachable = 0,
        reachable = 1,
        attackable = 2,
        hold = 3,
        capture = 4,
    }

    public enum UnitState: int
    {
        tired = 0,
        ready = 1,
        stalemate = 2,
        retaliating = 3,
    }

    


    public static Dictionary<Faction, Color> TeamColours = new Dictionary<Faction, Color>
    {
        {Faction.ComputerTeam, new Color(0.9f, 0.3f, 0.3f) },
        {Faction.PlayerTeam, new Color(0.1f, 0.5f, 0.8f) }
    };



    /*
     * 
     * This method returns the unit type that the specified "unit" is weak to
     * In the case of infantry which are vulnerable to all units the infantry type is returned
     * 
     * !NOTE! if a unit type is not found than the type of "unit" will be returned, this shouldn't happen though
     * because all unit types are accounted for
     * 
     */
    public static UnitType GetWeaknessOf(Unit unit)
    {
        UnitType unitIs = unit.GetUnitType();
        UnitType weakness = unit.GetUnitType();
        
        if(unitIs == UnitType.Helicopter) { weakness = UnitType.Flak; }
        if(unitIs == UnitType.Tank) { weakness = UnitType.Helicopter; }
        if (unitIs == UnitType.Flak) { weakness = UnitType.Flak; }

        return weakness;
    }


    /*
     * 
     * This method returns the unit type that the specified "unit" is strong against
     * In the case of infantry which are vulnerable to all units the infantry type is returned
     * 
     * !NOTE! if a unit type is not found than the type of "unit" will be returned, this shouldn't happen though
     * because all unit types are accounted for
     * 
     */
    public static UnitType GetStrengthOf(Unit unit)
    {
        UnitType unitIs = unit.GetUnitType();
        UnitType weakness = unit.GetUnitType();

        if (unitIs == UnitType.Helicopter) { weakness = UnitType.Tank; }
        if (unitIs == UnitType.Tank) { weakness = UnitType.Flak; }
        if (unitIs == UnitType.Flak) { weakness = UnitType.Helicopter; }

        return weakness;
    }


}
