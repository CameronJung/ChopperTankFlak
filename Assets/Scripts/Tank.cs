using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class Tank : Unit
{
    // Start is called before the first frame update
    void Start()
    {
        PaintUnit();
        PutOnBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
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
