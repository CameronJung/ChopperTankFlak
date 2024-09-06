using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class InfantrySquad : Unit
{

    private BuildingOverlay capturing = null;
    
    


    public override UnitType GetUnitType()
    {
        return UnitType.InfantrySquad;
    }


    public override bool IsWeakTo(Unit foe)
    {
        bool weak = false;

        if(foe.myState != UnitState.stalemate)
        {
            weak = foe.GetUnitType() != UnitType.InfantrySquad;
        }

        return weak;
    }

    //POLYMORPHISM
    public override void ResolveCombat(Unit other)
    {
        waitingForResponse = UniversalConstants.PredictBattleResult(this, other) != BattleOutcome.destroyed;


        other.BeEngaged(this);
    }


    //POLYMORPHISM
    public override void BeEngaged(Unit assailant)
    {
        

        //Soldiers always lose when engaged
        this.Die();

        if (this.waitingForResponse)
        {
            waitingForResponse = false;
            
        }
    }


    public override IEnumerator ExecuteHoldOrder()
    {
        //Check if we are capturing a building
        BuildingOverlay building = map.GetInstantiatedObject(map.WorldToCell(transform.position)).GetComponent(typeof(HexOverlay)) as BuildingOverlay;
        if (building != null)
        {
            building.Capture(this);
            capturing = building;
        }

        yield return null;
        yield return base.ExecuteHoldOrder();
    }



    public override IEnumerator ExecuteMoveOrder(Vector3 origin, Vector3 destination)
    {
        HandleCaptureInteruption();
        return base.ExecuteMoveOrder(origin, destination);
    }



    //This method is called when something happens to interupt a capture
    private void HandleCaptureInteruption()
    {
        if(capturing != null)
        {
            capturing.CaptureInterrupted();
            capturing = null;
        }
    }


    protected override void Die()
    {
        HandleCaptureInteruption();
        base.Die();
    }



    public override bool IsCapturing()
    {
        return capturing != null;
    }


    protected override void DetermineMoveOptions()
    {
        base.DetermineMoveOptions();

        foreach(HexAffect affect in Affectors.Values)
        {
            affect.MutateToCapture();
        }
    }


}
