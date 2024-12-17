using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequiredMove : DialogueStep
{

    [SerializeField] private Unit Selected = null;

    [SerializeField] private Unit Targeted = null;
    [SerializeField] private Vector3Int Destination = new Vector3Int();

    private void Start()
    {
        
    }


    public Vector3Int GetDestination()
    {
        Vector3Int dest = Destination;

        if(Targeted != null)
        {
            dest = Targeted.myTilePos;
        }

        return dest;
    }


    public Unit GetActor()
    {
        return Selected;
    }
}
