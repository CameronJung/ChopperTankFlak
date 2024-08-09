using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RangedUnit : TurretedUnit {

    //These two values dictate the number of tiles away a ranged unit can attack
    [SerializeField] protected int MinimumRange;
    [SerializeField] protected int MaximumRange;

}
