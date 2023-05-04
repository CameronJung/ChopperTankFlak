using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;

public class AIcommander : MonoBehaviour
{
    [SerializeField] private SelectionManager selector;
    [SerializeField] private CommandTracer commander;
    [SerializeField] private GameManager manager;

    private Unit[] military;
    private List<HexOverlay> possibilities;
    
    private bool myTurn = false;
    private bool turnStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (myTurn)
        {
            if (turnStarted)
            {
                turnStarted = false;
                StartCoroutine(IssueOrders());
            }
        }
    }


    public void TakeTurn(List<Unit> units)
    {
        this.military = units.ToArray();
        myTurn = true;
        turnStarted = true;
    }


    private IEnumerator IssueOrders()
    {
        //WaitForFixedUpdate wait = new WaitForFixedUpdate();
        WaitForSeconds delay = new WaitForSeconds(1.0f);

        yield return null;
        int unitsAvailable = military.Length;


        
        Debug.Log("There are " + unitsAvailable + " enemy units ready to move.");
        
        for(int idx = 0; idx < unitsAvailable; idx++)
        {


            if(military[idx].myState == UnitState.ready)
            {
                yield return null;

                selector.HandleDeselect();

                yield return null;

                selector.HandleNewSelection(military[idx].myTilePos);
                possibilities = selector.GetPossibilities();

                yield return delay;

                Debug.Assert(possibilities.Count > 0, "!ERROR! the AI selected a " 
                    + selector.selectedUnit.GetTitle() + " with no possible moves it was in the "
                    + selector.selectedUnit.myState + " state");

                HexOverlay choice = RandomCommand(possibilities);

                List<HexOverlay> attacks = AttackFilter(possibilities);

                if(attacks.Count > 0)
                {
                    choice = RandomCommand(attacks);
                }

                commander.SpoofSendCommand(choice.myCoords);

                yield return null;

                int cycles = 1;
                while (manager.IsUnitMoving() && cycles < 600)
                {
                    yield return null;
                    cycles++;
                }

                Debug.Assert(cycles < 600, "!ERROR! commander caught in endless wait loop");
                Debug.Log("waited for " + cycles + " frames");


                yield return null;
            }

            

        }


        selector.HandleDeselect();

        myTurn = false;

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
    private List<HexOverlay> AttackFilter(List<HexOverlay> options)
    {
        List<HexOverlay> attacks = new List<HexOverlay>();

        foreach (HexOverlay hex in options)
        {
            if(hex.currState == HexState.attackable)
            {
                attacks.Add(hex);
            }
            
        }

        return attacks;
    }
}
