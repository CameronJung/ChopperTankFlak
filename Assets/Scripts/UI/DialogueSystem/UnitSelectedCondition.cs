using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitSelectedCondition : ModularCondition
{

    [SerializeField] private Unit TargetedUnit;
    

    private Tilemap Map = null;

    private void Start()
    {
        base.InitializeCondition();
        Map = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<Tilemap>();
    }


    public override bool IsSatisfied()
    {
        bool satisfied = false;

        if(TargetedUnit == null)
        {
            satisfied = Selector.selectedUnit == null;
        }
        else
        {
            satisfied = Selector.selectedUnit == TargetedUnit;
        }


        return satisfied;
    }

    public override Vector3 GetSuggestedPointerPosition()
    {
        return Map.GetCellCenterWorld(TargetedUnit.myTilePos) + PointerOffset;
    }
}
