using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public abstract class Objective
{

    
    
    protected List<Unit> Assignees;

    public Objective SuperObjective { get; protected set; }
    public List<Objective> SubObjectives {get; protected set; }

    public float TacticalImportance { get; protected set; }

    protected MilitaryManager MilitaryMan;
    protected AIIntelHandler IntelHandler;
    protected Dictionary<UnitType, float> UnitTypeSuitablity;



    protected void InitializeObjective()
    {
        MilitaryMan = GameObject.Find(UniversalConstants.MANAGERPATH).GetComponent<MilitaryManager>();
        IntelHandler = GameObject.Find(UniversalConstants.AIPATH).GetComponent<AIIntelHandler>();
    }


    public abstract Vector3Int GetGoalDestination();


    /*
     * Evaluate Viability
     * 
     * This method returns a float value representing how possible/difficult an objective is to complete
     * A value less than or equal to 0 means that the objective is impossible, such as capturing a building without infantry
     */
    public abstract float EvaluateViability();


    /*
     * Evaluate Suitability
     * 
     * Returns a float value representing how well suited a unit is to execute/pursue this objective
     * Higher values represent a better match
     */
    public abstract float EvaluateSuitablitity(Unit unit);



    /*
     * Evaluate Unit Suitability
     * 
     * This method returns a float value representing if the "candidate" unit is capable of completing this objective
     * 
     */
    public abstract float EvaluateUnitViability(Unit candidate);


    override public abstract string ToString();
    
}
