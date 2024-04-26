using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;


//This interface is used to enforce compatability with the A* algorithm
public interface AStarable
{

    public abstract float GetF();

    public abstract float CalcGFor(Vector3Int origin, UnitType type);

    public abstract float CalcHFor(Vector3Int goal, UnitType type);


    public abstract void ResetAstarData();

}
