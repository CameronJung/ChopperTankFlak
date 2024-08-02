using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;

[CreateAssetMenu]
public class TerrainTile : Tile, ISelectable 
{


    [SerializeField] private bool[] traverseArray = {true, true, true, true};

    [TextArea][SerializeField] private string description;
    [SerializeField] private string title;


    /*
     * CanUnitPass
     * 
     * This method is a basic terrain check. It checks if the unit's movement type permits it to
     * be on this tile without considering any other nuances
     */
    public bool CanUnitPass(Unit unit)
    {
        
        return traverseArray[(int)unit.GetMovementType()];
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
