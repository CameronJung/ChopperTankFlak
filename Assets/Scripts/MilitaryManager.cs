using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;


public class MilitaryManager : MonoBehaviour
{

    private Dictionary<Faction, Military> militaries = new Dictionary<Faction, Military>
    { 
        {Faction.ComputerTeam, new Military(Faction.ComputerTeam) },
        {Faction.PlayerTeam, new Military(Faction.PlayerTeam) }
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

    public void RevitalizeMilitaryOf(Faction faction)
    {
        militaries[faction].Revitalize();
    }

    public List<Unit>GetListOfUnits(Faction faction)
    {
        return militaries[faction].GetListOfUnits();
    }
}
