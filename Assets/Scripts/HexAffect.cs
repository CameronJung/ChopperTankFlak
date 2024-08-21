using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;
using static GridHelper;

public class HexAffect
{

    public HexOverlay Hex { get; private set; }

    public int DistanceFrom { get; private set; }

    public int RangeFrom { get; private set; }

    public Unit Owner { get; private set; }

    public HexState RecordedState { get; private set; }


    public HexAffect(Unit belongsTo, HexOverlay affects, HexState state, int steps)
    {
        this.Hex = affects;
        Hex.NotifyAffectOf(belongsTo);
        this.DistanceFrom = steps;
        this.RangeFrom = GridHelper.CalcTilesBetweenGridCoords(belongsTo.myTilePos, affects.myCoords);
        this.Owner = belongsTo;
        this.RecordedState = state;
    }

    public void Restore(bool conspicuous = true)
    {
        Hex.RestoreEffect(this, conspicuous);
    }

    public void ChangeDistance(int contestant)
    {
        this.DistanceFrom = Mathf.Min(this.DistanceFrom, contestant);
    }
    


    /*
     * Mutate To Capture
     * 
     * this method is called by infantry, it checks if it is possible for the owner to capture a building on the affected Hex
     * 
     * The conditions are:
     *  - Hex must be a building overlay
     *  - Owner must be infantry
     *  - The building and owner must be of different factions
     *  - This affect must not be an attack type
     * 
     */
    public void MutateToCapture()
    {
        if(Hex is BuildingOverlay)
        {
            BuildingOverlay building = (BuildingOverlay)Hex;
            if (building.CanCapture(Owner))
            {
                if(RecordedState == HexState.reachable)
                {
                    RecordedState = HexState.capture;
                }
            }
        }
    }

}
