using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OrderUnitCondition : ModularCondition
{
    [SerializeField] private Vector3Int TileDestination = new Vector3Int();
    [SerializeField] private Unit SelectedUnit;
    [SerializeField] private Unit UnitDestination = null;



    // Start is called before the first frame update
    void Start()
    {
        base.InitializeCondition();
    }

    public override bool IsSatisfied()
    {
        bool satisfied = Selector.selectedUnit == SelectedUnit;

        if (SelectedUnit.HasMission())
        {
            satisfied = satisfied && (GetDestination() == SelectedUnit.GetMissionDestination());
        }

        return satisfied;
    }

    public override Vector3 GetSuggestedPointerPosition()
    {
        Tilemap map = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<Tilemap>();
        Vector3 pos = map.CellToWorld(TileDestination);

        if (UnitDestination != null)
        {
            pos = UnitDestination.transform.position;
        }

        pos += PointerOffset;

        return pos;
    }


    private Vector3Int GetDestination()
    {
        Vector3Int pos = TileDestination;

        if (UnitDestination != null)
        {
            pos = UnitDestination.myTilePos;
        }

        return pos;
    }
}
