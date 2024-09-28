using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
using static UniversalConstants;
using static AITacticalValues;

public abstract class Unit : MonoBehaviour, ISelectable
{
    //This value acts as a failsafe for order execution
    protected const int MAXFRAMESPERACTION = 60;


    [SerializeField] protected Faction allegiance = Faction.PlayerTeam;

    [SerializeField] private SpriteRenderer[] colourableSprites;

    [SerializeField] protected GameObject availSprite;
    [SerializeField] protected GameObject staleSprite;

    //This is the sprite all other sprites making up the unit are parented to
    [SerializeField] protected GameObject baseSprite;

    [SerializeField] protected int mobility = 5;
    [SerializeField] protected MovementType Movement;
    [SerializeField] protected GameObject deathEffect;
    protected Animator puppeteer = null;

    //Sounds
    [SerializeField] protected AudioClip attackSound;
    protected AudioSource soundMaker;


    [SerializeField] private string unitName;
    [TextArea] [SerializeField] private string unitDescription;


    protected Mission currMission;


    protected Tilemap map;
    protected GameManager manager;
    protected MilitaryManager militaryManager;


    protected Stack<Order> orders = new Stack<Order>();

    protected float speed = 8.0f;
    protected float turnSpeed = 225.0f;

    protected Order currOrder = null;
    protected bool orderComplete = false;
    protected bool executingOrders = false;
    protected bool ordersComplete = false;
    public bool actionReported { get; protected set; } = true;

    private bool retaliation = false;

    protected IEnumerator execution;

    //public bool hasAlreadyMadeAction { get; protected set; } = false;
    public UnitState myState { get; private set; } = UnitState.tired;


    protected Unit stalematedWith;


    public Vector3Int myTilePos { get; protected set; }

    public Vector3Int prevTilePos { get; protected set; }

    //Set to true when the unit is expecting retaliation
    public bool waitingForResponse { get; protected set; } = false;

    //This is the tactical value of destroying this unit
    public int bounty { get; protected set; } = 0;

    public bool AffectorsAreCurrent { get; protected set; } = false;

    protected Dictionary<int, HexAffect> Affectors = new Dictionary<int, HexAffect>();


    private void Awake()
    {
        Enlist();
        PutOnBoard();
    }


    // Start is called before the first frame update
    virtual public void Start()
    {

        
        PaintUnit();
        soundMaker = gameObject.GetComponent<AudioSource>();
        puppeteer = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FollowOrders();
    }


    //Called to make this unit attack the other unit
    //POLYMORPHISM
    public abstract void ResolveCombat(Unit other);

    public abstract UnitType GetUnitType();


    //returns true if attacking the unit foe will result in a loss
    //POLYMORPHISM
    public abstract bool IsWeakTo(Unit foe);


    //Called when the unit is attacked to handle how it reacts
    //POLYMORPHISM
    public abstract void BeEngaged(Unit assailant);



    //INHERITANCE
    protected virtual void Die()
    {
        
        HexOverlay hex = map.GetInstantiatedObject(myTilePos).GetComponent<HexOverlay>();
        hex.SetOccupiedBy(null);
        //manager.ReportDeath(this);
        militaryManager.RemoveUnitFrom(this);
        if (!actionReported)
        {
            manager.ReportActionComplete(this);
        }

        if(deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, gameObject.transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }


    public Faction GetAllegiance()
    {
        return allegiance;
    }


    public HexOverlay GetOccupiedHex()
    {
        return map.GetInstantiatedObject(myTilePos).GetComponent(typeof(HexOverlay)) as HexOverlay;
    }

    public MovementType GetMovementType()
    {
        return Movement;
    }


    //INHERITANCE
    //Called during start, this method will change the unit to match its team's colour
    protected void PaintUnit()
    {
        foreach (SpriteRenderer pic in colourableSprites)
        {
            pic.color = UniversalConstants.TeamColours.GetValueOrDefault(this.allegiance);
        }
    }

    //INHERITANCE
    //The unit uses its own global position to add itself to the game board
    protected void PutOnBoard()
    {
        map = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<Tilemap>();
        myTilePos = map.WorldToCell(gameObject.transform.position);
        prevTilePos = myTilePos;
        map.GetInstantiatedObject(myTilePos).GetComponent<HexOverlay>().SetOccupiedBy(this);
        transform.position = map.GetCellCenterWorld(myTilePos);
    }

    //INHERITANCE
    //called by Start, this method initializes the manager property and adds the unit to the manager's unit list
    protected void Enlist()
    {
        manager = GameObject.Find(MANAGERPATH).GetComponent<GameManager>();
        militaryManager = GameObject.Find(MANAGERPATH).GetComponent<MilitaryManager>();
        militaryManager.AddUnit(this);
        manager.ReportForDuty(this);
    }





    
    //The parameter conspicuous indicates if the changes to the gameboard's state should be shown
    public List<HexOverlay> OnSelected(bool conspicuous = true)
    {

        if(this.allegiance == manager.WhosTurn() && myState == UnitState.ready)
        //if (manager.WhosTurn() == Faction.PlayerTeam)
        {
            List<HexOverlay> hexes = new List<HexOverlay>();

            ShowEffectOnBoard( conspicuous && manager.WhosTurn() == Faction.PlayerTeam);

            foreach(HexAffect affect in Affectors.Values)
            {
                
                
                hexes.Add(affect.Hex);
            }
            return hexes;
            //return map.GetInstantiatedObject(this.myTilePos).GetComponent<HexOverlay>().BeginExploration(this, conspicuous);
        }
        else
        {
            //return an empty list
            return new List<HexOverlay>();
        }
    }


    public List<HexOverlay> TacticalAnalysis(bool conspicuous)
    {
        List<HexOverlay> affected = new List<HexOverlay>();

        
        this.ShowEffectOnBoard(conspicuous);

        foreach(HexAffect affect in Affectors.Values)
        {
            affected.Add(affect.Hex);
        }

        return affected;
    }

    
    public void BeginMission(Mission newMission)
    {
        this.currMission = newMission;
    }


    public void RecieveOrders(Stack<Order> commands)
    {
        
        this.orders = commands;
    }


    //ABSTRACTION
    public virtual IEnumerator ExecuteMoveOrder(Vector3 origin, Vector3 destination)
    {
        Vector3 direction = destination - origin;
        float travel = this.speed * Time.deltaTime;
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        if (!soundMaker.isPlaying)
        {
            soundMaker.Play();
        }

        int count = 1;

        //If we can make it to the destination this frame we just set our location instead
        while((travel * travel) <= (destination - gameObject.transform.position).sqrMagnitude && count < MAXFRAMESPERACTION)
        {
            gameObject.transform.position += direction.normalized * travel;

            //We only rotate the base sprite so the availSprite remains aligned with the grid
            Vector3 angles = baseSprite.transform.eulerAngles;
            angles.z = Vector3.SignedAngle(Vector3.right, direction, Vector3.forward);
            baseSprite.transform.eulerAngles = angles;

            yield return wait;
            direction = destination - transform.position;
            travel = this.speed * Time.deltaTime;
            count++;
        }

        if (count >= MAXFRAMESPERACTION)
        {
            Debug.Log("!ERROR! Move order completed through exceeding maximum frame tolerance");
        }

        gameObject.transform.position = destination;

        orderComplete = true;
        
    }


    //ABSTRACTION
    public virtual IEnumerator ExecuteAttackOrder(Vector3 origin, Vector3 destination)
    {
        Vector3 displacement = destination - origin;
        Vector3 heading = baseSprite.transform.eulerAngles;
        float turnBy = Time.deltaTime * this.turnSpeed;
        float bearing = Vector3.SignedAngle(Vector3.right, displacement, Vector3.forward);
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        //Attack orders always come at the end of a stack so we should update the board ASAP
        this.FinalizeMovement();
        

        if (bearing < 0)
        {
            bearing += 360;
        }

        float direction = Vector3.SignedAngle(baseSprite.transform.TransformDirection(Vector3.right), displacement, Vector3.forward);

        direction = Mathf.Sign(direction);

        int count = 1;

        while((turnBy < Mathf.Abs(heading.z - bearing)) && count < MAXFRAMESPERACTION)
        {
            baseSprite.transform.Rotate(Vector3.forward, direction * turnBy);
            yield return wait;
            turnBy = Time.deltaTime * this.turnSpeed;
            heading = baseSprite.transform.eulerAngles;
            count++;
        }

        if(count >= MAXFRAMESPERACTION)
        {
            Debug.Log("!ERROR! Attack order completed through exceeding maximum frame tolerance");
        }

        baseSprite.transform.eulerAngles = new Vector3(0, 0, bearing);
        HexOverlay hex = map.GetInstantiatedObject(map.WorldToCell(destination)).GetComponent<HexOverlay>();
        Unit target = hex.GetOccupiedBy();

        //Play attack animation
        if (puppeteer != null)
        {
            puppeteer.SetTrigger("Attack");
            soundMaker.PlayOneShot(attackSound);
            yield return new WaitForSeconds(0.5f);
        }

        this.ResolveCombat(target);

        //wait for a few frames to ensure that the other unit has had time to handle being attacked
        for(int i = 0; i < 3; i++)
        {
            yield return null;
        }
        
        orderComplete = true;
    }


    //process the order to do nothing
    public virtual IEnumerator ExecuteHoldOrder()
    {
        WaitForSeconds wait = new WaitForSeconds(0.25f);

        yield return wait;

        yield return null;
        orderComplete = true;
    }



    protected void Retaliate(Unit assailant)
    {
        this.retaliation = true;

        AttackOrder retaliation = new AttackOrder(transform.position, assailant.gameObject.transform.position, this);

        //New Lines are below

        Mission retaliate = new Mission(this, map);
        retaliate.AddOrder(retaliation);
        this.BeginMission(retaliate);
        //Old code below

        //this.orders.Push(retaliation);
    }


    //INHERITANCE
    protected void FollowOrders()
    {
        if (currMission != null || currOrder != null)
        {
            if (!currMission.IsComplete() || currOrder != null)
            {
                if (!executingOrders)
                {

                    manager.ReportActionStarted();

                    executingOrders = true;
                    actionReported = false;
                    availSprite.SetActive(false);
                }


                //There are orders to follow
                if (currOrder == null)
                {
                    currOrder = currMission.ReceiveOrder();
                    execution = currOrder.Execute();
                    StartCoroutine(execution);
                }

                if (orderComplete)
                {
                    currOrder = null;
                    orderComplete = false;
                }

            }
            else if (executingOrders && !waitingForResponse)
            {
                actionReported = true;
                executingOrders = false;
                FinalizeMovement();
                FinalizeAction();

                //We delay reporting the action as complete if we are expecting a response from another unit
                manager.ReportActionComplete(this);

            }
        }
    }





    //This method updates this units data to reflect changes made when an action was performed
    protected void FinalizeAction()
    {
        //If the sequence of attacks was retaliation we don't tire
        if(myState != UnitState.stalemate && !retaliation)
        {
            SetStateTired();
        }
        retaliation = false;

    }


    //After the unit moves update the game board to reflect that
    protected void FinalizeMovement()
    {
        //Reset the the tile this unit was at
        map.GetInstantiatedObject(myTilePos).GetComponent<HexOverlay>().SetOccupiedBy(null);
        soundMaker.Stop();

        prevTilePos = myTilePos;
        //Update the tile this unit is now on
        myTilePos = map.WorldToCell(gameObject.transform.position);
        map.GetInstantiatedObject(myTilePos).GetComponent<HexOverlay>().SetOccupiedBy(this);


        transform.position = map.GetCellCenterWorld(myTilePos);
    }



    //This method sets the unit as being ready to move this turn
    virtual public bool Revitalize()
    {
        bool ready = myState != UnitState.stalemate;
        this.bounty = 0;
        if (ready)
        {
            SetStateReady();
        }
        return ready;
    }


    //This method sets the value of bounty
    public void SetBounty(List<Directive> directives)
    {
        int highest = 0;
        int candidate = 0;

        if(directives.Count > 0)
        {
            foreach (Directive dir in directives)
            {
                if(dir.directiveType == HexState.attackable)
                {
                    Unit target = dir.GetOccupant();
                    if (target.IsWeakTo(this))
                    {
                        if (target.GetUnitType() == UnitType.InfantrySquad)
                        {
                            candidate = CAN_DESTROY_INFANTRY;
                        }
                        else
                        {
                            candidate = CAN_DESTROY_VEHICLE;
                        }
                    }

                }
                else if(dir.directiveType == HexState.capture){
                    candidate = CAN_CAPTURE_BASE;
                }

                highest = Mathf.Max(candidate, highest);
            }
        }


        bounty = highest;
    }


    public void NoticeBoardChange()
    {
        AffectorsAreCurrent = false;
    }


    public void ShowEffectOnBoard(bool conspicuous)
    {
        if (!AffectorsAreCurrent)
        {
            DetermineEffectOfUnit();
        }

        foreach(HexAffect affect in Affectors.Values)
        {
            affect.Restore(conspicuous);
        }
    }


    private void ClearAffectors()
    {
        foreach(HexAffect affect in Affectors.Values)
        {
            affect.Hex.NotifyUnaffectedBy(this);

        }

        Affectors.Clear();
    }


    virtual protected void DetermineEffectOfUnit()
    {
        ClearAffectors();
        DetermineMoveOptions();
        DetermineAttackOptions();
        AffectorsAreCurrent = true;
    }



    /*
     * Can Take Mission To
     * 
     * This method returns true if the Hexoverlay parameter here is one that would trigger this unit to move
     * upon being clicked with this unit selected. In technical terms, the parameter "here" must be a valid final
     * hex for a mission object; or in other words the goal or destination for a move
     * 
     * This method will return false if this unit is not "ready"
     * This method will update this unit's affectors if they are not up to date
     */
    public bool CanTakeMissionTo(HexOverlay here)
    {
        bool can = false;

        if(this.myState == UnitState.ready)
        {
            if (!AffectorsAreCurrent)
            {
                DetermineEffectOfUnit();
            }

            int key = GridHelper.HashGridCoordinates(here.myCoords);

            if (Affectors.ContainsKey(key))
            {
                can = !(Affectors[key].RecordedState == HexState.unreachable || (Affectors[key].RecordedState == HexState.range));
            }
        }

        return can;
    }

    /*
     * This method will find the hexes that this unit can move to and create HexAffect objects for them.
     * These objects will then be added to the dictionary of effectors for future reference
     * 
     * 
     */
    virtual protected void DetermineMoveOptions()
    {
        HexOverlay start = this.GetOccupiedHex();

        HashSet<HexOverlay> explored = new HashSet<HexOverlay>();
        HashSet<HexOverlay> unexplored = new HashSet<HexOverlay>();
        HashSet<HexOverlay> discovered = new HashSet<HexOverlay>();
        unexplored.Add(start);
        Affectors.Add(GridHelper.HashGridCoordinates(start.myCoords), new HexAffect(this, start, HexState.hold, 0));

        int itter = 0;

        do
        {
            
            foreach(HexOverlay hex in discovered)
            {
                if (hex.CanIpass(this))
                {
                    unexplored.Add(hex);
                }
            }


            foreach(HexOverlay hex in unexplored)
            {
                if (discovered.Contains(hex))
                {
                    discovered.Remove(hex);
                }


                int dist = Affectors[GridHelper.HashGridCoordinates(hex.myCoords)].DistanceFrom;
                //check the adjacent hexes
                foreach (HexOverlay adj in hex.adjacent)
                {
                    if (Affectors.ContainsKey(GridHelper.HashGridCoordinates(adj.myCoords)))
                    {
                        Affectors[GridHelper.HashGridCoordinates(adj.myCoords)].ChangeDistance(dist + 1);
                    }
                    else
                    {
                        if (adj.CanIpass(this) && dist < this.mobility)
                        {
                            Affectors.Add(GridHelper.HashGridCoordinates(adj.myCoords), new HexAffect(this, adj, HexState.reachable, dist +1));
                            discovered.Add(adj);
                        }
                        
                    }
                    
                    
                }

                explored.Add(hex);
            }

            foreach(HexOverlay hex in explored)
            {
                if (unexplored.Contains(hex))
                {
                    unexplored.Remove(hex);
                }
            }


            itter++;
        } while (discovered.Count > 0 && itter < 100);

        if(itter >= 100)
        {
            Debug.Log("Movement search exited through exceeding maximum itterations");
        }

    }



    /*
     * Determine Attack Options
     * 
     * This method will find out which hexes this unit can attack particularly in close range combat
     * This method requires that the hexes the unit can move to are already known
     */
    protected virtual void DetermineAttackOptions()
    {
        HashSet<HexOverlay> moves = new HashSet<HexOverlay>();

        foreach(HexAffect move in Affectors.Values)
        {
            move.MutateAttackable();
            moves.Add(move.Hex);
            
        }


        foreach(HexOverlay hex in moves)
        {
            int dist = Affectors[GridHelper.HashGridCoordinates(hex.myCoords)].DistanceFrom;

            if (hex.CanIBeOn(this))
            {
                foreach (HexOverlay adj in hex.adjacent)
                {
                    if (!Affectors.ContainsKey(GridHelper.HashGridCoordinates(adj.myCoords)))
                    {
                        //If it is a tile adjacent to where we can move than it is a tile this unit can attack
                        bool attackable = false;

                        if (adj.GetOccupiedBy() != null)
                        {
                            attackable = adj.GetOccupiedBy().GetAllegiance() != this.GetAllegiance();
                        }
                        adj.intel.AffectedBy(this);

                        if (attackable)
                        {
                            Affectors.Add(GridHelper.HashGridCoordinates(adj.myCoords), new HexAffect(this, adj, HexState.attackable, dist + 1, false, true));
                        }
                        else
                        {
                            Affectors.Add(GridHelper.HashGridCoordinates(adj.myCoords), new HexAffect(this, adj, HexState.range, dist + 1, false, true));
                        }
                    }
                    else
                    {
                        HexAffect affect = Affectors[GridHelper.HashGridCoordinates(adj.myCoords)];

                        if (affect.RecordedState == HexState.attackable || affect.RecordedState == HexState.range)
                        {
                            affect.ChangeDistance(dist + 1);
                        }
                    }


                }
            }
            
            
        }

        //check for hexes that can't be reached because of an allied unit
        foreach(HexOverlay hex in moves)
        {
            if (!hex.CanIBeOn(this))
            {
                if (hex.GetOccupiedBy() != null)
                {
                    //If a hex got into the movement, than we can move on it, but not be on it, than it must be because an allied unit is on it

                    foreach (HexOverlay adj in hex.adjacent)
                    {
                        if (!Affectors.ContainsKey(GridHelper.HashGridCoordinates(adj.myCoords)))
                        {
                            Affectors.Add(GridHelper.HashGridCoordinates(adj.myCoords), new HexAffect(this, adj, HexState.unreachable, hex.distanceFrom + 1));
                        }
                    }
                }
            }
        }
    }






    //Called by the unit this unit is in stalemate with
    public void StalemateResolved()
    {
        
        this.stalematedWith = null;
        this.SetStateTired();
    }

    public void EnterStalemate(Unit blocker) 
    {
        
        this.stalematedWith = blocker;
        SetStateStalemate();
    }




    // State setters
    protected void SetStateTired()
    {
        this.myState = UnitState.tired;
        availSprite.SetActive(false);
        staleSprite.SetActive(false);
    }


    protected void SetStateReady()
    {
        this.myState = UnitState.ready;
        if(allegiance == Faction.PlayerTeam)
        {
            availSprite.SetActive(true);
        }
        staleSprite.SetActive(false);
    }


    protected void SetStateStalemate()
    {
        myState = UnitState.stalemate;
        availSprite.SetActive(false);
        staleSprite.SetActive(true);
    }

    protected void SetStateRetaliating()
    {
        myState = UnitState.retaliating;
        availSprite.SetActive(false);
        staleSprite.SetActive(false);
    }

    



  



    // Accessors/ Getters
    public string GetTitle()
    {
        return unitName;
    }
    public string GetDescription()
    {
        return unitDescription;
    }

    public bool IsInStalemate()
    {
        return this.stalematedWith != null;
    }

    public int GetMobility()
    {
        return this.mobility;
    }


    //Used for debugging
    public Vector3Int GetCurrentTilePos()
    {
        Vector3Int realTilePos = map.WorldToCell(gameObject.transform.position);
        return realTilePos;
    }


    //This method is overridden by units capable of capturing buildings
    //Most unit can't capture buildings so this default method can stay in most cases
    public virtual bool IsCapturing()
    {
        return false;
    }


    public override string ToString()
    {
        string label = this.gameObject.name;

        label += "(" + this.GetUnitType() + ") at " + this.myTilePos;

        return label;
    }


}
