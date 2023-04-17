using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class Tank : Unit
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

    private void FixedUpdate()
    {
        FollowOrders();
    }

    public override UnitType GetUnitType()
    {
        return UnitType.Tank;
    }


    public override void ResolveCombat(Unit other) 
    {
        Debug.Log("Violence is never the answer.");
    }

    public override void BeEngaged(Unit assailant)
    {
        throw new System.NotImplementedException();
    }



}
