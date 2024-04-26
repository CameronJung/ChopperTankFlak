using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOrder : Order
{

    public MoveOrder(Vector3 orig, Vector3 dest, Unit unit) : base(orig, dest, unit)
    {
        this.origin = orig;
        this.destination = dest;
        this.recipient = unit;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override IEnumerator Execute()
    {
        return recipient.ExecuteMoveOrder(origin, destination);
    }

    public override string ToString()
    {
        return "Move Order";
    }
}
