using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;

[CreateAssetMenu]
public class TerrainTile : Tile, ISelectable 
{

    [SerializeField] private Dictionary<UnitType, bool> traversablleBy = new Dictionary<UnitType, bool>{
        {UnitType.InfantrySquad, true },
        {UnitType.Helicopter, true },
        {UnitType.Tank, true },
        {UnitType.Flak, true },
    };

    [SerializeField] private bool[] traverseArray = {true, true, true, true};

    [TextArea][SerializeField] private string description;
    [SerializeField] private string title;

    public bool CanUnitPass(Unit unit)
    {
        
        return traverseArray[(int)unit.GetUnitType()];
    }

    public string GetDescription()
    {
        return this.description;
    }

    public string GetTitle()
    {
        return this.title;
    }

}
