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
        WaitForFixedUpdate wait = new WaitForFixedUpdate();
        WaitForSeconds delay = new WaitForSeconds(1.0f);

        yield return wait;
        int unitsAvailable = manager.unitsAvailable;
        
        Debug.Log("There are " + unitsAvailable + " enemy units ready to move.");
        
        for(int idx = 0; idx < unitsAvailable; idx++)
        {
            yield return delay;

            selector.HandleNewSelection(military[idx].myTilePos);
            possibilities = selector.GetPossibilities();
            
            commander.SpoofSendCommand(RandomCommand().myCoords);

            while (manager.unitMoving)
            {
                yield return wait;
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
    private HexOverlay RandomCommand()
    {
        Debug.Log("There are " + possibilities.Count + " possible orders.");

        int pick = Random.Range(0, possibilities.Count);

        Debug.Log("Option #" + pick + " was selected.");

        return possibilities[pick];
    }


}
