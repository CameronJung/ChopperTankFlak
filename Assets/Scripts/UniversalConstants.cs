using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UniversalConstants : object
{
    public const string MAPPATH = "GameBoard/Map";
    public const string MANAGERPATH = "GameManager";
    public const string AIPATH = "EnemyCommander";
    public const string AUDIOPATH = "AudioSettings";
    public const string SELECTORPATH = "SelectionManager";

    //The number of turns it takes to capture a building
    public const int BUILDINGCAPFRESH = 2;


    //The square of the world distance between two adjacent hexes
    public const float HEXDISTANCESQUARED = 1.0f;
    public const float HEXWIDTHWORLDUNITS = 0.8659766f;
    public const float HEXHEIGHTWORLDUNITS = 1.0f;


    //This is the maximum value that an objective can have, this maximum value is assigned if
    //completeing the objective results in victory
    public const float MAXTACTICALIMPORTANCE = 100.0f;


    //Constants for unit types using enums makes code more readable
    public enum UnitType : int
    {
        InfantrySquad = 0,
        Helicopter = 1,
        Tank = 2,
        Flak = 3,
        Artillery = 4
    }

    public enum MovementType : int
    {
        Legs = 0,
        RotaryWings = 1,
        Treads = 2,
        Wheels = 3,
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

    public enum Faction : int
    {
        ComputerTeam = 0,
        PlayerTeam = 1
    }

    public enum HexState : int
    {
        unreachable = 0,
        reachable = 1,
        attackable = 2,
        hold = 3,
        capture = 4,
        range = 5,
        snipe = 6,
    }

    public enum UnitState : int
    {
        tired = 0,
        ready = 1,
        stalemate = 2,
        retaliating = 3,
    }

    public enum BattleOutcome : int
    {
        countered = -1,
        stalemate = 0,
        destroyed = 1
    }


    public enum BooleanControls : int
    {
        deselect = 0,
        end_turn = 1,
        pan_map = 2,
        AutoTurnEnd = 3,
        ContinueButton = 4,
        DialogueShown = 5,
    }

    public enum SpecifiableControls: int
    {
        select = 0,
        order_unit = 1,
    }

    public enum ControlAccess : int
    {
        forbidden = 0,
        conditional = 1,
        free = 2,
    }


    private static BattleOutcome[,] Outcomes = {
        {BattleOutcome.destroyed, BattleOutcome.countered, BattleOutcome.countered, BattleOutcome.countered, BattleOutcome.destroyed}, 
        {BattleOutcome.destroyed, BattleOutcome.stalemate, BattleOutcome.destroyed, BattleOutcome.countered, BattleOutcome.destroyed}, 
        {BattleOutcome.destroyed, BattleOutcome.countered, BattleOutcome.stalemate, BattleOutcome.destroyed, BattleOutcome.destroyed}, 
        {BattleOutcome.destroyed, BattleOutcome.destroyed, BattleOutcome.countered, BattleOutcome.stalemate, BattleOutcome.destroyed}, 
        {BattleOutcome.destroyed, BattleOutcome.destroyed, BattleOutcome.destroyed, BattleOutcome.destroyed, BattleOutcome.destroyed}
        };


    public static Dictionary<Faction, Color> TeamColours = new Dictionary<Faction, Color>
    {
        {Faction.ComputerTeam, new Color(0.9f, 0.3f, 0.3f) },
        {Faction.PlayerTeam, new Color(0.1f, 0.5f, 0.8f) }
    };


    public static readonly Dictionary<BattleOutcome, float> OUTCOME_WEIGHTS = new Dictionary<BattleOutcome, float>
    {
        {BattleOutcome.countered, 0.0f },
        { BattleOutcome.stalemate, 0.75f },
        {BattleOutcome.destroyed, 1.0f },
    };

    /*
     * 
     * This method returns the unit type that the specified "unit" is weak to
     * In the case of infantry which are vulnerable to all units the infantry type is returned
     * 
     * !NOTE! if a unit type is not found than the type of "unit" will be returned, this is technically true
     * because both infantry and rocket carriers are able to defeat their own unit type
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
     * !NOTE! if a unit type is not found than the type of "unit" will be returned, this is technically true
     * as both infantry and Rocket Carriers can defeat their own unit type
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



    /*
     * Predict Battle Result
     * 
     * this method returns the results of a hypothetical engagement provided the beligerant units
     * 
     * !Note! this method assumes that the attacker is not in stalemate
     * 
     */
    public static BattleOutcome PredictBattleResult(Unit Attacker, Unit target)
    {
        BattleOutcome result;

        if(target.myState == UnitState.stalemate)
        {
            result = BattleOutcome.destroyed;
        }
        else
        {
            result = Outcomes[(int)Attacker.GetUnitType(), (int)target.GetUnitType()];
        }


        return result;
    }




}
