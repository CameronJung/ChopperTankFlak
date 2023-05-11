using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;

public class AIcommander : MonoBehaviour
{
    private const int MAXCAUTION = 10;
    private const int MINCAUTION = 2;

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
        Debug.Log("The AI has a caution level of: " + caution);
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

        yield return null;
        int unitsAvailable = military.Length;


        
        Debug.Log("There are " + unitsAvailable + " enemy units ready to move.");
        
        for(int idx = 0; idx < unitsAvailable; idx++)
        {


            if(military[idx].myState == UnitState.ready)
            {
                yield return null;

                

                yield return null;

                selector.HandleNewSelection(military[idx].myTilePos);
                possibilities = selector.GetPossibilities();

                yield return delay;

                Debug.Assert(possibilities.Count > 0, "!ERROR! the AI selected a " 
                    + selector.selectedUnit.GetTitle() + " with no possible moves it was in the "
                    + selector.selectedUnit.myState + " state");

                HexOverlay choice = RandomCommand(possibilities);

                List<HexOverlay> attacks = AttackFilter(possibilities, military[idx]);
                List<HexOverlay> useful = ProductiveMovementFilter(possibilities, military[idx]);

                if(attacks.Count > 0)
                {
                    choice = RandomCommand(attacks);
                }
                else if (useful.Count > 0)
                {
                    choice = RandomCommand(useful);
                }


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


                yield return null;
            }

            

        }


        selector.HandleDeselect();


        yield return delay;
        //manager.EndTurn();

        yield return null;
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

        foreach (HexOverlay hex in options)
        {
            if(hex.currState == HexState.attackable)
            {
                if (unit.IsWeakTo(hex.GetOccupiedBy()))
                {
                    float chance = Random.Range(0.0f, 1.0f) * MAXCAUTION;
                    if(chance > caution)
                    {
                        attacks.Add(hex);
                    }
                }
                else
                {
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

        foreach(HexOverlay hex in options)
        {
            int distPrev = (hex.myCoords - unit.prevTilePos).sqrMagnitude;
            int distCurr = (hex.myCoords - unit.myTilePos).sqrMagnitude;

            //If the distance from hex to the previous position is more than the current position
            //Than we must be going in roughly the same direction
            if( distPrev > distCurr && hex.distanceFrom >= unit.GetMobility() -1)
            {
                useful.Add(hex);
            }
        }


        return useful;
    }
}
