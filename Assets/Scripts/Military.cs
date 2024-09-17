using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class Military 
{
    private List<Unit> units;

    readonly Faction faction;

    public Military(Faction allegience)
    {
        units = new List<Unit>();
        faction = allegience;
    }


    public void AddUnit(Unit noob)
    {
        if(noob.GetAllegiance() == faction)
        {
            units.Add(noob);
        }
    }



    /*
     * Take Unit
     * 
     * removes the unit from the list and returns it for handling
     * 
     */
    public Unit TakeUnit(Unit unit)
    {
        Unit stick = null;

        if (units.Contains(unit))
        {
            int index = units.IndexOf(unit);
            stick = units[index];
            units.RemoveAt(index);
        }

        return stick;
    }



    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
    }


    public int CountUnitType(UnitType species)
    {
        int count = 0;

        foreach(Unit unit in units)
        {
            if(unit.GetUnitType() == species)
            {
                count++;
            }
        }

        return count;
    }


    public int CountUnits()
    {
        return units.Count;
    }



    public List<Unit> GetUnitsOfType(UnitType species)
    {
        List<Unit> specific = new List<Unit>();

        foreach(Unit unit in units)
        {
            if(unit.GetUnitType() == species)
            {
                specific.Add(unit);
            }
        }

        return (specific);
    }

    public void Revitalize()
    {

        foreach(Unit unit in units)
        {
            unit.Revitalize();
        }
    }


    public List<Unit> GetListOfUnits()
    {
        return units;
    }

}
