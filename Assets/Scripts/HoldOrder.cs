using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldOrder : Order
{
    
    public HoldOrder(Vector3 orig, Vector3 dest, Unit unit) : base(orig, dest, unit)
    {
        this.origin = orig;
        this.destination = dest;
        this.recipient = unit;
    }



    public override IEnumerator Execute()
    {
        return recipient.ExecuteHoldOrder();
    }

    public override string ToString()
    {
        return "Hold Order";
    }
}
