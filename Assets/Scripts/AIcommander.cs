using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;

public class AIcommander : MonoBehaviour
{
    private const int MAXCAUTION = 10;
    private const int MINCAUTION = 4;
    private const int CHECKS_PER_FRAME = 4;

    [SerializeField] private SelectionManager selector;
    [SerializeField] private CommandTracer commander;
    [SerializeField] private GameManager manager;

    private Unit[] military;
    private MilitaryManager militaryManager;
    private List<HexOverlay> possibilities;
    private List<Directive> bestMoves;
    private AIIntelHandler intel;
    private Tilemap Map;
    private Strategist Tactician;
    

    //The higher this value is the less likely the AI is to follow through with unfavorable
    //engagements
    private int caution;
    

    // Start is called before the first frame update
    void Start()
    {
        Tactician = gameObject.GetComponent<Strategist>();
        caution = Random.Range(MINCAUTION, MAXCAUTION +1);
        intel = gameObject.GetComponent<AIIntelHandler>();
        Map = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<Tilemap>();
        militaryManager = GameObject.Find(UniversalConstants.MANAGERPATH).GetComponent<MilitaryManager>();
        selector.RememberStrategist(Tactician);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TakeTurn()
    {
        //this.military = units.ToArray();
        this.military = militaryManager.GetListOfUnits(Faction.ComputerTeam).ToArray();
        //StartCoroutine(IssueDirectives());
        StartCoroutine(ObjectiveBasedTurn(null));
    }

    public void TakeTurn( List<RequiredMove> stageLines)
    {
        if(stageLines.Count > 0)
        {
            StartCoroutine(ObjectiveBasedTurn(stageLines));
        }
        else
        {
            StartCoroutine(ObjectiveBasedTurn(null));
        }
    }



    private IEnumerator ObjectiveBasedTurn(List<RequiredMove> stageLines)
    {
        yield return null;

        //First process moves that are predetermined
        if(stageLines != null)
        {
            foreach(RequiredMove line in stageLines)
            {

                yield return null;

                selector.HandleAISelection(line.GetActor().myTilePos);

                commander.SpoofSendCommand(line.GetDestination());
                selector.HandleDeselect();
                yield return null;

                //unmoved.Remove(choice.GetUnit());

                int cycles = 0;

                //wait until the unit is moving
                while (!manager.IsUnitMoving() && cycles < 600)
                {
                    //Rounding off time scale means that frames during a pause won't be counted
                    cycles += Mathf.RoundToInt(Time.timeScale);
                    Debug.Assert(cycles < 600, "!ERROR! commander caught in endless wait loop");
                    yield return null;
                }

                cycles = 0;

                //Wait until the unit is done moving
                while (manager.IsUnitMoving() && cycles < 600)
                {
                    yield return null;
                    cycles += Mathf.RoundToInt(Time.timeScale);
                }
                Debug.Assert(cycles < 600, "!ERROR! commander caught in endless wait loop");

                selector.HandleDeselect();
            }
        }



        float turnStarted = Time.realtimeSinceStartup;

        bestMoves = new List<Directive>();

        List<Unit> unmoved = militaryManager.GetListOfReadyUnits(Faction.ComputerTeam);

        List<Unit> myUnits = militaryManager.GetListOfUnits(Faction.ComputerTeam);

        List<Unit> enemies = militaryManager.GetListOfUnits(Faction.PlayerTeam);



        while(unmoved.Count > 0 && !manager.IsBattleOver())
        {
            yield return null;
            selector.HandleDeselect();
            intel.WipeOldData();

            int itter = 0;

            //update everything I can do
            foreach (Unit ally in myUnits)
            {
                selector.HandleAISelection(ally.myTilePos);

                selector.HandleDeselect();

                ally.SetBounty();

                itter++;
                if (itter % CHECKS_PER_FRAME == 0)
                {
                    yield return null;
                }
            }
            itter = 0;

            //Look at what the player might do
            foreach (Unit enemy in enemies)
            {
                selector.HandleAISelection(enemy.myTilePos);

                //yield return null;

                selector.PerformTacticalAnalysis();
                //yield return null;

                enemy.SetBounty();

                selector.HandleDeselect();
                itter++;
                if (itter % CHECKS_PER_FRAME == 0)
                {
                    yield return null;
                }

            }

            itter = 0;
            foreach (Unit ally in unmoved)
            {
                selector.HandleAISelection(ally.myTilePos);

                selector.HandleDeselect();

                ally.SetCurrentSafety();

                itter++;
                if (itter % CHECKS_PER_FRAME == 0)
                {
                    yield return null;
                }
            }
            


            yield return Tactician.AssignObjectives();
            

            

            itter = 0;
            //Now that all the options are known we choose a unit to move
            foreach (Unit ally in unmoved)
            {
                selector.HandleAISelection(ally.myTilePos);



                bestMoves = MaintainBest(this.bestMoves, selector.GetSmartestMoves(intel, ally));
                
                selector.HandleDeselect();

                itter++;
                if (itter % CHECKS_PER_FRAME == 0)
                {
                    yield return null;
                }
            }


            Unit chosenUnit = this.RandomDirective(bestMoves).GetUnit();
            /*
            float best = -1.0f;
            
            foreach(KeyValuePair<Unit, ObjectiveAssignment> kv in Tactician.Assignments)
            {
                if(kv.Value.suitability > best)
                {
                    best = kv.Value.suitability;
                    chosenUnit = kv.Key;
                }
            }*/


            yield return null;

            selector.HandleAISelection(chosenUnit.myTilePos);

            yield return selector.GetSmartestMoves(intel);

            List<Directive> choices = selector.GetDirectives();

            Directive picked = this.RandomDirective(choices);


            string moveDetails = "Chosen move was: " + picked.ToString() + " The unit was assigned: " + Tactician.Assignments[picked.GetUnit()];
            UnitLeader leader = picked.GetUnit().GetComponent<UnitLeader>();
            if (leader.HasWaypoint())
            {
                moveDetails += " Waypoint was at " + leader.WayPoint.myCoords;//+ " Waypoint is for " + leader.WaypointTowards.ToString();
            }

            if (Tactician.Assignments[picked.GetUnit()].objective.EvaluateUnitViability(picked.GetUnit()) < 0)
            {
                leader.LogObjectivesList();
            }

            Debug.Log(moveDetails);


            commander.SpoofSendCommand(picked.GetDestinationCoords());
            selector.HandleDeselect();
            yield return null;

            //unmoved.Remove(choice.GetUnit());

            int cycles = 0;

            //wait until the unit is moving
            while (!manager.IsUnitMoving() && cycles < 600)
            {
                //Rounding off time scale means that frames during a pause won't be counted
                cycles += Mathf.RoundToInt(Time.timeScale);
                Debug.Assert(cycles < 600, "!ERROR! commander caught in endless wait loop");
                yield return null;
            }

            cycles = 0;

            //Wait until the unit is done moving
            while (manager.IsUnitMoving() && cycles < 600)
            {
                yield return null;
                cycles += Mathf.RoundToInt(Time.timeScale);
            }
            Debug.Assert(cycles < 600, "!ERROR! commander caught in endless wait loop");

            bestMoves.Clear();


            unmoved = militaryManager.GetListOfReadyUnits(Faction.ComputerTeam);
            //The AI will hopefully not be dumb enough to get its own units killed, but I'm not leaving this up to chance
            myUnits = militaryManager.GetListOfUnits(Faction.ComputerTeam);
        }
        Debug.Log("The Commander took " + (Time.realtimeSinceStartup - turnStarted) + " seconds to complete its turn");

        selector.HandleDeselect();
        yield return null;
        
        manager.HandleTurnEnd(Faction.ComputerTeam);
        
    }




    private IEnumerator IssueDirectives()
    {
        WaitForSeconds delay = new WaitForSeconds(1.0f);
        int unitsAvailable = military.Length;

        float timeStarted = Time.realtimeSinceStartup;
        yield return null;
        int unmovable = 0;

        //List of units that haven't moved this turn
        List<Unit> unmoved = new List<Unit>(military);

        //List of the best possible moves for all units;
        List<Directive> moves = new List<Directive>();

        //When the list unmoved is empty all units have been moved and the turn can end
        while(unmoved.Count > 0 && !manager.IsBattleOver())
        {
            int idx = 0;


            //Check each unit for moves
            while (idx < unmoved.Count)
            {
                //issues may arise if the selected unit was selected by the player as their final move
                selector.HandleDeselect();


                

                //Start by considering what the player might do next turn
                //Unit[] enemies = manager.GetPlayerMilitary();
                Unit[] enemies = militaryManager.GetListOfUnits(Faction.PlayerTeam).ToArray();
                intel.WipeOldData();
                foreach(Unit enemy in enemies)
                {
                    

                    selector.HandleAISelection(enemy.myTilePos);

                    //yield return null;

                    selector.PerformTacticalAnalysis();
                    //yield return null;

                    selector.HandleDeselect();
                }


                //Tactician.GenerateObjectives();
                yield return Tactician.AssignObjectives();

                if (unmoved[idx].myState == UnitState.ready)
                {
                    yield return null;



                    //yield return null;

                    selector.HandleAISelection(unmoved[idx].myTilePos);

                    //Only keep the best moves for consideration

                    yield return StartCoroutine(selector.GetSmartestMoves(intel));
                    
                    moves = MaintainBest(moves, selector.GetDirectives()) ;

                    Debug.Assert(moves.Count > 0, "!ERROR! the AI selected a "
                        + unmoved[idx].GetTitle() + " with no possible moves it was in the "
                        + unmoved[idx].myState + " state");
                    Debug.Assert(unmoved[idx].GetCurrentTilePos() == unmoved[idx].myTilePos);

                    yield return null;

                    //increment idx to look at next unit
                    idx++;
                }
                else
                {
                    unmovable++;
                    Debug.Log("The AI selected a "
                        + unmoved[idx].GetTitle() + " at tile: " + unmoved[idx].myTilePos
                        + " It is not ready to move");
                    //Remove unmovable units from the list, DO NOT increment idx
                    unmoved.RemoveAt(idx);
                }

            }//All possible moves have been analyzed



            selector.HandleDeselect();
            yield return null;

            if(moves.Count > 0)
            {
                Directive choice = RandomDirective(moves);


                //Describes the move in the console
                string moveDetails = "Chosen move was: " + choice.ToString() + " The unit was assigned: " + Tactician.Assignments[choice.GetUnit()];
                UnitLeader leader = choice.GetUnit().GetComponent<UnitLeader>();
                if (leader.HasWaypoint())
                {
                    moveDetails += " Waypoint was at " + leader.WayPoint.myCoords + " Waypoint is for " + leader.WaypointTowards.ToString();
                }

                if(Tactician.Assignments[choice.GetUnit()].objective.EvaluateUnitViability(choice.GetUnit()) < 0)
                {
                    leader.LogObjectivesList();
                }

                Debug.Log(moveDetails);

                selector.HandleAISelection(choice.GetUnit().myTilePos);


                //yield return null;
                //yield return null;

                commander.SpoofSendCommand(choice.GetDestinationCoords());
                selector.HandleDeselect();
                yield return null;

                unmoved.Remove(choice.GetUnit());

                int cycles = 0;

                //wait until the unit is moving
                while (!manager.IsUnitMoving() && cycles < 600)
                {
                    //Rounding off time scale means that frames during a pause won't be counted
                    cycles += Mathf.RoundToInt(Time.timeScale);
                    Debug.Assert(cycles < 600, "!ERROR! commander caught in endless wait loop");
                    yield return null;
                }

                cycles = 0;

                //Wait until the unit is done moving
                while (manager.IsUnitMoving() && cycles < 600)
                {
                    yield return null;
                    cycles += Mathf.RoundToInt(Time.timeScale);
                }
                Debug.Assert(cycles < 600, "!ERROR! commander caught in endless wait loop");
            }



            intel.WipeOldData();
            moves.Clear();
        }//All units have been moved

        Debug.Log(" The commander took " + (Time.realtimeSinceStartup - timeStarted) + " seconds to perform its turn");

        


        selector.HandleDeselect();
        yield return null;
        if (unmovable > 0)
        {
            manager.HandleTurnEnd(Faction.ComputerTeam);
        }

    }


    // Just choose a random command from possibilities
    // no real intelligence, mostly a place holder
    private HexOverlay RandomCommand(List<HexOverlay> options)
    {
        
        int pick = Random.Range(0, options.Count);

        return options[pick];
    }



    private Directive RandomDirective(List<Directive> options)
    {
        int pick = Random.Range(0, options.Count);

        return options[pick];
    }






    //Selection processes


    //This function takes a list of hexOverlays and returns a list of HexOverlays with an attack state
    private List<HexOverlay> AttackFilter(List<HexOverlay> options, Unit unit)
    {
        List<HexOverlay> attacks = new List<HexOverlay>();

        bool critical = false;


        foreach (HexOverlay hex in options)
        {
            if(hex.currState == HexState.attackable && !critical)
            {

                float chance = Random.Range(0.0f, 1.0f) * MAXCAUTION;
                Unit enemy = hex.GetOccupiedBy();

                
                
                //Make the AI avoid fights it will lose, unless it isn't cautious
                if (!unit.IsWeakTo(enemy) || (chance > caution))
                {

                    //Stop a capture at all costs
                    if (enemy.IsCapturing())
                    {
                        //Ignore everything else
                        critical = true;
                        attacks.Clear();
                    }

                    attacks.Add(hex);
                    

                }
                
                
            }
            
        }

        return attacks;
    }



    //This function will return a list of moves that will provide constructive movement toward a direction
    //Case and point the returned moves don't have the unit turning around and undoing the previous move
    private List<HexOverlay> ProductiveMovementFilter(List<HexOverlay> options, Unit unit)
    {
        List<HexOverlay> useful = new List<HexOverlay>();
        bool firstMove = unit.prevTilePos == unit.myTilePos;



        foreach(HexOverlay hex in options)
        {
            int distPrev = (hex.myCoords - unit.prevTilePos).sqrMagnitude;
            int distCurr = (hex.myCoords - unit.myTilePos).sqrMagnitude;

            if (firstMove)
            {
                //For the first move we'll consider a move of significant distance productive
                if (hex.distanceFrom >= unit.GetMobility() - 1)
                {
                    useful.Add(hex);
                }
            }
            else
            {
                //If the distance from hex to the previous position is more than the current position
                //Than we must be going in roughly the same direction
                if (distPrev > distCurr && hex.distanceFrom >= unit.GetMobility() - 1)
                {
                    useful.Add(hex);
                }
            }

            
        }


        return useful;
    }



    private List<HexOverlay> CaptureFilter(List<HexOverlay> options)
    {
        List<HexOverlay> captures = new List<HexOverlay>();

        int idx = 0;
        bool found = false;
        while(idx < options.Count && !found)
        {
            if(options[idx].currState == HexState.capture)
            {
                found = true;
                captures.Add(options[idx]);
            }
            idx++;
        }

        return captures;
    }



    /*
     * MAINTAIN BEST
     * 
     * Given two lists of directives this method returns a list containing only the smartest moves
     * 
     * !NOTE! this method assumes both parameter lists contain directives of equal intelligence.
     * Case and point this method has 3 possible outcomes it returns baseline, it returns candidates,
     * or it returns a list with all entries from both lists.
     */
    private List<Directive> MaintainBest(List<Directive> baseline, List<Directive> candidates)
    {
        List<Directive> best = baseline;

        if(baseline.Count == 0 || candidates.Count == 0)
        {
            if(baseline.Count == 0)
            {
                //The possibility remains that the returned list is empty but this is still a proper response
                best = candidates;
            }
            
        }
        else
        {
            int smartest = best[0].GetSmartness();

            if(candidates[0].GetSmartness() >= smartest)
            {
                if(candidates[0].GetSmartness() > smartest)
                {
                    best = candidates;
                }
                else
                {
                    best.AddRange(candidates);
                }
            }
        }

        return best;
    }
}
