using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class GameManager : MonoBehaviour
{
    private int turn = 0;

    [SerializeField] private AIcommander enemyCO;
    [SerializeField] private ClickHandler clicker;
    [SerializeField] private EndGamePanel gameEnd;
    private AIIntelHandler intel;

    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> computerUnits = new List<Unit>();

    private int numUnitsMoving = 0;

    private int numComputerUnitsReady = 0;
    private int numPlayerUnitsReady = 0;

    private bool turnComplete = false;

    public bool unitMoving { get; private set; } = false;

    public int unitsAvailable { get; private set; } = 0;




    //Changes to true when the game ends for some reason
    private bool battleOver = false;


    // Start is called before the first frame update0
    void Start()
    {
        intel = enemyCO.gameObject.GetComponent<AIIntelHandler>();
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
        turnComplete = false;
        RevitalizeTeam();

        if(turn % 2 == 0)
        {
            clicker.AllowClicks();
        }
        else
        {
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

        bool gameOver = false;
        if (casualty.GetAllegiance() == Faction.PlayerTeam)
        {
            
            playerUnits.Remove(casualty);
            gameOver = (playerUnits.Count == 0) ;
        }
        else
        {
            computerUnits.Remove(casualty);
            gameOver = (computerUnits.Count == 0);
        }

        if (gameOver)
        {
            HandleBattleOver(computerUnits.Count == 0);
        }

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
                HandleTurnEnd(WhosTurn());
            }
            unitMoving = false;
        }
        
    }


    //Called by a unit when they start performing an action

    public void ReportActionStarted(bool isRetaliation = false)
    {
        
        if(WhosTurn() == Faction.PlayerTeam)
        {
            clicker.BlockClicks();
        }
        intel.WipeOldData();

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
        int cycles = 0;
        while (IsUnitMoving())
        {
            yield return null;
            cycles++;
            Debug.Assert(cycles < 600, "movement is taking too long to end");
        }

        EndTurn();
    }


    //Called to end a turn voluntarily or automatically
    public void HandleTurnEnd(UniversalConstants.Faction faction)
    {
        if (!turnComplete && WhosTurn() == faction)
        {
            turnComplete = true;
            StartCoroutine(AwaitTurnChange());
        }
        
    }

    public void PlayerEndTurn()
    {
        if (!battleOver)
        {
            HandleTurnEnd(Faction.PlayerTeam);
        }
        
    }

    private void HandleBattleOver(bool victorious)
    {
        battleOver = true;
        gameEnd.gameObject.SetActive(true);
        clicker.BlockClicks();
        gameEnd.HandleGameEnd(victorious);
    }

    public void ReportHQCapture(Faction victor)
    {
        HandleBattleOver(victor == Faction.PlayerTeam);
    }


    public Unit[] GetPlayerMilitary()
    {
        return playerUnits.ToArray();
    }
}
