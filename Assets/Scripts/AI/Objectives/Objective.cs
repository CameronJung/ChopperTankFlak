using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public abstract class Objective
{

    private const float MAXIMUMTURNS = 5.0f;
    
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
     * Evaluate Distance
     * 
     * This method evaluates how far a unit is to the goal
     * it will return float value inversely proportional to the number of turns it will take the unit to reach the destination
     * 
     */
    protected float EvaluateDistance(Unit unit, int distance)
    {
        float rating = 0.0f;

        float turns = Mathf.Max( (distance / unit.GetMobility()), MAXIMUMTURNS);

        rating = Mathf.Abs(turns - MAXIMUMTURNS);

        return rating;
    }
}
