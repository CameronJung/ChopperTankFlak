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

    public Dictionary<Unit, Objective> Assignments { get; private set; }
    private GlobalNavigationData Navigation;
    private AStarMeasurement measurer;


    


    // Start is called before the first frame update
    void Start()
    {
        Commander = gameObject.GetComponent<AIcommander>();
        intelHandler = gameObject.GetComponent<AIIntelHandler>();
        militaryManager = GameObject.Find(UniversalConstants.MANAGERPATH).GetComponent<MilitaryManager>();
        Navigation = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<GlobalNavigationData>();

        Objectives = new List<Objective>();
        Assignments = new Dictionary<Unit, Objective>();
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

        List<Unit> units = militaryManager.GetListOfUnits(Faction.ComputerTeam);

        GenerateObjectives();

        Dictionary<Unit, List<ObjectiveAssignment>> possibilities = new Dictionary<Unit, List<ObjectiveAssignment>>();


        foreach(Unit unit in units)
        {
            possibilities.Add(unit, new List<ObjectiveAssignment>());

            foreach(Objective goal in Objectives)
            {
                ObjectiveAssignment oa = new ObjectiveAssignment(unit, goal, measurer);
                yield return oa.CalculateSuitability();
                possibilities[unit].Add(oa);
            }

            possibilities[unit].Sort();
            Assignments.Add(unit, (possibilities[unit])[0].objective);
        }

        //PseudoCode
        //Get list of AI units (units)

        //if the list of units isn't empty
        //  go through the list of units
        //      assign each unit to its most suited objective
    }



    public void GenerateObjectives()
    {
        List<Unit> enemies = militaryManager.GetListOfUnits(Faction.PlayerTeam);
        List<BuildingOverlay> buildings = militaryManager.GetListOfBuildings(Faction.PlayerTeam);

        Objectives.Clear();

        foreach(Unit enemy in enemies)
        {
            DestroyUnitObjective killMission = new DestroyUnitObjective(enemy, enemies.Count);
            Objectives.Add( killMission);
        }

        foreach (BuildingOverlay building in buildings)
        {
            CaptureBuildingObjective captureMission = new CaptureBuildingObjective(building);
            Objectives.Add(captureMission);
        }

        //Debug.Log("The tactician has found " + Objectives.Count + " Objective(s) to pursue");
    }


    public Vector3Int GetDestinationOfUnit(Unit unit)
    {
        Vector3Int destination = new Vector3Int(0,0,100);

        if (Assignments.ContainsKey(unit))
        {
            destination = Assignments[unit].GetGoalDestination();
        }
        else
        {
            Debug.LogError("Attempted to retrieve objective of a unit with no assigned objective");
        }

        Debug.Assert(destination.z == 0, "A unit has been assigned to an invalid destination");

        return destination;
    }

}
