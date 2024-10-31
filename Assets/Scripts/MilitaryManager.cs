using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;



/*
 * Military Manager
 * This class tracks and maintains the buildings and units that belong to the factions are in battle
 * 
 */
public class MilitaryManager : MonoBehaviour
{

    private Dictionary<Faction, Military> militaries = new Dictionary<Faction, Military>
    { 
        {Faction.ComputerTeam, new Military(Faction.ComputerTeam) },
        {Faction.PlayerTeam, new Military(Faction.PlayerTeam) }
    };

    private Dictionary<Faction, List<BuildingOverlay>> Buildings = new Dictionary<Faction, List<BuildingOverlay>>
    {
        {Faction.ComputerTeam, new List<BuildingOverlay>() },
        {Faction.PlayerTeam, new List<BuildingOverlay>() }
    };

    private GameManager boss;
    

    // Start is called before the first frame update
    void Start()
    {
        boss = gameObject.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void AddUnit(Unit noob)
    {
        militaries[noob.GetAllegiance()].AddUnit(noob);
    }

    public void RemoveUnitFrom(Unit unit)
    {
        militaries[unit.GetAllegiance()].RemoveUnit(unit);
        boss.ReportDeath(unit);
    }

    public Military GetMilitary(Faction belongsTo)
    {
        return militaries[belongsTo];
    }

    public int CountTotalUnits(Faction faction)
    {
        return militaries[faction].CountUnits();
    }

    public int CountUnitsOfType(Faction faction, UnitType kind)
    {
        return militaries[faction].CountUnitType(kind);
    }

    public void RevitalizeMilitaryOf(Faction faction)
    {
        militaries[faction].Revitalize();
    }

    public List<Unit>GetListOfUnits(Faction faction)
    {
        return militaries[faction].GetListOfUnits();
    }

    public List<Unit>GetListOfReadyUnits(Faction faction)
    {
        return militaries[faction].GetListOfReadyUnits();
    }


    public void RegisterBuildingOwnership(BuildingOverlay building) 
    {
        Buildings[building.GetAllegiance()].Add(building);
    }

    public void ChangeBuildingOwnershipRegistration(BuildingOverlay building, Faction capturer)
    {
        Buildings[building.GetAllegiance()].Remove(building);
        Buildings[capturer].Add(building);
    }

    public List<BuildingOverlay> GetListOfBuildings(Faction faction)
    {
        return Buildings[faction];
    }
}
