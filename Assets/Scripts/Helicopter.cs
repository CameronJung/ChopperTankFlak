using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : Unit
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

    public override UniversalConstants.UnitType GetUnitType()
    {
        return UniversalConstants.UnitType.Helicopter;
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
