using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;

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

    [SerializeField] private string unitName;

    [TextArea][SerializeField] private string unitDescription;

    

    private Tilemap map;
    protected GameManager manager;

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


    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //Called to make this unit attack the other unit
    public abstract void ResolveCombat(Unit other);

    public abstract UnitType GetUnitType();


    //returns true if attacking the unit foe will result in a loss
    public abstract bool IsWeakTo(Unit foe);


    //Called when the unit is attacked to handle how it reacts
    public abstract void BeEngaged(Unit assailant);

    


    protected void Die()
    {
        
        HexOverlay hex = map.GetInstantiatedObject(myTilePos).GetComponent<HexOverlay>();
        hex.SetOccupiedBy(null);
        manager.ReportDeath(this);
        if (!actionReported)
        {
            manager.ReportActionComplete(this);
        }
        Destroy(gameObject);
    }


    public Faction GetAllegiance()
    {
        return allegiance;
    }



    //Called during start, this method will change the unit to match its team's colour
    protected void PaintUnit()
    {
        foreach (SpriteRenderer pic in colourableSprites)
        {
            pic.color = UniversalConstants.TeamColours.GetValueOrDefault(this.allegiance);
        }
    }

    //The unit uses its own global position to add itself to the game board
    protected void PutOnBoard()
    {
        map = GameObject.Find(UniversalConstants.MAPPATH).GetComponent<Tilemap>();
        myTilePos = map.WorldToCell(gameObject.transform.position);
        prevTilePos = myTilePos;
        map.GetInstantiatedObject(myTilePos).GetComponent<HexOverlay>().SetOccupiedBy(this);
        transform.position = map.GetCellCenterWorld(myTilePos);
    }

    //called by Start, this method initializes the manager property and adds the unit to the manager's unit list
    protected void Enlist()
    {
        manager = GameObject.Find(MANAGERPATH).GetComponent<GameManager>();
        manager.ReportForDuty(this);
    }







    public List<HexOverlay> OnSelected()
    {
        //This method is only relavent for the player's units
        if(this.allegiance == manager.WhosTurn() && myState == UnitState.ready)
        {
            return map.GetInstantiatedObject(this.myTilePos).GetComponent<HexOverlay>().BeginExploration(this);
        }
        else
        {
            //return an empty list
            return new List<HexOverlay>();
        }
    }

    public void RecieveOrders(Stack<Order> commands)
    {
        
        this.orders = commands;
    }

    public IEnumerator ExecuteMoveOrder(Vector3 origin, Vector3 destination)
    {
        Vector3 direction = destination - origin;
        float travel = this.speed * Time.deltaTime;
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

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


    //This is a default implementation viable as a replacement
    public IEnumerator ExecuteAttackOrder(Vector3 origin, Vector3 destination)
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

        this.ResolveCombat(target);

        //wait for a few frames to ensure that the other unit has had time to handle being attacked
        for(int i = 0; i < 3; i++)
        {
            yield return null;
        }
        
        orderComplete = true;
        
        
    }


    //process the order to do nothing
    public IEnumerator ExecuteHoldOrder()
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
        this.orders.Push(retaliation);
    }


    protected void FollowOrders()
    {
        
        if (orders.Count > 0 || currOrder != null)
        {
            if (!executingOrders)
            {
                Debug.Log(this.allegiance + " " + this.GetUnitType() + " is now executing orders");
                manager.ReportActionStarted();

                executingOrders = true;
                actionReported = false;
                availSprite.SetActive(false);
            }

            
            //There are orders to follow
            if(currOrder == null)
            {
                currOrder = orders.Pop();
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
            Debug.Log(this.allegiance + " " + this.GetUnitType() + " has completed orders");

            //We delay reporting the action as complete if we are expecting a response from another unit
            manager.ReportActionComplete(this);
            
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

        prevTilePos = myTilePos;
        //Update the tile this unit is now on
        myTilePos = map.WorldToCell(gameObject.transform.position);
        map.GetInstantiatedObject(myTilePos).GetComponent<HexOverlay>().SetOccupiedBy(this);


        transform.position = map.GetCellCenterWorld(myTilePos);
    }



    //This method sets the unit as being ready to move this turn
    public bool Revitalize()
    {
        bool ready = myState != UnitState.stalemate;
        if (ready)
        {
            
            SetStateReady();
        }
        return ready;
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
}
