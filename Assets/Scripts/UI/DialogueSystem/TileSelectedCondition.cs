using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileSelectedCondition : ModularCondition
{

    [SerializeField] Vector3Int TargetedTile;


    // Start is called before the first frame update
    void Start()
    {
        base.InitializeCondition();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool IsSatisfied()
    {
        return (Selector.selectedUnit == null && Selector.selectedPos == TargetedTile);
    }

    public override Vector3 GetSuggestedPointerPosition()
    {
        Tilemap map = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<Tilemap>();

        Vector3 pos = map.GetCellCenterLocal(TargetedTile);
        pos += PointerOffset;

        return pos;
    }
}
