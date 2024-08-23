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

    //This property is used by ranged units to note hexes where their weapon can reach in
    //addition to some other action like movement
    public bool WithinRange { get; private set; }

    public Unit Owner { get; private set; }

    public HexState RecordedState { get; private set; }

    


    public HexAffect(Unit belongsTo, HexOverlay affects, HexState state, int steps, bool inRange = false)
    {
        this.Hex = affects;
        Hex.NotifyAffectOf(belongsTo);
        this.DistanceFrom = steps;
        this.RangeFrom = GridHelper.CalcTilesBetweenGridCoords(belongsTo.myTilePos, affects.myCoords);
        this.Owner = belongsTo;
        this.RecordedState = state;
        this.WithinRange = inRange;
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


    /*
     * Mutate Within Range
     * 
     * Should the conditions be appropriate this method will change the HexAffect to mark the HexOverlay as being within Range
     * 
     * Conditions:
     *  The owner is a ranged unit
     *  The value of range from is between the parameters min and max (inclusive)
     */
    public void MutateWithinRange(int min, int max)
    {
        if(Owner is RangedUnit)
        {
            this.WithinRange = this.RangeFrom >= min && this.RangeFrom <= max;
        }
    }

}
