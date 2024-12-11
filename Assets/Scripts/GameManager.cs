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
    [SerializeField] private GameMusicManager musicManager;

    [SerializeField] private GameObject pausePanel;
    private MilitaryManager militaryManager;
    private DialogueManager Director = null;


    private AIIntelHandler intel;

    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> computerUnits = new List<Unit>();

    private int numUnitsMoving = 0;

    private int numComputerUnitsReady = 0;
    private int numPlayerUnitsReady = 0;

    private bool turnComplete = false;

    public bool unitMoving { get; private set; } = false;

    public int unitsAvailable { get; private set; } = 0;

    public bool paused { get; private set; } = false;




    //Changes to true when the game ends for some reason
    private bool battleOver = false;
    private ControlsManager Controller = null;


    private void Awake()
    {
        Controller = gameObject.GetComponent<ControlsManager>();
        militaryManager = gameObject.GetComponent<MilitaryManager>();
        Director = gameObject.GetComponent<DialogueManager>();
        
    }

    // Start is called before the first frame update0
    void Start()
    {
        intel = enemyCO.gameObject.GetComponent<AIIntelHandler>();

        if (Application.isMobilePlatform)
        {
            Debug.Log("This is a mobile platform");
        }
        else
        {
            Debug.Log("This is not a mobile platform");
        }

        

        Debug.Log("Target FPS is: " + Application.targetFrameRate);
        
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

        Debug.Log($"The computer team has {militaryManager.CountTotalUnits(Faction.ComputerTeam)} units.");
        Debug.Log($"The player team has {militaryManager.CountTotalUnits(Faction.PlayerTeam)} units.");

        if (turn % 2 == 0)
        {
            clicker.AllowClicks();
            musicManager.PlayPlayerMusic();
        }
        else
        {
            clicker.BlockClicks();

            if(Director != null)
            {
                List<RequiredMove> moves = Director.CurrAct.GetAISteps();

                enemyCO.TakeTurn(moves);
            }
            else
            {
                enemyCO.TakeTurn();
            }
            



            musicManager.PlayComputerMusic();
        }
    }



    //Handles operations for ending a turn
    private void EndTurn()
    {
        turn++;
        if (Controller != null && Director != null)
        {
            Director.NextAct();
        }
        BeginTurn();

    }

    


    private void RevitalizeTeam()
    {
        if (!battleOver)
        {
            unitsAvailable = 0;

            foreach (Unit unit in militaryManager.GetListOfUnits(WhosTurn()))
            {

                if (unit.Revitalize())
                {
                    unitsAvailable++;
                    numPlayerUnitsReady++;
                }
            }
        }
        
    }




    public void ReportDeath(Unit casualty)
    {

        bool gameOver = militaryManager.CountTotalUnits(casualty.GetAllegiance()) == 0;

        if (gameOver)
        {
            HandleBattleOver(militaryManager.CountTotalUnits(Faction.ComputerTeam) == 0);
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
            if (unitsAvailable == 0 && Controller.CurrentPermissions.CheckBooleanControlPermission(BooleanControls.AutoTurnEnd))
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
        //intel.WipeOldData();

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
        if (!turnComplete && WhosTurn() == faction )
        {
            turnComplete = true;
            StartCoroutine(AwaitTurnChange());
        }
        
    }

    public void PlayerEndTurn()
    {
        if (!battleOver && Controller.CurrentPermissions.CheckBooleanControlPermission(BooleanControls.end_turn))
        {
            HandleTurnEnd(Faction.PlayerTeam);
        }
        
    }

    private void HandleBattleOver(bool victorious)
    {
        battleOver = true;
        gameEnd.gameObject.SetActive(true);
        clicker.BlockClicks();
        musicManager.StopMusic();
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

    public bool IsBattleOver()
    {
        return battleOver;
    }



    /************* PAUSE HANDELING ***************/

    public void PauseGame()
    {
        Time.timeScale = 0;
        clicker.BlockClicks();

        pausePanel.SetActive(true);
    }

    public void UnpauseGame()
    {
        if (this.WhosTurn() == UniversalConstants.Faction.PlayerTeam)
        {
            clicker.AllowClicks();
        }
        pausePanel.SetActive(false);
        Time.timeScale = 1;

    }
}
