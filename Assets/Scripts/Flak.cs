using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class Flak : Unit
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
        return UnitType.Flak;
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
