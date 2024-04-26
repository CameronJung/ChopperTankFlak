using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;
using static GridHelper;


//When the player selects a unit this class begins following the mouse movements and starts drawing a path
public class CommandTracer : MonoBehaviour
{

    [SerializeField] private Camera cam;

    [SerializeField] private Tilemap map;

    [SerializeField] private GameManager manager;

    private LineRenderer liner;

    private Vector3Int currTilePos = new Vector3Int(0, 0, 100);
    private Vector3Int prevTilePos = new Vector3Int(0, 0, 100);

    //This boolean flag determines if the command should be drawn
    public bool drawing { get; private set; } = false;

    

    //indicates if the tile hovered over displays a valid move for the commandee
    private bool validTile = false;
    private bool prevValidity = false;
    
    private Unit commandee = null;
    private Vector3[] points;
    //This is the index of the last order
    private int endPoint = 0;
    private int prevEnd = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        liner = gameObject.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (drawing && Input.mousePresent && manager.WhosTurn() == Faction.PlayerTeam)
        {
            Vector3Int rawPos = map.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));
            rawPos.z = 0;

            HandleMouseAtTile(rawPos);

            
            
        }
        
    }



    private void HandleMouseAtTile(Vector3Int mouseTilePos)
    {
        currTilePos = mouseTilePos;


        if (currTilePos != prevTilePos && map.HasTile(currTilePos))
        {
            HexOverlay currHex = map.GetInstantiatedObject(currTilePos).GetComponent<HexOverlay>();
            if (currHex.currState > 0)
            {
                prevValidity = validTile;
                validTile = currHex.currState == HexState.attackable || currHex.GetOccupiedBy() == null
                        || currHex.currState == HexState.hold || currHex.currState == HexState.capture;


                liner.enabled = true;

                



                
                prevEnd = endPoint;

                if (currHex.currState == HexState.attackable)
                {
                    if (prevEnd + 1 < points.Length)
                    {
                        //If we are attacking and there is room for the additional tile than we try putting it after the previous endpoint
                        points[prevEnd +1] = currHex.transform.position;
                        endPoint = prevEnd + 1;
                    }
                    else
                    {
                        //if there is no room than we replace the previous endpoint position and push the prevEndpoint back
                        points[prevEnd] = currHex.transform.position;

                    }
                }
                else
                {
                    points[currHex.distanceFrom] = map.GetCellCenterWorld(currTilePos);
                    endPoint = currHex.distanceFrom;
                }
                liner.positionCount = endPoint + 1;



                liner.SetPositions(points);
                
                //endPoint = currHex.distanceFrom;

                if (!CheckLineValidity(endPoint))
                {
                    RedrawLine();
                    liner.positionCount = endPoint + 1;
                }
                liner.SetPositions(points);
            }
            else
            {
                liner.enabled = false;
                validTile = false;
            }
            prevTilePos = currTilePos;
        }
    }



    public void StartDrawingCommand(Unit unit)
    {
        if (unit.GetAllegiance() == manager.WhosTurn() && (unit.myState == UnitState.ready))
        {
            //The most tiles any unit could have will be mobility + 2
            points = new Vector3[unit.GetMobility() + 2];
            points[0] = map.GetCellCenterWorld(unit.myTilePos);
            commandee = unit;

            //Only show the line for player controlled units
            liner.enabled = unit.GetAllegiance() == Faction.PlayerTeam;
            drawing = true;
        }
        
    }

    public void StopDrawingCommand()
    {
        if (drawing)
        {
            
            drawing = false;
            commandee = null;
            liner.enabled = false;
        }
        
    }



    //This method checks the current path in positions to see if it is valid
    private bool CheckLineValidity(int endDist)
    {
        bool valid = true;
        bool prevWasAttack = false;

        for(int idx = endDist; idx > 0; idx--)
        {
            valid = valid && GridHelper.CoarseAdjacencyTest(points[idx], points[idx-1]);
            HexOverlay hex = map.GetInstantiatedObject(map.WorldToCell(points[idx])).GetComponent<HexOverlay>();

            if (prevWasAttack && valid)
            {
                //If the previous tile was an attack then that means the unit stops at this tile and thus needs to be empty
                valid = valid && (hex.GetOccupiedBy() == null || hex.GetOccupiedBy() == commandee);
            }

            if(idx < endDist && valid)
            {
                //We shouldn't draw paths through enemy units
                valid = valid && hex.currState != HexState.attackable;
            }

            prevWasAttack = hex.currState == HexState.attackable;
        }

        return valid;
    }





    /*
     * SalvageLine
     * 
     * this method will attempt to place the currtile into the points array such that it will preserve as much of the line as possible
     */
    private void SalvageLine(HexOverlay noobHex, HexOverlay oldeHex)
    {
        if(noobHex.currState == HexState.attackable)
        {
            if(prevEnd + 1 < points.Length)
            {
                //If we are attacking and there is room for the additional tile than we try putting it after the previous endpoint
            }
        }



    }




    /*
     * This method redraws the line from the from the commandee's position to the currently selected tile
     * !NOTE! both the currTile and endPoint values must be correct for this method to work
     * 
     * PSEUDO CODE GLOSSARY
     * 
     * CURR TILE IDX: the index of the curr tile's
     * ATTACK POSITION: the tile that commandee will be on when it attacks the target on the current tile
     * DIRECT DESTINATION: the tile that a path of tiles with sequential distanceFrom values will be drawn to
     * DIRECT DESTINATION DISTANCE: The distance from value of the direct destination
     */
    private void RedrawLine()
    {
        
        //  Record the curr tile as the direct destination
        HexOverlay currTile = map.GetInstantiatedObject(currTilePos).GetComponent<HexOverlay>();
        HexOverlay directDestination = map.GetInstantiatedObject(currTilePos).GetComponent<HexOverlay>();
        int currTileIdx = endPoint;
        int directDestinationIdx = directDestination.distanceFrom;
        
        // IF (The proposed mission is to attack)
        if (directDestination.currState == HexState.attackable)
        {


            //set the tile before the curr tile as the attack position
            HexOverlay attackPosition = map.GetInstantiatedObject(map.WorldToCell(points[currTileIdx - 1])).GetComponent<HexOverlay>();

            //IF(The tile before the curr Tile is NOT a valid position to attack from)
            if (!(attackPosition.CanIBeOn(commandee) && attackPosition.currState != HexState.unreachable
                && CoarseAdjacencyTest(points[currTileIdx], points[currTileIdx - 1])))
            {
                //Find a valid attack position adjacent to the curr tile
                attackPosition = currTile.FindValidNeighborFor(commandee);
                
            }


            //place the new attack position in its appropriate spot in the array
            points[attackPosition.distanceFrom] = attackPosition.gameObject.transform.position;

            //place the curr tile in the position after the attack position
            points[attackPosition.distanceFrom + 1] = currTile.gameObject.transform.position;
            //record the curr tile distance
            currTileIdx = attackPosition.distanceFrom + 1;

            //AT THIS POINT THE POSITION BEFORE THE CURRENT TILE IS A VALID ATTACK POSITION

            //set the direct destination as the attack position
            directDestination = attackPosition;
            //update the direct destination distance
            directDestinationIdx = attackPosition.distanceFrom;
            
        }

        // make a recursive call to plot a direct path from the direct destination to the commandee's position
        points = directDestination.MakeDirectPathToHere(points, directDestinationIdx, commandee);
        endPoint = currTileIdx;

        // assert that the line is valid to the curr tile distance
        bool success = CheckLineValidity(currTileIdx);
        if (!success)
        {
            
            string nubPath = map.WorldToCell(points[0]).ToString();


            for (int idx = 0; idx < points.Length; idx++)
            {
                
                nubPath += " -> " + map.WorldToCell(points[idx]);
            }

            Debug.LogWarning("The line redraw method made this invalid line: " + nubPath);
        }
    }




    //This function translates the points array into a list of orders that will be sent to the selected unit
    public void SendCommand()
    {
        if (validTile)
        {
            if (!CheckLineValidity(endPoint))
            {

                string lineData = "";

                for (int i = 0; i < points.Length; i++)
                {
                    lineData += map.WorldToCell(points[i]) + " -> ";
                }

                Debug.Log("!Error! command was sent with invalid line: " + lineData);
            }
            Mission mission = new Mission(commandee, map);



            if (commandee.myTilePos == currTilePos)
            {
                mission.AddOrder(new HoldOrder(points[0], points[0], commandee));
            }
            else
            {
                //Check if the last order is an attack
                bool attacks = map.GetInstantiatedObject(currTilePos).GetComponent<HexOverlay>().currState == HexState.attackable;

                for (int idx = endPoint; idx >= 1; idx--)
                {
                    if (idx == endPoint)
                    {
                        if (attacks)
                        {
                            mission.AddOrder(new AttackOrder(points[idx - 1], points[idx], this.commandee));
                        }
                        else
                        {
                            //Add an extra hold command at the end
                            if ((map.GetInstantiatedObject(map.WorldToCell(points[endPoint])).GetComponent(typeof(HexOverlay)) as HexOverlay).currState == HexState.capture)
                            {
                                mission.AddOrder(new HoldOrder(points[endPoint], points[endPoint], commandee));
                            }

                            mission.AddOrder(new MoveOrder(points[endPoint - 1], points[endPoint], this.commandee));
                        }

                    }
                    else
                    {
                        mission.AddOrder(new MoveOrder(points[idx - 1], points[idx], this.commandee));
                    }

                }
            }


            endPoint = 0;

            

            commandee.BeginMission(mission);
            
        }
    }





    //This method sets up the command tracer as though the mouse moved to a specific tile
    //This allows the code to be used by the AI
    public void SpoofSendCommand(Vector3Int fakeMouseTile)
    {
        prevTilePos = new Vector3Int(0, 0, -100);
        HexOverlay lie = map.GetInstantiatedObject(fakeMouseTile).GetComponent<HexOverlay>();

        if(lie.currState == HexState.attackable)
        {
            prevTilePos = lie.FindValidNeighbor();
        }



        HandleMouseAtTile(fakeMouseTile);


        


        SendCommand();
    }
}
