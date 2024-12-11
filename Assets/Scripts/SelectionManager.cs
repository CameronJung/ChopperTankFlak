using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Tilemap map;
    [SerializeField] private ReconPanel recon;
    [SerializeField] private GameObject cursor;
    [SerializeField] private CommandTracer commander;
    

    public Unit selectedUnit { get; private set; }

    private Strategist Tactician;

    private List<HexOverlay> affectedHexes;
    public Vector3Int selectedPos { get; private set; } = new Vector3Int();

    private List<Directive> Directives;
    private AStarMeasurement Ruler;
    private GlobalNavigationData Navigation;
    private ControlsManager Controller;

    //Variables to remember to revert selection state
    private List<HexOverlay> MemAffected;
    private Vector3Int MemPos;
    private Unit MemUnit = null;
    private bool MemHadSelection = false;


    // Start is called before the first frame update
    void Start()
    {
        Controller = GameObject.Find(UniversalConstants.MANAGERPATH).GetComponent<ControlsManager>();
        Ruler = gameObject.GetComponent<AStarMeasurement>();
        Navigation = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<GlobalNavigationData>();
    }

    public void RememberStrategist(Strategist strat)
    {
        Tactician = strat;
    }
    

    private void LogSelectionState()
    {
        MemAffected = affectedHexes;
        MemPos = selectedPos;
        MemUnit = selectedUnit;
        MemHadSelection = cursor.activeInHierarchy;
    }

    private void RevertSelectionState()
    {
        
        cursor.SetActive(MemHadSelection);

        if (MemHadSelection)
        {
            HandleNewSelection(MemPos);
            if (MemUnit != null)
            {
                HandleUnitSelected();
            }
        }
        else
        {
            HandleDeselect();
        }

        /*
        */

        
        
    }

    public void HandlePlayerNewSelection(Vector3Int tilePos)
    {
        if(!Controller.CurrentPermissions.CheckSpecifiableControlPermission(SpecifiableControls.select, ControlAccess.forbidden))
        {



            HandleNewSelection(tilePos);
            Debug.Log("selected tile is: " + selectedPos);

            if(Controller.CurrentPermissions.CheckSpecifiableControlPermission(SpecifiableControls.select, ControlAccess.conditional))
            {
                if (!Controller.IsConditionSatisfied())
                {
                    RevertSelectionState();
                }
            }


        }

        
    }

    public void HandlePlayerDeselection()
    {
        if (Controller.CurrentPermissions.CheckBooleanControlPermission(BooleanControls.deselect))
        {
            HandleDeselect();

            if (Controller.CurrentPermissions.CheckSpecifiableControlPermission(SpecifiableControls.select, ControlAccess.conditional))
            {
                if (!Controller.IsConditionSatisfied())
                {
                    RevertSelectionState();
                }
            }
        }
    }


    public void HandleNewSelection(Vector3Int tilePos, bool conspicuous = true)
    {
        LogSelectionState();
        GameObject obj = map.GetInstantiatedObject(tilePos);


        if(obj != null)
        {
            HexOverlay hex = obj.GetComponent(typeof(HexOverlay)) as HexOverlay;


            if (tilePos == selectedPos)
            {
                //If we have clicked on the same tile display the other type of data
                if (selectedUnit != null)
                {
                    HandleUnitDeselected();
                    recon.DisplayIntelAbout(map.GetTile<TerrainTile>(tilePos), tilePos);
                }
                else if (hex.GetOccupiedBy() != null)
                {
                    //Units that are proccessing a mission should be unselectable
                    if (!hex.GetOccupiedBy().HasMission())
                    {
                        HandleUnitSelected(conspicuous);
                        recon.DisplayIntelAbout(hex.GetOccupiedBy(), tilePos);
                    }
                    else
                    {
                        recon.DisplayIntelAbout(map.GetTile<TerrainTile>(tilePos), tilePos);
                    }
                    
                }
            }
            //Otherwise send the unit if one is present
            else
            {
                selectedPos = tilePos;
                if (hex.GetOccupiedBy() != null)
                {
                    HandleUnitDeselected();
                    recon.DisplayIntelAbout(hex.GetOccupiedBy(), tilePos);
                    HandleUnitSelected(conspicuous);
                }
                else
                {
                    HandleUnitDeselected();
                    recon.DisplayIntelAbout(map.GetTile<TerrainTile>(tilePos), tilePos);
                }
            }


            if (conspicuous)
            {
                cursor.SetActive(true);
                cursor.transform.position = map.CellToWorld(tilePos);
            }
            
        }
        
    }


    /*
     * HandleAISelection
     * 
     * This method handles selections made by the AI it differs from handleNewSelection in that it does not make
     * visible changes to the UI or game board.
     */
    public void HandleAISelection(Vector3Int tilePos)
    {
        GameObject obj = map.GetInstantiatedObject(tilePos);


        if (obj != null)
        {
            HexOverlay hex = obj.GetComponent(typeof(HexOverlay)) as HexOverlay;


            if (tilePos == selectedPos)
            {
                //If we have clicked on the same tile display the other type of data
                if (selectedUnit != null)
                {
                    HandleUnitDeselected();
                    Debug.Log("AI deselected unit by clicking twice");
                }
                else if (hex.GetOccupiedBy() != null)
                {
                    HandleUnitSelected(false);
                }
            }
            //Otherwise send the unit if one is present
            else
            {
                selectedPos = tilePos;
                if (hex.GetOccupiedBy() != null)
                {
                    HandleUnitDeselected();
                    HandleUnitSelected(false);
                }
                else
                {
                    HandleUnitDeselected();
                }
            }

        }
    }



    /*
     * Perform Tactical Analysis
     * 
     * This method is called by the AI while a player unit is selected
     * It examines the moves that could be made by the player's unit and assigns a bounty based on that
     */
    public void PerformTacticalAnalysis()
    {
        //Ensure proper use of function
        if(selectedUnit.GetAllegiance() == UniversalConstants.Faction.PlayerTeam)
        {

            List<Directive> directives = new List<Directive>();



            /*
            GameObject obj = map.GetInstantiatedObject(selectedPos);
            HexOverlay hextile = obj.GetComponent(typeof(HexOverlay)) as HexOverlay;
            
            affectedHexes = hextile.BeginExploration(selectedUnit, false);
            */

            affectedHexes = selectedUnit.TacticalAnalysis(false);

            foreach (HexOverlay hex in affectedHexes)
            {
                if(hex.currState == UniversalConstants.HexState.attackable || hex.currState == UniversalConstants.HexState.capture)
                {
                    directives.Add(new Directive(selectedUnit, hex));
                }
            }
            selectedUnit.SetBounty(directives);
            
            HandleDeselect();
        }
    }


    public void HandleDeselect()
    {
        cursor.SetActive(false);
        if(selectedUnit != null)
        {
            HandleUnitDeselected();
        }
        recon.DisplayNone();
    }


    /*
     * HandleUnitSelection
     * 
     * This function is called when a unit is selected and handles the nuances involved with the process
     * the parameter conspicuous indicates if the selection should result in possible moves being displayed on the gameboard
     * 
     */
    private void HandleUnitSelected(bool conspicuous = true)
    {
        selectedUnit = map.GetInstantiatedObject(selectedPos).GetComponent<HexOverlay>().GetOccupiedBy();
        affectedHexes = selectedUnit.OnSelected(conspicuous);
        commander.StartDrawingCommand(selectedUnit);
    }



    private void HandleUnitDeselected()
    {
        if(selectedUnit != null)
        {
            commander.StopDrawingCommand();
            //reset tiles
            foreach (HexOverlay hex in affectedHexes)
            {
                hex.SetBlank();
            }
            selectedUnit = null;
        }
        
    }

    public bool IsUnitSelected()
    {
        return selectedUnit != null;
    }



    //Returns a list of hexes the selected unit can move to
    //this is mostly used by the AI
    public List<HexOverlay> GetPossibilities()
    {
        Debug.Assert(selectedUnit != null, "There is no unit selected");
        List<HexOverlay> possibilities = new List<HexOverlay>();

        bool viable;
        foreach(HexOverlay hex in affectedHexes)
        {
            viable = false;
            
            if (hex.CanIpass(selectedUnit))
            {
                Unit occupier = hex.GetOccupiedBy();
                if (occupier != null)
                {
                    //Throw hexes that are occupied by other allied units
                    viable = (occupier.GetAllegiance() != selectedUnit.GetAllegiance()
                        || occupier == selectedUnit);
                }
                else
                {
                    viable = true;
                }

            }

            if (viable)
            {
                possibilities.Add(hex);
            }
            
        }


        return possibilities;
    }

    //Returns a list of directives the selected unit can move to
    //this is mostly used by the AI
    public IEnumerator GetSmartestMoves(AIIntelHandler knowledge)
    {

        
        float timeSpentMeasuring = 0.0f;
        
        float timeForTurn = Time.realtimeSinceStartup;

        Debug.Assert(selectedUnit != null, "There is no unit selected");
        Directives = new List<Directive>();

        Vector3Int tacticalGoal = Tactician.GetDestinationOfUnit(selectedUnit);

        float timeForMeasure = Time.realtimeSinceStartup;
        yield return Ruler.BeginMeasurement(selectedUnit, tacticalGoal);
        timeForMeasure = Time.realtimeSinceStartup - timeForMeasure;
        timeSpentMeasuring += timeForMeasure;
        float numberOfMeasurements = 1.0f;
        int unitDistance = Ruler.GetMeasuredDistance();
        int destDistance = -1;

        
        Debug.Assert(unitDistance >= 0, "Ruler was not finished measuring distance for Unit");
        

        


        bool viable;
        foreach (HexOverlay hex in affectedHexes)
        {
            viable = false;

            //if (hex.CanIpass(selectedUnit))

            if(selectedUnit.CanTakeMissionTo(hex) && hex.RepresentsPossibleMoveTo(selectedUnit))
            {
                Unit occupier = hex.GetOccupiedBy();
                if (occupier != null)
                {
                    //Throw hexes that are occupied by other allied units
                    viable = (occupier.GetAllegiance() != selectedUnit.GetAllegiance()
                        || occupier == selectedUnit);
                }
                else
                {
                    viable = true;
                }

            }

            if (viable)
            {
                Directive noob = new Directive(selectedUnit, hex, knowledge);

                noob.ShowSmartnessOnBoard();

                if (hex.distanceFrom == selectedUnit.GetMobility())
                {

                    if (Navigation.HasDistance(selectedUnit.GetUnitType(), hex.myCoords, tacticalGoal))
                    {
                        //re-use distances already computed
                        destDistance = Navigation.GetDistance(selectedUnit.GetUnitType(), hex.myCoords, tacticalGoal);
                    }
                    else { 
                        timeForMeasure = Time.realtimeSinceStartup;
                        //yield return null;
                        yield return Ruler.BeginMeasurement(selectedUnit, tacticalGoal, hex.myCoords);
                        destDistance = Ruler.GetMeasuredDistance();
                        Navigation.LogDistance(selectedUnit.GetUnitType(), hex.myCoords, tacticalGoal, destDistance);
                        timeForMeasure = Time.realtimeSinceStartup - timeForMeasure;
                        timeSpentMeasuring += timeForMeasure;
                        numberOfMeasurements += 1.0f;
                    }
                }
                else
                {
                    destDistance = -1;
                }

                
                
                //hex.nav.MeasureTrueDistance(selectedUnit, knowledge.GetPlayerBaseLoc());

                noob.SetDistances(unitDistance, destDistance);

                if(Directives.Count > 0) { 

                    //Maintain possibilities as a list of the best possible moves
                    if(noob.GetSmartness() >= Directives[0].GetSmartness())
                    {
                        if(noob.GetSmartness() > Directives[0].GetSmartness())
                        {
                            Directives.Clear();
                        }
                        Directives.Add(noob);
                    }
                }
                else
                {
                    Directives.Add(noob);
                }
                
            }

        }

        if (Application.isEditor)
        {
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            yield return null;
        }
        

        timeForTurn = Time.realtimeSinceStartup - timeForTurn;
        //Debug.Log("It Took " + timeForTurn + " seconds to make a move.");
        //Debug.Log(timeSpentMeasuring + " seconds were spent measuring distances");
        //Debug.Log("A total of " + numberOfMeasurements + " measurements were made");
        //Debug.Log("The average measurement took " + (timeSpentMeasuring / numberOfMeasurements) + " seconds to compute");
        //return possibilities;
    }



    /*
     * Get Smartest Moves
     * 
     * This Method returns a list of directives that represent the smartest moves available to the Unit in the parameter, "unit"
     * 
     */
    public List<Directive> GetSmartestMoves(AIIntelHandler knowledge, Unit unit)
    {
        List<Directive> moves = new List<Directive>();
        List<HexOverlay> options = unit.OnSelected(false);

        int smartest = int.MinValue;

        foreach(HexOverlay hex in options)
        {

            Directive move = new Directive(unit, hex, knowledge);

            if(move.GetSmartness() >= smartest)
            {
                if(move.GetSmartness() > smartest)
                {
                    //If we find a better move toss out the dumber ones
                    moves.Clear();
                    smartest = move.GetSmartness();
                }

                moves.Add(move);

            }

        }

        return moves;
    }


    public List<Directive> GetDirectives()
    {
        if(Directives == null)
        {
            //This shouldn't happen but if it does return an empty list
            Debug.Log("Tried to get directives that didn't exist");
            this.Directives = new List<Directive>();
        }
        return this.Directives;
    }

}
