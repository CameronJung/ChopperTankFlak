using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using static UniversalConstants;

public class Strategist : MonoBehaviour
{

    private AIcommander Commander;
    private MilitaryManager militaryManager;
    private AIIntelHandler intelHandler;

    private List<Objective> Objectives;

    public EventHandler<Unit> OnUnitDeath;

    public Dictionary<Unit, ObjectiveAssignment> Assignments { get; private set; }
    private GlobalNavigationData Navigation;
    private AStarMeasurement measurer;

    public Dictionary<Vector3Int, Objective> Destinations { get; private set; } = new Dictionary<Vector3Int, Objective>();


    


    // Start is called before the first frame update
    void Start()
    {
        Commander = gameObject.GetComponent<AIcommander>();
        intelHandler = gameObject.GetComponent<AIIntelHandler>();
        militaryManager = GameObject.Find(UniversalConstants.MANAGERPATH).GetComponent<MilitaryManager>();
        Navigation = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<GlobalNavigationData>();

        Objectives = new List<Objective>();
        Assignments = new Dictionary<Unit, ObjectiveAssignment>();
        measurer = gameObject.GetComponent<AStarMeasurement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void AdaptToDeathOfUnit(Unit died)
    {
        Debug.Log("OnDeath not yet implemented");
        Debug.Log("A unit died");
    }


    /*
     * Assign Objectives
     * 
     * this method will go through the AI's units and assign them to objectives
     * 
     */
    public IEnumerator AssignObjectives()
    {

        Assignments.Clear();

        List<Unit> units = militaryManager.GetListOfReadyUnits(Faction.ComputerTeam);

        GenerateObjectives();
        yield return null;
        Dictionary<Unit, List<ObjectiveAssignment>> possibilities = new Dictionary<Unit, List<ObjectiveAssignment>>();

        /*
        foreach(Unit unit in units)
        {
            possibilities.Add(unit, new List<ObjectiveAssignment>());

            foreach(Objective goal in Objectives)
            {
                if(goal.EvaluateUnitViability(unit) > 0.0f)
                {
                    ObjectiveAssignment oa = new ObjectiveAssignment(unit, goal, measurer);
                    yield return oa.CalculateSuitability();
                    possibilities[unit].Add(oa);
                }
                
            }

            possibilities[unit].Sort();
            Assignments.Add(unit, (possibilities[unit])[0]);

        }*/


        foreach (Unit unit in units)
        {
            possibilities.Add(unit, new List<ObjectiveAssignment>());
            List<ObjectiveAssignment> oas = new List<ObjectiveAssignment>();

            foreach (Objective goal in Objectives)
            {
                //if (goal.EvaluateUnitViability(unit) > 0.0f)
                //{
                    ObjectiveAssignment oa = new ObjectiveAssignment(unit, goal, GridHelper.CalcTilesBetweenGridCoords(unit.myTilePos, goal.GetGoalDestination()));
                    oas.Add(oa);
                //}
            }

            //possibilities[unit].Sort();
            oas.Sort();
            UnitLeader leader = unit.GetComponent<UnitLeader>();
            yield return leader.BeginPlanning(oas[0].objective.GetGoalDestination());

            List<ObjectiveAssignment> leaderSuggestions = leader.GetPossibleAssignments();

            if(leaderSuggestions.Count > 0)
            {
                if (!leader.HasWaypoint())
                {
                    
                    //If the leader does not have a waypoint than it must not have found a path to the objective
                    //So we repeat the search, this time for the best objective that was found
                    yield return leader.BeginPlanning(leaderSuggestions[0].objective.GetGoalDestination());

                    Debug.Assert(leader.HasWaypoint(), "The unit leader was unable to make a waypoint for an objective it found.");

                }

                
                this.Assignments.Add(unit, leader.GetPossibleAssignments()[0]);
            }
            else
            {
                this.Assignments.Add(unit, oas[0]);
            }

            

        }

    }



    public void GenerateObjectives()
    {
        List<Unit> enemies = militaryManager.GetListOfUnits(Faction.PlayerTeam);
        List<BuildingOverlay> buildings = militaryManager.GetListOfBuildings(Faction.PlayerTeam);

        Objectives.Clear();
        Destinations.Clear();

        foreach (BuildingOverlay building in buildings)
        {
            CaptureBuildingObjective captureMission = new CaptureBuildingObjective(building);
            Objectives.Add(captureMission);
            Destinations.Add(captureMission.GetGoalDestination(), captureMission);
        }

        //It is important that destroy unit objectives are generated after capture building objectives
        //If a player unit is on a player building than they have the same destination and this will result in a
        //collision in the Destinations Dictionary
        //To account for the missing objective the unit covering the building will have its bounty increased
        foreach(Unit enemy in enemies)
        {
            DestroyUnitObjective killMission = new DestroyUnitObjective(enemy, enemies.Count);
            Objectives.Add( killMission);
            if (Destinations.ContainsKey(killMission.GetGoalDestination()))
            {
                Destinations[killMission.GetGoalDestination()] = killMission;
            }
            else
            {
                Destinations.Add(killMission.GetGoalDestination(), killMission);
            }
            
        }

        

        //Debug.Log("The tactician has found " + Objectives.Count + " Objective(s) to pursue");
    }


    public Vector3Int GetDestinationOfUnit(Unit unit)
    {
        Vector3Int destination = new Vector3Int(0,0,100);

        if (Assignments.ContainsKey(unit))
        {
            destination = Assignments[unit].objective.GetGoalDestination();
        }
        else
        {
            Debug.LogError("Attempted to retrieve objective of a unit with no assigned objective");
        }

        Debug.Assert(destination.z == 0, "A unit has been assigned to an invalid destination");

        return destination;
    }

    public bool IsHexDestroyUnitDestination(HexOverlay hex)
    {
        bool hostileHere = Destinations.ContainsKey(hex.myCoords);

        if (hostileHere)
        {
            hostileHere = Destinations[hex.myCoords] is DestroyUnitObjective;
        }

        return hostileHere;
    }

}
