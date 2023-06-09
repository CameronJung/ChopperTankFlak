using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;

public class AIcommander : MonoBehaviour
{
    private const int MAXCAUTION = 10;
    private const int MINCAUTION = 4;


    [SerializeField] private SelectionManager selector;
    [SerializeField] private CommandTracer commander;
    [SerializeField] private GameManager manager;

    private Unit[] military;
    private List<HexOverlay> possibilities;

    //The higher this value is the less likely the AI is to follow through with unfavorable
    //engagements
    private int caution;
    

    // Start is called before the first frame update
    void Start()
    {
        caution = Random.Range(MINCAUTION, MAXCAUTION +1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TakeTurn(List<Unit> units)
    {
        this.military = units.ToArray();
        StartCoroutine(IssueOrders());
    }


    private IEnumerator IssueOrders()
    {
        WaitForSeconds delay = new WaitForSeconds(1.0f);
        int unitsAvailable = military.Length;


        yield return null;
        int unmovable = 0;


        
        
        for(int idx = 0; idx < unitsAvailable; idx++)
        {
            //issues may arise if the selected unit was selected by the player as their final move
            selector.HandleDeselect();


            if (military[idx].myState == UnitState.ready)
            {
                yield return null;

                

                yield return null;

                selector.HandleNewSelection(military[idx].myTilePos);

                possibilities = selector.GetPossibilities();

                Debug.Assert(possibilities.Count > 0, "!ERROR! the AI selected a "
                    + military[idx].GetTitle() + " with no possible moves it was in the "
                    + military[idx].myState + " state");
                Debug.Assert(military[idx].GetCurrentTilePos() == military[idx].myTilePos);

                yield return delay;
                
                

                HexOverlay choice = RandomCommand(possibilities);


                List<HexOverlay> special = new List<HexOverlay>();
                //List<HexOverlay> attacks = AttackFilter(possibilities, military[idx]);
                //List<HexOverlay> useful = ProductiveMovementFilter(possibilities, military[idx]);

                //If infantry try to capture a facility
                if(military[idx].GetUnitType() == UnitType.InfantrySquad)
                {
                    special = CaptureFilter(possibilities);
                }

                //Otherwise look for enemies to attack
                if(special.Count == 0 || military[idx].GetUnitType() != UnitType.InfantrySquad)
                {
                    special = AttackFilter(possibilities, military[idx]);
                }

                //Otherwise try to move in a useful manner
                if (special.Count == 0)
                {
                    special = ProductiveMovementFilter(possibilities, military[idx]);
                }

                //If all else fails do something random
                if (special.Count == 0)
                {
                    special = possibilities;
                }

                //pick an option
                choice = RandomCommand(special);

                commander.SpoofSendCommand(choice.myCoords);
                selector.HandleDeselect();
                yield return null;

                int cycles = 1;

                //wait until the unit is moving
                while (!manager.IsUnitMoving() && cycles < 600)
                {
                    cycles++;
                    Debug.Assert(cycles < 600, "!ERROR! commander caught in endless wait loop");
                    yield return null;
                }

                cycles = 0;

                //Wait until the unit is done moving
                while (manager.IsUnitMoving() && cycles < 600)
                {
                    yield return null;
                    cycles++;
                }

                Debug.Assert(cycles < 600, "!ERROR! commander caught in endless wait loop");


                
            }
            else
            {
                unmovable++;
                Debug.Log("The AI selected a "
                    + military[idx].GetTitle() + " at tile: " + military[idx].myTilePos
                    + " It is not ready to move");
            }

            yield return null;

        }


        selector.HandleDeselect();
        yield return null;
        if(unmovable > 0)
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
}
