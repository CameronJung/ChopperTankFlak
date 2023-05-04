using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class GameManager : MonoBehaviour
{
    private int turn = 0;

    [SerializeField] private AIcommander enemyCO;
    [SerializeField] private ClickHandler clicker;

    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> computerUnits = new List<Unit>();

    private int numUnitsMoving = 0;

    private int numComputerUnitsReady = 0;
    private int numPlayerUnitsReady = 0;



    public bool unitMoving { get; private set; } = false;

    public int unitsAvailable { get; private set; } = 0;




    //Changes to true when the game ends for some reason
    //private bool gameEnd = false;


    // Start is called before the first frame update0
    void Start()
    {
        BeginTurn();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public void ReportForDuty(Unit unit)
    {
        
        if(unit.GetAllegiance() == Faction.PlayerTeam)
        {
            playerUnits.Add(unit);
        }
        else
        {
            computerUnits.Add(unit);
        }
    }

    private void BeginTurn()
    {
        
        RevitalizeTeam();

        if(turn % 2 == 0)
        {
            Debug.Log("Player's turn begins");
            clicker.AllowClicks();
        }
        else
        {
            Debug.Log("Computer turn begins");
            clicker.BlockClicks();
            enemyCO.TakeTurn(computerUnits);
        }
    }



    //Handles operations for ending a turn
    private void EndTurn()
    {
        turn++;
        BeginTurn();
    }

    


    private void RevitalizeTeam()
    {
        unitsAvailable = 0;
        if (turn % 2 == 0)
        {
            foreach (Unit unit in playerUnits)
            {
                
                if (unit.Revitalize())
                {
                    unitsAvailable++;
                    numPlayerUnitsReady++;
                }
            }
        }
        else
        {
            foreach (Unit unit in computerUnits)
            {
                if (unit.Revitalize())
                {
                    unitsAvailable++;
                    numComputerUnitsReady++;
                }
            }
        }
    }




    public void ReportDeath(Unit casualty)
    {


        if (casualty.GetAllegiance() == Faction.PlayerTeam)
        {
            playerUnits.Remove(casualty);
        }
        else
        {
            computerUnits.Remove(casualty);
        }


        Debug.Log(casualty.GetAllegiance() + " " + casualty.GetUnitType() + " has died");
    }



    public void ReportActionComplete(Unit unit)
    {
        

        if (WhosTurn() == Faction.PlayerTeam)
        {
            clicker.AllowClicks();
            numPlayerUnitsReady--;
        }
        else
        {
            numComputerUnitsReady--;
        }
        numUnitsMoving--;

        if (unit.GetAllegiance() == WhosTurn())
        {
            unitsAvailable--;
            if (unitsAvailable == 0)
            {
                HandleTurnEnd();
            }
            unitMoving = false;
            Debug.Log("There are " + unitsAvailable + " units left to move.");
        }
        
    }


    //Called by a unit when they start performing an action

    public void ReportActionStarted(bool isRetaliation = false)
    {
        if(WhosTurn() == Faction.PlayerTeam)
        {
            clicker.BlockClicks();
        }


        unitMoving = true;
        numUnitsMoving++;
    }


    public Faction WhosTurn()
    {
        if (turn % 2 == 0)
        {
            return Faction.PlayerTeam;
        }
        else
        {
            return Faction.ComputerTeam;
        }
    }





    public bool IsUnitMoving()
    {
        Debug.Assert(numUnitsMoving >= 0, "!ERROR! num units moving has become negative");
        return numUnitsMoving > 0;
    }


    private IEnumerator AwaitTurnChange() 
    {
        while (IsUnitMoving())
        {
            yield return null;
        }

        EndTurn();
    }

    public void HandleTurnEnd()
    {
        StartCoroutine(AwaitTurnChange());
    }
}
