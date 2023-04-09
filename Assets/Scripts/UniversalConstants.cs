using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UniversalConstants : object
{
    public const string MAPPATH = "GameBoard/Map";

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


    public static Dictionary<Faction, Color> TeamColours = new Dictionary<Faction, Color>
    {
        {Faction.ComputerTeam, new Color(0.9f, 0.3f, 0.3f) },
        {Faction.PlayerTeam, new Color(0.1f, 0.5f, 0.8f) }
    };


}
