using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ContinueCondition : ModularCondition
{

    [SerializeField] Unit UnitTarget = null;
    [SerializeField] Vector3Int TileTarget = new Vector3Int();


    private Tilemap Map = null;

    private void Start()
    {
        base.InitializeCondition();
        Map = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<Tilemap>();
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool IsSatisfied()
    {
        return true;
    }

    public override Vector3 GetSuggestedPointerPosition()
    {

        Vector3 pos = Map.GetCellCenterLocal(TileTarget);

        if(UnitTarget != null)
        {
            pos = Map.GetCellCenterWorld(UnitTarget.myTilePos);
        }

        pos += PointerOffset;

        return pos;
    }
}
