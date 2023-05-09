using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class InfantrySquad : Unit
{

    
    private void Awake()
    {
        Enlist();
        PutOnBoard();
    }

    // Start is called before the first frame update
    void Start()
    {
        PaintUnit();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FollowOrders();
    }

    public override UnitType GetUnitType()
    {
        return UnitType.InfantrySquad;
    }



    public override void ResolveCombat(Unit other)
    {
        if (other.myState != UnitState.stalemate)
        {
            waitingForResponse = (other.GetUnitType() != this.GetUnitType());
        }


        other.BeEngaged(this);
    }



    public override void BeEngaged(Unit assailant)
    {
        

        //Soldiers always lose when engaged
        this.Die();

        if (this.waitingForResponse)
        {
            waitingForResponse = false;
            
        }
    }



}
