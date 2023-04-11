using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UniversalConstants;

public abstract class Unit : MonoBehaviour, ISelectable
{

    [SerializeField] protected Faction allegiance = Faction.PlayerTeam;

    [SerializeField] private SpriteRenderer[] colourableSprites;

    [SerializeField] public int mobility { get; protected set; } = 5;

    [SerializeField] private string unitName;
    [TextArea][SerializeField] private string unitDescription;

    private Tilemap map;

    public Vector3Int myTilePos { get; protected set; }

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


    //Called when the unit is attacked to handle how it reacts
    public abstract void BeEngaged(Unit assailant);


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
        map.GetInstantiatedObject(myTilePos).GetComponent<HexOverlay>().SetOccupiedBy(this);
        transform.position = map.GetCellCenterWorld(myTilePos);
    }

    public string GetTitle()
    {
        return unitName;
    }
    public string GetDescription()
    {
        return unitDescription;
    }

    public List<HexOverlay> OnSelected()
    {
        //This method is only relavent for the player's units
        if(this.allegiance == UniversalConstants.Faction.PlayerTeam)
        {
            return map.GetInstantiatedObject(this.myTilePos).GetComponent<HexOverlay>().BeginExploration(this);
        }
        else
        {
            //return an empty list
            return new List<HexOverlay>();
        }
    }
}
