using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public abstract class Order
{

    public Vector3 destination { get; protected set; }
    public Vector3 origin { get; protected set; }
    protected Unit recipient;

    public Order(Vector3 orig, Vector3 dest, Unit unit) 
    {
        origin = orig;
        destination = dest;
        recipient = unit;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //This method will control the unit provided and make it perform the action
    public abstract IEnumerator Execute();
}
