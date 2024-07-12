using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Tilemap map;
    [SerializeField] private ReconPanel recon;
    [SerializeField] private GameObject cursor;
    [SerializeField] private CommandTracer commander;

    public Unit selectedUnit { get; private set; }


    private List<HexOverlay> affectedHexes;
    private Vector3Int selectedPos = new Vector3Int();
    private List<Directive> Directives;
    private AStarMeasurement Ruler;
    private GlobalNavigationData Navigation;

    // Start is called before the first frame update
    void Start()
    {
        Ruler = gameObject.GetComponent<AStarMeasurement>();
        Navigation = gameObject.GetComponent<GlobalNavigationData>();
    }


    

    public void HandleNewSelection(Vector3Int tilePos, bool conspicuous = true)
    {

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
                    HandleUnitSelected(conspicuous);
                    recon.DisplayIntelAbout(hex.GetOccupiedBy(), tilePos);
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
            GameObject obj = map.GetInstantiatedObject(selectedPos);
            HexOverlay hextile = obj.GetComponent(typeof(HexOverlay)) as HexOverlay;

            affectedHexes = hextile.BeginExploration(selectedUnit, false);
            

            foreach (HexOverlay hex in affectedHexes)
            {
                if(hex.currState == UniversalConstants.HexState.attackable || hex.currState == UniversalConstants.HexState.capture)
                {
                    directives.Add(new Directive(selectedUnit, hex));
                }
                //hex.nav.ChangeDebugTextTo(hex.intel.GetDebugString());
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

        float timeForMeasure = Time.realtimeSinceStartup;
        yield return Ruler.BeginMeasurement(selectedUnit, knowledge.GetPlayerBaseLoc());
        timeForMeasure = Time.realtimeSinceStartup - timeForMeasure;
        timeSpentMeasuring += timeForMeasure;
        float numberOfMeasurements = 1.0f;
        int unitDistance = Ruler.GetMeasuredDistance();
        int destDistance = -1;

        
        Debug.Assert(unitDistance > 0, "Ruler was not finished measuring distance for Unit");
        


        bool viable;
        foreach (HexOverlay hex in affectedHexes)
        {
            viable = false;

            //if (hex.CanIpass(selectedUnit))
            if(hex.RepresentsPossibleMoveTo(selectedUnit))
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

                if (hex.distanceFrom == selectedUnit.GetMobility())
                {

                    if (Navigation.HasDistance(selectedUnit.GetUnitType(), hex.myCoords, knowledge.GetPlayerBaseLoc()))
                    {
                        //re-use distances already computed
                        destDistance = Navigation.GetDistance(selectedUnit.GetUnitType(), hex.myCoords, knowledge.GetPlayerBaseLoc());
                    }
                    else { 
                        timeForMeasure = Time.realtimeSinceStartup;
                        yield return Ruler.BeginMeasurement(selectedUnit, knowledge.GetPlayerBaseLoc(), hex.myCoords);
                        destDistance = Ruler.GetMeasuredDistance();
                        Navigation.LogDistance(selectedUnit.GetUnitType(), hex.myCoords, knowledge.GetPlayerBaseLoc(), destDistance);
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
                    if(noob.getSmartness() >= Directives[0].getSmartness())
                    {
                        if(noob.getSmartness() > Directives[0].getSmartness())
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
        timeForTurn = Time.realtimeSinceStartup - timeForTurn;
        //Debug.Log("It Took " + timeForTurn + " seconds to make a move.");
        //Debug.Log(timeSpentMeasuring + " seconds were spent measuring distances");
        //Debug.Log("A total of " + numberOfMeasurements + " measurements were made");
        //Debug.Log("The average measurement took " + (timeSpentMeasuring / numberOfMeasurements) + " seconds to compute");
        //return possibilities;
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
