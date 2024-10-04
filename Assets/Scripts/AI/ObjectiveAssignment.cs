using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UniversalConstants;


public class ObjectiveAssignment : IComparable<ObjectiveAssignment>
{

    private const float MAXIMUMTURNS = 5.0f;

    public Unit assignee { get; private set; }
    public Objective objective { get; private set; }

    public float suitability { get; private set; }

    private AStarMeasurement ruler;
    

    public ObjectiveAssignment(Unit u, Objective o, AStarMeasurement m)
    {
        this.assignee = u;
        this.objective = o;
        this.suitability = 0;
        this.ruler = m;
    }

    public IEnumerator CalculateSuitability()
    {
        float fitness = 0.0f;
        yield return ruler.BeginMeasurement(assignee, objective.GetGoalDestination());
        float distRating = EvaluateDistance(ruler.GetMeasuredDistance());
        fitness = distRating * this.objective.EvaluateSuitablitity(assignee) * this.objective.TacticalImportance;


        suitability = fitness;
    }




    public int CompareTo( ObjectiveAssignment other)
    {
        int comparison = -1;

        if(other.suitability <= this.suitability)
        {
            if(other.suitability < this.suitability)
            {
                comparison = 1;
            }
            else
            {
                comparison = 0;
            }
        }

        return comparison;
    }


    /*
     * Evaluate Distance
     * 
     * This method evaluates how far a unit is to the goal
     * it will return float value inversely proportional to the number of turns it will take the unit to reach the destination
     * 
     */
    protected float EvaluateDistance(int distance )
    {
        float rating = 0.0f;

        float turns = Mathf.Min((distance / assignee.GetMobility()), MAXIMUMTURNS);

        rating = Mathf.Abs(turns - MAXIMUMTURNS);

        return rating;
    }
}
