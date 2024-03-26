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


                /*
                //Special Handeling of attack orders
                //Since an Attack order doesn't have a movement cost it can exceed the unit's mobility and the
                //distance property won't neccesarily be sequential
                */
                if (currHex.currState == HexState.attackable)
                {
                    if(prevTilePos == null)
                    {
                        Debug.Log("previous Tile position is null for some reason");
                    }

                    

                    //If the previous tile was not valid to attack from than set the previous to a valid one
                    if (!prevValidity)
                    {
                        prevTilePos = currHex.FindValidNeighbor();

                    }
                    HexOverlay prevHex = map.GetInstantiatedObject(prevTilePos).GetComponent<HexOverlay>();
                    
                    liner.positionCount = prevHex.distanceFrom + 2;
                    //input the position of the unit we attack
                    points[prevHex.distanceFrom + 1] = map.GetCellCenterWorld(currTilePos);
                    //input the position we want to attack from
                    points[prevHex.distanceFrom] = map.GetCellCenterWorld(prevTilePos);

                    if (!validateLine(prevHex.distanceFrom + 1))
                    {
                        points = currHex.MakePathToHere(commandee, points, prevHex.distanceFrom + 1);

                    }
                    liner.SetPositions(points);
                    endPoint = prevHex.distanceFrom + 1;
                }
                else
                {
                    //Determine if the hex represents a valid move
                    //A hex is invalid if it is occupied by another allied unit
                    
                    liner.positionCount = currHex.distanceFrom + 1;

                    points[currHex.distanceFrom] = map.GetCellCenterWorld(currTilePos);
                    if (!validateLine(currHex.distanceFrom))
                    {
                        points = currHex.MakePathToHere(commandee, points, currHex.distanceFrom);

                    }
                    liner.SetPositions(points);
                    endPoint = currHex.distanceFrom;
                }

                
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
    private bool validateLine(int endDist)
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
                valid = valid && hex.GetOccupiedBy() == null;
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


    //This function translates the points array into a list of orders that will be sent to the selected unit
    public void SendCommand()
    {
        if (validTile)
        {
            
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
            //commandee.RecieveOrders(orders);
            
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
        /*
        if (!validateLine(endPoint))
        {

            string lineData = "";

            for(int i = 0; i < points.Length; i++)
            {
                lineData += map.WorldToCell(points[i]) + " -> ";
            }

            Debug.Log("!Error! command was sent with invalid line: " + lineData);
        }
        */

        SendCommand();
    }
}
